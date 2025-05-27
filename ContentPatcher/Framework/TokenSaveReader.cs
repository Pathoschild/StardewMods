using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Constants;
using Netcode;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.GameData;
using StardewValley.GameData.Characters;
using StardewValley.Locations;
using StardewValley.Network;

namespace ContentPatcher.Framework;

/// <summary>Handles reading info from the current save.</summary>
/// <remarks>This allows reading data from the save immediately after it's parsed, before the save is loaded.</remarks>
internal class TokenSaveReader
{
    /*********
    ** Fields
    *********/
    /// <summary>The number of context updates since the game was launched, including the current one if applicable.</summary>
    private readonly Func<int> GetUpdateTick;

    /// <summary>The backing field for <see cref="IsParsed"/>.</summary>
    private readonly Func<bool> IsParsedImpl;

    /// <summary>The backing field for <see cref="IsBasicInfoLoadedImpl"/>.</summary>
    private readonly Func<bool> IsBasicInfoLoadedImpl;

    /// <summary>The backing field for <see cref="IsLoaded"/>.</summary>
    private readonly Func<bool> IsLoadedImpl;

    /// <summary>A cache of common values fetched during the current context updates.</summary>
    private readonly Dictionary<string, object?> Cache = [];

    /// <summary>The last context update for which cached values were updated.</summary>
    private int LastCacheTick;

    /// <summary>Whether locations have been loaded from the save file into the game world.</summary>
    /// <remarks>When loading a save file, locations aren't loaded yet when <see cref="IsBasicInfoLoaded"/> is first set. However, they are loaded at that point when creating a save.</remarks>
    private bool AreLocationsLoaded => this.IsLoaded || (this.IsBasicInfoLoaded && SaveGame.loaded is null);


    /*********
    ** Accessors
    *********/
    /// <summary>Whether data can be read from the save file now.</summary>
    public bool IsReady => this.IsParsed;

    /// <summary>Whether the save file has been parsed into <see cref="SaveGame.loaded"/> (regardless of whether the game started loading it yet).</summary>
    public bool IsParsed => this.IsParsedImpl();

    /// <summary>Whether the basic save info is loaded (including the date, weather, and player info). The in-game locations and world may not exist yet.</summary>
    public bool IsBasicInfoLoaded => this.IsBasicInfoLoadedImpl();

    /// <summary>Whether the save data has been fully loaded. This happens before the game applies problem fixes, checks for achievements, starts music, etc.</summary>
    public bool IsLoaded => this.IsLoadedImpl();



    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="updateTick">The number of context updates since the game was launched, including the current one if applicable.</param>
    /// <param name="isParsed">Whether the save file has been parsed into <see cref="SaveGame.loaded"/> (regardless of whether the game started loading it yet).</param>
    /// <param name="isBasicInfoLoaded">Whether the basic save info is loaded (including the date, weather, and player info). The in-game locations and world may not exist yet.</param>
    /// <param name="isLoaded">Whether the save data has been fully loaded. This happens before the game applies problem fixes, checks for achievements, starts music, etc.</param>
    public TokenSaveReader(Func<int> updateTick, Func<bool> isParsed, Func<bool> isBasicInfoLoaded, Func<bool> isLoaded)
    {
        this.GetUpdateTick = updateTick;
        this.IsParsedImpl = isParsed;
        this.IsBasicInfoLoadedImpl = isBasicInfoLoaded;
        this.IsLoadedImpl = isLoaded;
    }

    /****
    ** General utilities
    ****/
    /// <summary>Get a player instance.</summary>
    /// <param name="type">The player type.</param>
    public Farmer? GetPlayer(PlayerType type)
    {
        // loaded
        if (this.IsBasicInfoLoaded)
        {
            return type == PlayerType.HostPlayer
                ? Game1.MasterPlayer
                : Game1.player;
        }

        // loading
        if (this.IsParsed)
            return SaveGame.loaded.player; // current == host if they're loading the save file

        return null;
    }

