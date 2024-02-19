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
using StardewModdingAPI.Enums;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Network;

namespace ContentPatcher.Framework
{
    /// <summary>Handles reading info from the current save.</summary>
    /// <remarks>This allows reading data from the save immediately after it's parsed, before the save is loaded.</remarks>
    internal class TokenSaveReader
    {
        /*********
        ** Fields
        *********/
        /// <summary>The number of context updates since the game was launched, including the current one if applicable.</summary>
        private readonly Func<int> GetUpdateTick;

        /// <summary>Whether the save file has been parsed into <see cref="SaveGame.loaded"/> (regardless of whether the game started loading it yet).</summary>
        private readonly Func<bool> IsSaveParsed;

        /// <summary>Whether the basic save info is loaded (including the date, weather, and player info). The in-game locations and world may not exist yet.</summary>
        private readonly Func<bool> IsSaveBasicInfoLoadedImpl;

        /// <summary>This happens before the game applies problem fixes, checks for achievements, starts music, etc.</summary>
        private readonly Func<bool> IsSaveLoadedImpl;

        /// <summary>A cache of common values fetched during the current context updates.</summary>
        private readonly Dictionary<string, object?> Cache = new();

        /// <summary>The last context update for which cached values were updated.</summary>
        private int LastCacheTick;


        /*********
        ** Accessors
        *********/
        /// <summary>Whether data can be read from the save file now.</summary>
        public bool IsReady => this.IsSaveParsed();

        /// <summary>Whether the basic save info is loaded (including the date, weather, and player info). The in-game locations and world may not exist yet.</summary>
        public bool IsSaveBasicInfoLoaded => this.IsSaveBasicInfoLoadedImpl();

        /// <summary>This happens before the game applies problem fixes, checks for achievements, starts music, etc.</summary>
        public bool IsSaveLoaded => this.IsSaveLoadedImpl();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="updateTick">The number of context updates since the game was launched, including the current one if applicable.</param>
        /// <param name="isSaveParsed">Whether the save file has been parsed into <see cref="SaveGame.loaded"/> (regardless of whether the game started loading it yet).</param>
        /// <param name="isSaveBasicInfoLoaded">Whether the basic save info is loaded (including the date, weather, and player info). The in-game locations and world may not exist yet</param>
        /// <param name="isSaveLoaded">This happens before the game applies problem fixes, checks for achievements, starts music, etc.</param>
        public TokenSaveReader(Func<int> updateTick, Func<bool> isSaveParsed, Func<bool> isSaveBasicInfoLoaded, Func<bool> isSaveLoaded)
        {
            this.GetUpdateTick = updateTick;
            this.IsSaveParsed = isSaveParsed;
            this.IsSaveBasicInfoLoadedImpl = isSaveBasicInfoLoaded;
            this.IsSaveLoadedImpl = isSaveLoaded;
        }

        /****
        ** General utilities
        ****/
        /// <summary>Get a player instance.</summary>
        /// <param name="type">The player type.</param>
        public Farmer? GetPlayer(PlayerType type)
        {
            return this.GetForState(
                loaded: () => type == PlayerType.HostPlayer
                    ? Game1.MasterPlayer
                    : Game1.player,
                reading: save => save.player, // current == host if they're loading the save file
                defaultValue: null
            );
        }

        /// <summary>Get the current player instance.</summary>
        public Farmer? GetCurrentPlayer()
        {
            return this.GetPlayer(PlayerType.CurrentPlayer);
        }

        /// <summary>Get all players in the game, even if they're offline.</summary>
        public IEnumerable<Farmer> GetAllPlayers()
        {
            return this.GetCached(
                nameof(this.GetAllPlayers),
                () => this.GetForState(
                    loaded: Game1.getAllFarmers,
                    reading: save => new[] { save.player }.Concat(save.farmhands),
                    defaultValue: Array.Empty<Farmer>()
                )
            );
        }

        /// <summary>Get a player's location.</summary>
        /// <param name="player">The player instance.</param>
        public GameLocation? GetCurrentLocation(Farmer? player)
        {
            return this.GetCached(
                $"{nameof(this.GetCurrentLocation)}:{player?.UniqueMultiplayerID}",
                () => this.GetForState(
                    loaded: () => Context.IsWorldReady
                        ? player?.currentLocation
                        : player?.currentLocation ?? (player != null ? this.GetLocationFromName(player.lastSleepLocation.Value) : null), // currentLocation is set later in the save loading process
                    reading: _ => player != null
                        ? this.GetLocationFromName(player.lastSleepLocation.Value)
                        : null,
                    defaultValue: null,

                    requiresPreload: true
                )
            );
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

            long? id = this.GetCached(
                $"{nameof(this.GetLocationOwnerId)}:{location.Name}",
                () =>
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
                                Dictionary<string, long>? cellarAssignments = this.GetForState(
                                    loaded: () => Game1.player.team.cellarAssignments.FieldDict.ToDictionary(p => $"Cellar{p.Key}", p => p.Value.Value),
                                    reading: save => save.cellarAssignments.ToDictionary(p => $"Cellar{p.Key}", p => p.Value),
                                    defaultValue: null
                                );

                                if (cellarAssignments is not null)
                                {
                                    string key = location.Name == "Cellar"
                                        ? "Cellar1"
                                        : location.Name;
                                    if (cellarAssignments.TryGetValue(key, out long owner))
                                        return owner;
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
                }
            );

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
            return this.GetForState(
                loaded: () => Game1.dayOfMonth,
                reading: save => save.dayOfMonth,
                defaultValue: 0
            );
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
            return this.GetForState(
                loaded: () => Game1.stats.DaysPlayed,
                reading: save => (save.obsolete_stats ?? save.player.stats).DaysPlayed,
                defaultValue: 0u
            );
        }

