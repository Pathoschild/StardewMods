using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Constants;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;

namespace ContentPatcher.Framework
{
    /// <summary>Handles reading info from the current save.</summary>
    internal class TokenSaveReader
    {
        /*********
        ** Fields
        *********/
        /// <summary>Simplifies access to private code.</summary>
        private readonly IReflectionHelper Reflection;

        /// <summary>The backing field for <see cref="IsReady"/>.</summary>
        private readonly Func<bool> IsReadyImpl;


        /*********
        ** Accessors
        *********/
        /// <summary>Whether the basic save info is loaded (including the date, weather, and player info). The in-game locations and world may not exist yet.</summary>
        public bool IsReady => this.IsReadyImpl();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="reflection">Simplifies access to private code.</param>
        /// <param name="isReady">Whether the basic save info is loaded (including the date, weather, and player info). The in-game locations and world may not exist yet.</param>
        public TokenSaveReader(IReflectionHelper reflection, Func<bool> isReady)
        {
            this.Reflection = reflection;
            this.IsReadyImpl = isReady;
        }

        /****
        ** General utilities
        ****/
        /// <summary>Get a player instance.</summary>
        /// <param name="type">The player type.</param>
        public Farmer GetPlayer(PlayerType type)
        {
            return type == PlayerType.HostPlayer
                ? Game1.MasterPlayer
                : Game1.player;
        }

        /// <summary>Get the current player instance.</summary>
        public Farmer GetCurrentPlayer()
        {
            return Game1.player;
        }

        /// <summary>Get all players in the game, even if they're offline.</summary>
        public IEnumerable<Farmer> GetAllPlayers()
        {
            return Game1.getAllFarmers();
        }

        /// <summary>Get the current player's location.</summary>
        public GameLocation GetCurrentLocation()
        {
            return Game1.currentLocation;
        }

        /****
        ** Date & weather
        ****/
        /// <summary>Get the day of month.</summary>
        public int GetDay()
        {
            return SDate.Now().Day;
        }

        /// <summary>Get the name for today's day event (e.g. wedding or festival).</summary>
        public string GetDayEvent()
        {
            // marriage
            if (SaveGame.loaded?.weddingToday ?? Game1.weddingToday)
                return "wedding";

            // festival
            IDictionary<string, string> festivalDates = Game1.content.Load<Dictionary<string, string>>("Data\\Festivals\\FestivalDates", LocalizedContentManager.LanguageCode.en); // {{DayEvent}} shouldn't be translated
            if (festivalDates.TryGetValue($"{Game1.currentSeason}{Game1.dayOfMonth}", out string festivalName))
                return festivalName;

            return null;
        }

        /// <summary>Get the day of week.</summary>
        public DayOfWeek GetDayOfWeek()
        {
            return SDate.Now().DayOfWeek;
        }

        /// <summary>Get the day of week.</summary>
        public uint GetDaysPlayed()
        {
            return Game1.stats.DaysPlayed;
        }

        /// <summary>Get the season.</summary>
        public string GetSeason()
        {
            return SDate.Now().Season;
        }

        /// <summary>Get the year number.</summary>
        public int GetYear()
        {
            return SDate.Now().Year;
        }

        /// <summary>Get the weather value for a location context.</summary>
        /// <param name="context">The location context.</param>
        public Weather GetWeather(LocationContext context)
        {
            // special case: day events override weather in the valley
            if (context == LocationContext.Valley)
            {
                if (Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason) || (SaveGame.loaded?.weddingToday ?? Game1.weddingToday))
                    return Weather.Sun;
            }

            // else get from game
            var model = Game1.netWorldState.Value.GetWeatherForLocation((GameLocation.LocationContext)context);
            if (model.isSnowing.Value)
                return Weather.Snow;
            if (model.isRaining.Value)
                return model.isLightning.Value ? Weather.Storm : Weather.Rain;
            if (model.isDebrisWeather.Value)
                return Weather.Wind;
            return Weather.Sun;
        }

        /// <summary>Get the time of day.</summary>
        public int GetTime()
        {
            return Game1.timeOfDay;
        }

        /****
        ** Player
        ****/
        /// <summary>Get the letter IDs, mail flags, and world state IDs set for the player.</summary>
        /// <param name="player">The player whose values to get.</param>
        /// <remarks>See mail logic in <see cref="Farmer.hasOrWillReceiveMail"/>.</remarks>
        public IEnumerable<string> GetFlags(Farmer player)
        {
            return player
                .mailReceived
                .Union(player.mailForTomorrow)
                .Union(player.mailbox)
                .Concat(Game1.worldStateIDs);
        }

        /// <summary>Get the wallet items for the current player.</summary>
        public IEnumerable<string> GetWalletItems()
        {
            Farmer player = this.GetCurrentPlayer();
            if (player == null)
                yield break;

            if (player.eventsSeen.Contains(2120303))
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
            if (player.eventsSeen.Contains(3910979))
                yield return WalletItem.SpringOnionMastery.ToString();
        }

        /****
        ** Relationships
        ****/
        /// <summary>Get values for a given player's children.</summary>
        /// <param name="player">The player whose children to get.</param>
        /// <param name="type">The token values to get.</param>
        public IEnumerable<string> GetChildValues(Farmer player, ConditionType type)
        {
            // get home
            FarmHouse home = Context.IsWorldReady
                ? Game1.getLocationFromName(player.homeLocation.Value) as FarmHouse
                : SaveGame.loaded?.locations.OfType<FarmHouse>().FirstOrDefault(p => p.Name == player.homeLocation.Value);
            if (home == null)
                yield break;

            // get children
            foreach (Child child in home.getChildren())
            {
                yield return type switch
                {
                    ConditionType.ChildNames => child.Name,
                    ConditionType.ChildGenders => (child.Gender == NPC.female ? Gender.Female : Gender.Male).ToString(),
                    _ => throw new NotSupportedException($"Invalid child token type '{type}', must be one of '{nameof(ConditionType.ChildGenders)}' or '{nameof(ConditionType.ChildNames)}'.")
                };
            }
        }