    /// <summary>Get the current player instance.</summary>
    public Farmer? GetCurrentPlayer()
    {
        return this.GetPlayer(PlayerType.CurrentPlayer);
    }

    /// <summary>Get all players in the game, even if they're offline.</summary>
    public IEnumerable<Farmer> GetAllPlayers()
    {
        return this.GetCached("AllPlayers", () =>
        {
            // loaded
            if (this.IsBasicInfoLoaded)
                return Game1.getAllFarmers();

            // loading
            if (this.IsParsed)
                return new[] { SaveGame.loaded.player }.Concat(SaveGame.loaded.farmhands);

            return [];
        });
    }

    /// <summary>Get a player's location.</summary>
    /// <param name="player">The player instance.</param>
    public GameLocation? GetCurrentLocation(Farmer? player)
    {
        return this.GetCached($"CurrentLocation:{player?.UniqueMultiplayerID}", () =>
        {
            // fully loaded
            if (Context.IsWorldReady)
                return player?.currentLocation;

            // save data loaded
            if (this.AreLocationsLoaded)
            {
                return
                    player?.currentLocation
                    ?? (player != null ? this.GetLocationFromName(player.lastSleepLocation.Value) : null); // currentLocation is set later in the save loading process
            }

            // loading
            if (this.IsParsed)
            {
                return player != null
                    ? this.GetLocationFromName(player.lastSleepLocation.Value)
                    : null;
            }

            return null;
        });
    }

    /// <summary>Get the current player's location context.</summary>
    /// <param name="player">The player instance.</param>
    public string GetCurrentLocationContext(Farmer? player)
    {
        return this.GetLocationContext(this.GetCurrentLocation(player));
    }

    /// <summary>Get the player ID who owns the building containing the location.</summary>
    /// <param name="location">The location to check.</param>
    public long? GetLocationOwnerId(GameLocation? location)
    {
        if (location is null)
            return null;

        long? id = this.GetCached($"LocationOwnerId:{location.Name}", () =>
        {
            switch (location)
            {
                // home
                case FarmHouse farmhouse:
                    return farmhouse.owner?.UniqueMultiplayerID;
                case IslandFarmHouse:
                    return this.GetPlayer(PlayerType.HostPlayer)?.UniqueMultiplayerID;

                // cellar
                case Cellar:
                    {
                        // extract cellar number
                        int cellarNumber = 1;
                        if (!location.Name.StartsWith("Cellar") || (location.Name.Length > 6 && !int.TryParse(location.Name[6..], out cellarNumber)))
                            return null;

                        // loaded
                        if (this.IsBasicInfoLoaded)
                        {
                            return Game1.player.team.cellarAssignments.TryGetValue(cellarNumber, out long owner)
                                ? owner
                                : null;
                        }

                        // loading
                        if (this.IsParsed)
                        {
                            foreach (var pair in SaveGame.loaded.cellarAssignments)
                            {
                                if (pair.Key == cellarNumber)
                                    return pair.Value;
                            }
                        }

                        return null;
                    }

                // building/cellar interior
                default:
                    if (!location.IsOutdoors)
                    {
                        IDictionary<GameLocation, long> locationOwners = this.GetBuildingInteriorOwners();
                        if (locationOwners.TryGetValue(location, out long id))
                            return id;
                    }

                    return null;
            }
        });

        return id != 0
            ? id
            : null;
    }


    /****
    ** Date & weather
    ****/
    /// <summary>Get the day of month.</summary>
    public int GetDay()
    {
        // loaded
        if (this.IsBasicInfoLoaded)
            return Game1.dayOfMonth;

        // loading
        if (this.IsParsed)
            return SaveGame.loaded.dayOfMonth;

        return 0;
    }