        /// <summary>Get the season.</summary>
        public string GetSeason()
        {
            return this.GetForState(
                loaded: () => Game1.currentSeason,
                reading: save => save.currentSeason,
                defaultValue: "spring"
            );
        }

        /// <summary>Get the year number.</summary>
        public int GetYear()
        {
            return this.GetForState(
                loaded: () => Game1.year,
                reading: save => save.year,
                defaultValue: 0
            );
        }

        /// <summary>Get the currently defined contexts.</summary>
        public HashSet<string> GetContexts()
        {
            return this.GetCached(
                nameof(this.GetContexts),
                () =>
                {
                    HashSet<string> contexts = new()
                    {
                        LocationContexts.DefaultId,
                        LocationContexts.DesertId,
                        LocationContexts.IslandId
                    };

                    foreach (GameLocation location in this.GetLocations())
                        contexts.Add(this.GetLocationContext(location));

                    return contexts;
                }
            );
        }

        /// <summary>Get the weather value for a location context.</summary>
        /// <param name="contextName">The name of the location context.</param>
        public string? GetWeather(string contextName)
        {
            LocationWeather? model = this.GetForState(
                loaded: () => Game1.netWorldState.Value.GetWeatherForLocation(contextName),
                reading: save => save.locationWeather != null && save.locationWeather.TryGetValue(contextName, out LocationWeather? weather) ? weather : null,
                defaultValue: null
            );

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
            return this.GetForState(
                loaded: () => Game1.timeOfDay,
                reading: _ => 0600,
                defaultValue: 0600
            );
        }

        /****
        ** Player
        ****/
        /// <summary>Get the letter IDs, mail flags, and world state IDs set for the player.</summary>
        /// <param name="player">The player instance.</param>
        /// <remarks>See mail logic in <see cref="Farmer.hasOrWillReceiveMail"/>.</remarks>
        public IEnumerable<string> GetFlags(Farmer player)
        {
            return player
                .mailReceived
                .Union(player.mailForTomorrow)
                .Union(player.mailbox)
                .Concat(
                    this.GetForState(
                        loaded: () => Game1.worldStateIDs ?? Enumerable.Empty<string>(),
                        reading: save => save.worldStateIDs ?? Enumerable.Empty<string>(),
                        defaultValue: Array.Empty<string>()
                    )
                );
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
            return this.GetForState(
                loaded: () => player.DailyLuck,
                reading: save => save.dailyLuck + (player.hasSpecialCharm ? 0.025f : 0),
                defaultValue: 0
            );
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
            List<Child>? children = this.GetCached(
                $"{nameof(this.GetChildValues)}:{player.UniqueMultiplayerID}",
                () => (this.GetLocationFromName(player.homeLocation.Value) as FarmHouse)?.getChildren()
            );
            if (children == null)
                return Enumerable.Empty<string>();

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
            return this.GetCached(
                nameof(this.GetFriendships),
                () =>
                {
                    Dictionary<string, NetRef<Friendship>> met = this.GetCurrentPlayer()
                        ?.friendshipData
                        ?.FieldDict
                        ?? new();

                    return
                        (
                            from pair in met
                            select new KeyValuePair<string, Friendship?>(pair.Key, pair.Value.Value)
                        )
                        .Concat(
                            from npc in this.GetSocialVillagers()
                            where !met.ContainsKey(npc.Name)
                            select new KeyValuePair<string, Friendship?>(npc.Name, null)
                        );
                }
            );
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
            var data = this.GetCached(
                $"{nameof(this.TryGetSpouseInfo)}:{player.UniqueMultiplayerID}",
                () =>
                {
                    // get raw data
                    long? spousePlayerID = null;
                    Friendship? friendship = this.GetForState(
                        loaded: () =>
                        {
                            spousePlayerID = player.team.GetSpouse(player.UniqueMultiplayerID);
                            return player.GetSpouseFriendship();
                        },

                        reading: save =>
                        {
                            // player spouse
                            foreach (var pair in save.farmerFriendships)
                            {
                                if (pair.Key.Contains(player.UniqueMultiplayerID) && (pair.Value.IsEngaged() || pair.Value.IsMarried()))
                                {
                                    spousePlayerID = pair.Key.GetOther(player.UniqueMultiplayerID);
                                    return pair.Value;
                                }
                            }

                            // NPC spouse
                            return player.spouse is not (null or "") && player.friendshipData.TryGetValue(player.spouse, out Friendship value)
                                ? value
                                : null;
                        },

                        defaultValue: null
                    );

                    // parse spouse info
                    string? name = null;
                    Gender gender = Gender.Male;
                    bool isPlayer = false;
                    if (spousePlayerID.HasValue)
                    {
                        Farmer? spouse = this.GetAllPlayers().FirstOrDefault(p => p.UniqueMultiplayerID == spousePlayerID);
                        if (spouse != null)
                        {
                            name = spouse.Name;
                            gender = spouse.IsMale ? Gender.Male : Gender.Female;
                            isPlayer = true;
                        }
                    }
                    else
                    {
                        NPC? spouse = this.GetAllCharacters().FirstOrDefault(p => p.Name == player.spouse && p.isVillager());
                        if (spouse != null)
                        {
                            name = spouse.Name;
                            gender = spouse.Gender;
                            isPlayer = false;
                        }
                    }
                    bool valid = name != null && friendship != null;

                    // create cache entry
                    return (Name: name, Friendship: friendship, Gender: gender, IsPlayer: isPlayer, IsValid: valid);
                }
            );

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
            return this.GetForState(
                loaded: () => Game1.whichFarm == Farm.mod_layout
                    ? Game1.whichModFarm?.Id ?? FarmType.Custom.ToString()
                    : this.GetEnum(Game1.whichFarm, FarmType.Custom).ToString(),
                reading: save => int.TryParse(save.whichFarm, out _) && Enum.TryParse(save.whichFarm, out FarmType farmType)
                    ? farmType.ToString()
                    : save.whichFarm,
                defaultValue: FarmType.Standard.ToString()
            );
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

            return this.GetCached(
                $"{nameof(this.GetLocationFromName)}:{name}",
                () => this.GetForState(
                    loaded: () => Game1.getLocationFromName(name),
                    reading: save => save.locations.FirstOrDefault(p => p.NameOrUniqueName == name),
                    defaultValue: null,

                    requiresPreload: true
                )
            );
        }