        /// <summary>Get the friendship data for the current player.</summary>
        /// <returns>Returns a list of friendship models for met NPCs, and null for unmet NPCs.</returns>
        public IEnumerable<KeyValuePair<string, Friendship>> GetFriendships()
        {
            var data = Game1.player.friendshipData;

            // met NPCs
            foreach (KeyValuePair<string, Friendship> pair in data.Pairs)
                yield return pair;

            // unmet NPCs
            foreach (NPC npc in this.GetSocialVillagers())
            {
                if (!data.ContainsKey(npc.Name))
                    yield return new KeyValuePair<string, Friendship>(npc.Name, null);
            }
        }

        /// <summary>Get the current player's spouse.</summary>
        public string GetSpouse()
        {
            return this.GetCurrentPlayer().spouse;
        }

        /// <summary>Get the name and gender of the player's spouse, if they're married</summary>
        /// <param name="player">The player whose spouse to check.</param>
        /// <param name="name">The spouse name.</param>
        /// <param name="gender">The spouse gender.</param>
        /// <param name="isPlayer">Whether the spouse is a player character.</param>
        /// <returns>Returns true if the player's spouse info was successfully found.''</returns>
        public bool TryGetSpouseInfo(Farmer player, out string name, out Friendship friendship, out Gender gender, out bool isPlayer)
        {
            friendship = player.GetSpouseFriendship();

            long? spousePlayerID = player.team.GetSpouse(player.UniqueMultiplayerID);
            if (spousePlayerID.HasValue)
            {
                Farmer spouse = Game1.getFarmerMaybeOffline(spousePlayerID.Value);
                if (spouse != null)
                {
                    name = spouse.Name;
                    gender = spouse.IsMale ? Gender.Male : Gender.Female;
                    isPlayer = true;
                    return true;
                }
            }
            else
            {
                NPC spouse = Game1.getCharacterFromName(player.spouse, mustBeVillager: true);
                if (spouse != null)
                {
                    name = spouse.Name;
                    gender = spouse.Gender == NPC.male ? Gender.Male : Gender.Female;
                    isPlayer = false;
                    return true;
                }
            }

            name = null;
            gender = Gender.Male;
            isPlayer = false;
            return false;
        }

        /****
        ** World
        ****/
        /// <summary>Get the current player's selected farm cave type.</summary>
        public FarmCaveType GetFarmCaveType()
        {
            return this.GetEnum(this.GetCurrentPlayer().caveChoice.Value, FarmCaveType.None);
        }

        /// <summary>Get the farm name.</summary>
        public string GetFarmName()
        {
            return this.GetCurrentPlayer().farmName.Value;
        }

        /// <summary>Get the farm type.</summary>
        public FarmType GetFarmType()
        {
            return this.GetEnum(Game1.whichFarm, FarmType.Custom);
        }

        /// <summary>Get whether the community center is complete.</summary>
        /// <remarks>See game logic in <see cref="StardewValley.Locations.Town.resetLocalState"/>.</remarks>
        public bool GetIsCommunityCenterComplete()
        {
            Farmer host = this.GetPlayer(PlayerType.HostPlayer);

            return host.mailReceived.Contains("ccIsComplete") || host.hasCompletedCommunityCenter();
        }

        /// <summary>Get whether the JojaMart is complete.</summary>
        /// <remarks>See game logic in <see cref="GameLocation"/> for the 'C' precondition.</remarks>
        public bool GetIsJojaMartComplete()
        {
            Farmer host = this.GetPlayer(PlayerType.HostPlayer);

            if (!host.mailReceived.Contains("JojaMember"))
                return false;

            GameLocation town = this.GetLocationFromName("Town");
            return this.Reflection.GetMethod(town, "checkJojaCompletePrerequisite").Invoke<bool>();
        }


        /*********
        ** Private methods
        *********/
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

        /// <summary>Get a location from the save by its name.</summary>
        /// <param name="name">The location name.</param>
        private GameLocation GetLocationFromName(string name)
        {
            return Game1.getLocationFromName(name);
        }

        /// <summary>Get all locations in the game.</summary>
        private IEnumerable<GameLocation> GetLocations()
        {
            return CommonHelper.GetLocations();
        }

        /// <summary>Get all social NPCs.</summary>
        private IEnumerable<NPC> GetSocialVillagers()
        {
            foreach (NPC npc in this.GetAllCharacters())
            {
                bool isSocial =
                    npc.CanSocialize
                    || (npc.Name == "Krobus" && !this.GetCurrentPlayer().friendshipData.ContainsKey(npc.Name)); // Krobus is marked non-social before he's met

                if (isSocial)
                    yield return npc;
            }
        }

        /// <summary>Get all characters in reachable locations.</summary>
        /// <remarks>This is similar to <see cref="Utility.getAllCharacters()"/>, but doesn't sometimes crash when a farmhand warps and <see cref="Game1.currentLocation"/> isn't set yet.</remarks>
        private IEnumerable<NPC> GetAllCharacters()
        {
            return this
                .GetLocations()
                .SelectMany(p => p.characters)
                .Distinct(new ObjectReferenceComparer<NPC>());
        }
    }
}