    /// <summary>Get the name for today's day event (e.g. wedding or festival).</summary>
    public string? GetDayEvent()
    {
        // marriage
        if (SaveGame.loaded?.weddingToday ?? Game1.weddingToday)
            return "wedding";

        // festival
        IDictionary<string, string> festivalDates = Game1.content.Load<Dictionary<string, string>>("Data\\Festivals\\FestivalDates", LocalizedContentManager.LanguageCode.en); // {{DayEvent}} shouldn't be translated
        if (festivalDates.TryGetValue($"{this.GetSeason()}{this.GetDay()}", out string? festivalName))
            return festivalName;

        return null;
    }

    /// <summary>Get the day of week.</summary>
    public DayOfWeek GetDayOfWeek()
    {
        return (this.GetDay() % 7) switch
        {
            1 => DayOfWeek.Monday,
            2 => DayOfWeek.Tuesday,
            3 => DayOfWeek.Wednesday,
            4 => DayOfWeek.Thursday,
            5 => DayOfWeek.Friday,
            6 => DayOfWeek.Saturday,
            _ => 0
        };
    }

    /// <summary>Get the day of week.</summary>
    public uint GetDaysPlayed()
    {
        // loaded
        if (this.IsBasicInfoLoaded)
            return Game1.stats.DaysPlayed;

        // loading
        if (this.IsParsed)
            return (SaveGame.loaded.obsolete_stats ?? SaveGame.loaded.player.stats).DaysPlayed;

        return 0;
    }

    /// <summary>Get the season.</summary>
    public string GetSeason()
    {
        // loaded
        if (this.IsBasicInfoLoaded)
            return Game1.currentSeason;

        // loading
        if (this.IsParsed)
            return SaveGame.loaded.currentSeason;

        return "spring";
    }

    /// <summary>Get the year number.</summary>
    public int GetYear()
    {
        // loaded
        if (this.IsBasicInfoLoaded)
            return Game1.year;

        // loading
        if (this.IsParsed)
            return SaveGame.loaded.year;

        return 0;
    }

    /// <summary>Get the currently defined contexts.</summary>
    public HashSet<string> GetContexts()
    {
        return this.GetCached("Contexts", () =>
        {
            HashSet<string> contexts = [
                LocationContexts.DefaultId,
                LocationContexts.DesertId,
                LocationContexts.IslandId
            ];

            foreach (GameLocation location in this.GetLocations())
                contexts.Add(this.GetLocationContext(location));

            return contexts;
        });
    }

    /// <summary>Get the weather value for a location context.</summary>
    /// <param name="contextName">The name of the location context.</param>
    public string? GetWeather(string contextName)
    {
        // get raw weather
        LocationWeather? model;
        if (this.IsBasicInfoLoaded)
            model = Game1.netWorldState.Value.GetWeatherForLocation(contextName);
        else if (this.IsParsed)
            model = SaveGame.loaded.locationWeather?.TryGetValue(contextName, out LocationWeather? fromSaveData) is true ? fromSaveData : null;
        else
            model = null;

        // normalize value
        string? weather = model?.weather?.Value;
        return weather switch
        {
            Game1.weather_festival => Game1.weather_sunny,
            Game1.weather_wedding => Game1.weather_sunny,
            null => Game1.weather_sunny,
            _ => weather
        };
    }

    /// <summary>Get the time of day.</summary>
    public int GetTime()
    {
        return this.IsBasicInfoLoaded
            ? Game1.timeOfDay
            : 0600;
    }

    /****
    ** Player
    ****/
    /// <summary>Get the letter IDs, mail flags, and world state IDs set for the player.</summary>
    /// <param name="player">The player instance.</param>
    /// <remarks>See mail logic in <see cref="Farmer.hasOrWillReceiveMail"/>.</remarks>
    public IEnumerable<string> GetFlags(Farmer player)
    {
        // get world state IDs
        IEnumerable<string> worldStateIds;
        if (this.IsBasicInfoLoaded)
            worldStateIds = Game1.worldStateIDs ?? [];
        else if (this.IsParsed)
            worldStateIds = SaveGame.loaded.worldStateIDs ?? [];
        else
            worldStateIds = [];

        // get flags
        return player
            .mailReceived
            .Union(player.mailForTomorrow)
            .Union(player.mailbox)
            .Concat(worldStateIds);
    }