        /// <summary>Get all locations in the game.</summary>
        private IEnumerable<GameLocation> GetLocations()
        {
            return this.GetCached(
                nameof(this.GetLocations),
                () => this.GetForState(
                    loaded: () => CommonHelper.GetLocations(),

                    reading: save => save.locations
                        .Concat(
                            from location in save.locations
                            from building in location.buildings
                            where building.indoors.Value != null
                            select building.indoors.Value
                        ),

                    defaultValue: Enumerable.Empty<GameLocation>(),

                    requiresPreload: true
                )
            );
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
            if (this.IsSaveBasicInfoLoaded && location?.locationContextId != null)
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
            return this.GetCached(
                nameof(this.GetBuildingInteriorOwners),
                () =>
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
                }
            );
        }

        /// <summary>Get all social NPCs.</summary>
        private IEnumerable<NPC> GetSocialVillagers()
        {
            return this.GetCached(
                nameof(this.GetSocialVillagers),
                () => this
                    .GetAllCharacters()
                    .Where(npc =>
                        npc.CanSocialize
                        || (npc.Name == "Krobus" && this.GetCurrentPlayer()?.friendshipData.ContainsKey(npc.Name) != true) // Krobus is marked non-social before he's met
                    )
            );
        }

        /// <summary>Get all characters in reachable locations.</summary>
        /// <remarks>This is similar to <see cref="Utility.getAllCharacters()"/>, but doesn't sometimes crash when a farmhand warps and <see cref="Game1.currentLocation"/> isn't set yet.</remarks>
        private IEnumerable<NPC> GetAllCharacters()
        {
            return this.GetCached(
                nameof(this.GetAllCharacters),
                () => this
                    .GetLocations()
                    .SelectMany(p => p.characters)
                    .Distinct(new ObjectReferenceComparer<NPC>())
            );
        }

        /// <summary>Get a value from the save file depending on the load state.</summary>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="loaded">Get the value if the save file is loaded into the base game data.</param>
        /// <param name="reading">Get the value if the save file has been parsed, but not loaded yet.</param>
        /// <param name="defaultValue">The default value if no save is parsed or loaded.</param>
        /// <param name="requiresPreload">Whether to only use <paramref name="loaded"/> if we've reached <see cref="LoadStage.Preloaded"/> or <see cref="LoadStage.Loaded"/>. This is needed when reading location data, which is restored later in the load process.</param>
        private TValue GetForState<TValue>(Func<TValue> loaded, Func<SaveGame, TValue> reading, TValue defaultValue, bool requiresPreload = false)
        {
            if (this.IsSaveLoaded || (!requiresPreload && this.IsSaveBasicInfoLoaded))
                return loaded();

            if (this.IsSaveParsed())
                return reading(SaveGame.loaded);

            return defaultValue;
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
}