    /// <summary>Get the wallet items for the current player.</summary>
    public IEnumerable<string> GetWalletItems()
    {
        Farmer? player = this.GetCurrentPlayer();
        if (player == null)
            yield break;

        if (player.eventsSeen.Contains("2120303"))
            yield return WalletItem.BearsKnowledge.ToString();
        if (player.hasClubCard)
            yield return WalletItem.ClubCard.ToString();
        if (player.hasDarkTalisman)
            yield return WalletItem.DarkTalisman.ToString();
        if (player.canUnderstandDwarves)
            yield return WalletItem.DwarvishTranslationGuide.ToString();
        if (player.HasTownKey)
            yield return WalletItem.KeyToTheTown.ToString();
        if (player.hasMagicInk)
            yield return WalletItem.MagicInk.ToString();
        if (player.hasMagnifyingGlass)
            yield return WalletItem.MagnifyingGlass.ToString();
        if (player.hasRustyKey)
            yield return WalletItem.RustyKey.ToString();
        if (player.hasSkullKey)
            yield return WalletItem.SkullKey.ToString();
        if (player.hasSpecialCharm)
            yield return WalletItem.SpecialCharm.ToString();
        if (player.eventsSeen.Contains("3910979"))
            yield return WalletItem.SpringOnionMastery.ToString();
    }

    /// <summary>Get a player's daily luck value.</summary>
    /// <param name="player">The player instance.</param>
    public double GetDailyLuck(Farmer player)
    {
        // loaded
        if (this.IsBasicInfoLoaded)
            return player.DailyLuck;

        // loading
        if (this.IsParsed)
            return SaveGame.loaded.dailyLuck + (player.hasSpecialCharm ? 0.025f : 0);

        return 0;
    }

    /****
    ** Relationships
    ****/
    /// <summary>Get values for a given player's children.</summary>
    /// <param name="player">The player instance.</param>
    /// <param name="type">The token values to get.</param>
    public IEnumerable<string> GetChildValues(Farmer player, ConditionType type)
    {
        // get children
        List<Child>? children = this.GetCached($"ChildValues:{type}:{player.UniqueMultiplayerID}", () =>
            (this.GetLocationFromName(player.homeLocation.Value) as FarmHouse)?.getChildren()
        );
        if (children == null)
            return [];

        // get values
        Func<Child, string> filter = type switch
        {
            ConditionType.ChildNames => (child => child.Name),
            ConditionType.ChildGenders => (child => child.Gender.ToString()),
            _ => throw new NotSupportedException($"Invalid child token type '{type}', must be one of '{nameof(ConditionType.ChildGenders)}' or '{nameof(ConditionType.ChildNames)}'.")
        };
        return children.Select(filter);
    }

    /// <summary>Get the friendship data for the current player.</summary>
    /// <returns>Returns a list of friendship models for met NPCs, and null for unmet NPCs.</returns>
    public IEnumerable<KeyValuePair<string, Friendship?>> GetFriendships()
    {
        return this.GetCached("Friendships", () =>
        {
            Dictionary<string, NetRef<Friendship>> met =
                this.GetCurrentPlayer()?.friendshipData?.FieldDict
                ?? new();

            return
                (
                    from pair in met
                    select new KeyValuePair<string, Friendship?>(pair.Key, pair.Value.Value)
                )
                .Concat(
                    from name in this.GetSocialVillagerNames()
                    where !met.ContainsKey(name)
                    select new KeyValuePair<string, Friendship?>(name, null)
                );
        });
    }

    /// <summary>Get the name and gender of the player's spouse, if they're married</summary>
    /// <param name="player">The player whose spouse to check.</param>
    /// <param name="name">The spouse name.</param>
    /// <param name="friendship">The friendship data for the relationship.</param>
    /// <param name="gender">The spouse gender.</param>
    /// <param name="isPlayer">Whether the spouse is a player character.</param>
    /// <returns>Returns true if the player's spouse info was successfully found.</returns>
    [SuppressMessage("ReSharper", "VariableHidesOuterVariable", Justification = "This is deliberate.")]
    public bool TryGetSpouseInfo(Farmer player, [NotNullWhen(true)] out string? name, [NotNullWhen(true)] out Friendship? friendship, out Gender gender, out bool isPlayer)
    {
        var data = this.GetCached($"SpouseInfo:{player.UniqueMultiplayerID}", () =>
        {
            // fetch data
            string? name = null;
            Gender gender = Gender.Male;
            bool isPlayer = false;
            if (this.TryGetRawSpouseInfo(player, out Friendship? friendship, out long? spousePlayerId))
            {
                // parse spouse info
                if (spousePlayerId.HasValue)
                {
                    Farmer? spouse = this.GetAllPlayers().FirstOrDefault(p => p.UniqueMultiplayerID == spousePlayerId);
                    if (spouse != null)
                    {
                        name = spouse.Name;
                        gender = spouse.IsMale ? Gender.Male : Gender.Female;
                        isPlayer = true;
                    }
                }
                else
                {
                    NPC? spouse = this.GetAllCharacters().FirstOrDefault(p => p.Name == player.spouse && p.IsVillager);
                    if (spouse != null)
                    {
                        name = spouse.Name;
                        gender = spouse.Gender;
                        isPlayer = false;
                    }
                }
            }
            bool valid = name != null && friendship != null;

            // create cache entry
            return (Name: name, Friendship: friendship, Gender: gender, IsPlayer: isPlayer, IsValid: valid);
        });

        name = data.Name;
        friendship = data.Friendship;
        gender = data.Gender;
        isPlayer = data.IsPlayer;
        return data.IsValid;
    }

    /****
    ** World
    ****/
    /// <summary>Get the current player's selected farm cave type.</summary>
    public FarmCaveType GetFarmCaveType()
    {
        int? choice = this.GetCurrentPlayer()?.caveChoice.Value;
        return choice.HasValue
            ? this.GetEnum(choice.Value, FarmCaveType.None)
            : FarmCaveType.None;
    }

    /// <summary>Get the farm name.</summary>
    public string? GetFarmName()
    {
        return this.GetCurrentPlayer()?.farmName.Value;
    }

    /// <summary>Get the farm type.</summary>
    public string GetFarmType()
    {
        // loaded
        if (this.IsBasicInfoLoaded)
        {
            return Game1.whichFarm == Farm.mod_layout
                ? Game1.whichModFarm?.Id ?? FarmType.Custom.ToString()
                : this.GetEnum(Game1.whichFarm, FarmType.Custom).ToString();
        }

        // loading
        if (this.IsParsed)
        {
            string farmType = SaveGame.loaded.whichFarm;
            if (Enum.TryParse(farmType, out FarmType parsed))
                farmType = parsed.ToString();
            return farmType;
        }

        return FarmType.Standard.ToString();
    }

    /// <summary>Get the farm's asset name relative to the game's <c>Content/Maps</c> folder.</summary>
    public string GetFarmMapAssetName()
    {
        // loaded
        if (this.IsBasicInfoLoaded)
            return Farm.getMapNameFromTypeInt(Game1.whichFarm);

        // loading
        if (this.IsParsed)
        {
            string farmId = SaveGame.loaded.whichFarm;

            // standard type
            if (Utility.TryParseEnum(farmId, out FarmType farmType) && farmType != FarmType.Custom)
                return Farm.getMapNameFromTypeInt((int)farmType);

            // custom type
            foreach (ModFarmType? entry in DataLoader.AdditionalFarms(Game1.content))
            {
                if (entry?.Id == farmId)
                    return entry.MapName;
            }
        }

        // unknown/default type
        return "Farm";
    }

    /// <summary>Get whether the community center is complete.</summary>
    /// <remarks>See game logic in <see cref="Town.resetLocalState"/>.</remarks>
    public bool GetIsCommunityCenterComplete()
    {
        Farmer? host = this.GetPlayer(PlayerType.HostPlayer);

        return
            host?.mailReceived.Contains("ccIsComplete") == true
            || host?.hasCompletedCommunityCenter() == true;
    }

    /// <summary>Get whether the JojaMart is complete.</summary>
    /// <remarks>See game logic in <see cref="Utility.hasFinishedJojaRoute"/>.</remarks>
    public bool GetIsJojaMartComplete()
    {
        Farmer? host = this.GetPlayer(PlayerType.HostPlayer);

        return
            host?.mailReceived.Any(flag => flag is "jojaVault" or "jojaPantry" or "jojaBoilerRoom" or "jojaCraftsRoom" or "jojaFishTank" or "JojaMember") is true
            && host.mailReceived.Contains("ccVault")
            && host.mailReceived.Contains("ccPantry")
            && host.mailReceived.Contains("ccBoilerRoom")
            && host.mailReceived.Contains("ccCraftsRoom")
            && host.mailReceived.Contains("ccFishTank");
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get a location from the save by its name.</summary>
    /// <param name="name">The location name.</param>
    private GameLocation? GetLocationFromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        return this.GetCached($"Location:{name}", () =>
        {
            // loaded
            if (this.AreLocationsLoaded)
                return Game1.getLocationFromName(name);

            // loading
            if (this.IsParsed)
                return SaveGame.loaded.locations.FirstOrDefault(p => p.NameOrUniqueName == name);

            return null;
        });
    }

    /// <summary>Get all locations in the game.</summary>
    private IEnumerable<GameLocation> GetLocations()
    {
        return this.GetCached("Locations", () =>
        {
            // loaded
            if (this.AreLocationsLoaded)
                return CommonHelper.GetLocations();

            // loading
            if (this.IsParsed)
            {
                return SaveGame.loaded.locations
                    .Concat(
                        from location in SaveGame.loaded.locations
                        from building in location.buildings
                        where building.indoors.Value != null
                        select building.indoors.Value
                    );
            }

            return [];
        });
    }

    /// <summary>Get a location's context.</summary>
    /// <param name="location">The location instance.</param>
    private string GetLocationContext(GameLocation? location)
    {
        // save is fully loaded, get context from location
        if (Context.IsWorldReady)
            return location?.GetLocationContextId() ?? LocationContexts.DefaultId;

        // save is partly loaded, get from location if available.
        // Note: avoid calling GetLocationContextId() which may trigger a map load before the
        // game is fully initialized.
        if (this.IsBasicInfoLoaded && location?.locationContextId != null)
            return location.locationContextId;

        // Else fake it based on the assumption that the player is sleeping in a vanilla
        // location. If the player sleeps in a custom context, the token will only be incorrect
        // for a short period early in the load process.
        return location is IslandLocation or IslandFarmHouse
            ? LocationContexts.IslandId
            : LocationContexts.DefaultId;
    }

    /// <summary>Get all owners for all constructed buildings on the farm.</summary>
    private IDictionary<GameLocation, long> GetBuildingInteriorOwners()
    {
        return this.GetCached("InteriorOwners", () =>
        {
            var owners = new Dictionary<GameLocation, long>();

            if (this.GetLocationFromName("Farm") is Farm farm)
            {
                foreach (Building building in farm.buildings)
                {
                    GameLocation? interior = building.GetIndoors();
                    long owner = building.owner.Value;

                    if (interior is not null && owner != 0)
                        owners[interior] = owner;
                }
            }

            return owners;
        });
    }

    /// <summary>Get the names of all social NPCs.</summary>
    private ISet<string> GetSocialVillagerNames()
    {
        return this.GetCached("SocialVillagerNames", () =>
        {
            HashSet<string> set = new(StringComparer.OrdinalIgnoreCase);

            // from data
            if (Game1.characterData != null)
            {
                foreach ((string name, CharacterData? data) in Game1.characterData)
                {
                    if (data is null || !GameStateQuery.CheckConditions(data.CanSocialize))
                        continue;

                    set.Add(name);
                }
            }

            // from world
            foreach (NPC npc in this.GetAllCharacters())
            {
                if (set.Contains(npc.Name) || !npc.CanSocialize)
                    continue;

                set.Add(npc.Name);
            }

            return set;
        });
    }

    /// <summary>Get all characters in reachable locations.</summary>
    /// <remarks>This is similar to <see cref="Utility.getAllCharacters()"/>, but doesn't sometimes crash when a farmhand warps and <see cref="Game1.currentLocation"/> isn't set yet.</remarks>
    private IEnumerable<NPC> GetAllCharacters()
    {
        return this.GetCached("AllCharacters", () =>
            this
                .GetLocations()
                .SelectMany(p => p.characters)
                .Distinct(new ObjectReferenceComparer<NPC>())
        );
    }

    /// <summary>Get the player's spouse, if they're married.</summary>
    /// <param name="player">The player whose spouse to get.</param>
    /// <param name="friendship">The friendship info for the player or NPC marriage, if they're married.</param>
    /// <param name="playerSpouseId">The unique ID of the player they're married to, if applicable.</param>
    /// <returns>Returns whether they're married (whether to a player or NPC).</returns>
    private bool TryGetRawSpouseInfo(Farmer player, [NotNullWhen(true)] out Friendship? friendship, out long? playerSpouseId)
    {
        // loaded
        if (this.IsBasicInfoLoaded)
        {
            playerSpouseId = player.team.GetSpouse(player.UniqueMultiplayerID);
            friendship = player.GetSpouseFriendship();
            return friendship != null;
        }

        // loading
        if (this.IsParsed)
        {
            // player spouse
            foreach (var pair in SaveGame.loaded.farmerFriendships)
            {
                if (pair.Key.Contains(player.UniqueMultiplayerID) && (pair.Value.IsEngaged() || pair.Value.IsMarried()))
                {
                    playerSpouseId = pair.Key.GetOther(player.UniqueMultiplayerID);
                    friendship = pair.Value;
                    return true;
                }
            }

            // NPC spouse
            playerSpouseId = null;
            friendship = player.spouse is not (null or "") && player.friendshipData.TryGetValue(player.spouse, out Friendship value)
                ? value
                : null;
            return friendship != null;
        }

        playerSpouseId = null;
        friendship = null;
        return false;
    }

    /// <summary>Get a constant for a given value.</summary>
    /// <typeparam name="TEnum">The constant enum type.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <param name="defaultValue">The value to use if the value is invalid.</param>
    private TEnum GetEnum<TEnum>(int value, TEnum defaultValue)
    {
        return Enum.IsDefined(typeof(TEnum), value)
            ? (TEnum)(object)value
            : defaultValue;
    }

    /// <summary>Read data from the save file and cache it for the current context update tick.</summary>
    /// <typeparam name="TValue">The data type to fetch and cache.</typeparam>
    /// <param name="key">A unique key for the data.</param>
    /// <param name="fetch">Fetch the latest value if needed.</param>
    private TValue GetCached<TValue>(string key, Func<TValue> fetch)
    {
        // clear cache on update tick
        if (this.LastCacheTick != this.GetUpdateTick())
        {
            this.Cache.Clear();
            this.LastCacheTick = this.GetUpdateTick();
        }

        // get from cache
        if (this.Cache.TryGetValue(key, out object? cacheEntry))
        {
            return cacheEntry switch
            {
                TValue cachedValue => cachedValue,
                null when default(TValue) is null => default!,
                _ => throw new InvalidOperationException($"Can't fetch cached save data for the '{key}' cache key; requested a {typeof(TValue).FullName} value, but the cache contains {(cacheEntry == null ? "null" : $"'{cacheEntry.GetType().FullName}'")} instead.")
            };
        }

        // fetch and cache
        TValue value = fetch();
        this.Cache[key] = value;
        return value;
    }
}
