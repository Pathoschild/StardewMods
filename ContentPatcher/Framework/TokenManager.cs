using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Constants;
using ContentPatcher.Framework.Tokens;
using ContentPatcher.Framework.Tokens.ValueProviders;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace ContentPatcher.Framework
{
    /// <summary>Manages the available contextual tokens.</summary>
    internal class TokenManager : IContext
    {
        /*********
        ** Fields
        *********/
        /// <summary>The available global tokens.</summary>
        private readonly GenericTokenContext GlobalContext;

        /// <summary>The available tokens defined within the context of each content pack.</summary>
        private readonly Dictionary<IContentPack, ModTokenContext> LocalTokens = new Dictionary<IContentPack, ModTokenContext>();

        /// <summary>The installed mod IDs.</summary>
        private readonly InvariantHashSet InstalledMods;

        /// <summary>Simplifies access to private code.</summary>
        private readonly IReflectionHelper Reflection;


        /*********
        ** Accessors
        *********/
        /// <summary>Whether the basic save info is loaded (including the date, weather, and player info). The in-game locations and world may not exist yet.</summary>
        public bool IsBasicInfoLoaded { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="contentHelper">The content helper from which to load data assets.</param>
        /// <param name="installedMods">The installed mod IDs.</param>
        /// <param name="modTokens">The custom tokens provided by mods.</param>
        /// <param name="reflection">Simplifies access to private code.</param>
        public TokenManager(IContentHelper contentHelper, InvariantHashSet installedMods, IEnumerable<IToken> modTokens, IReflectionHelper reflection)
        {
            this.InstalledMods = installedMods;
            this.GlobalContext = new GenericTokenContext(this.IsModInstalled);
            this.Reflection = reflection;

            foreach (IToken modToken in modTokens)
                this.GlobalContext.Tokens[modToken.Name] = modToken;
            foreach (IValueProvider valueProvider in this.GetGlobalValueProviders(contentHelper, installedMods))
                this.GlobalContext.Tokens[valueProvider.Name] = new GenericToken(valueProvider);
        }

        /// <summary>Get the tokens which are defined for a specific content pack. This returns a reference to the list, which can be held for a live view of the tokens. If the content pack isn't currently tracked, this will add it.</summary>
        /// <param name="pack">The content pack to manage.</param>
        public ModTokenContext TrackLocalTokens(ManagedContentPack pack)
        {
            string scope = pack.Manifest.UniqueID;

            if (!this.LocalTokens.TryGetValue(pack.Pack, out ModTokenContext localTokens))
            {
                this.LocalTokens[pack.Pack] = localTokens = new ModTokenContext(scope, this);
                foreach (IValueProvider valueProvider in this.GetLocalValueProviders(pack))
                    localTokens.Add(new GenericToken(valueProvider, scope));
            }

            return localTokens;
        }

        /// <summary>Get the token context for a given mod ID.</summary>
        /// <param name="contentPackID">The content pack ID to search for.</param>
        /// <exception cref="KeyNotFoundException">There's no content pack registered with the given <paramref name="contentPackID"/>.</exception>
        public IContext GetContextFor(string contentPackID)
        {
            foreach (var pair in this.LocalTokens)
            {
                if (pair.Key.Manifest.UniqueID.Equals(contentPackID, StringComparison.InvariantCultureIgnoreCase))
                    return pair.Value;
            }

            throw new KeyNotFoundException($"There's no content pack registered for ID '{contentPackID}'.");
        }

        /// <summary>Update the current context.</summary>
        /// <param name="globalChangedTokens">The global token values which changed, or <c>null</c> to update all tokens.</param>
        public void UpdateContext(InvariantHashSet globalChangedTokens = null)
        {
            // update global tokens
            foreach (IToken token in this.GlobalContext.Tokens.Values)
            {
                if (token.IsMutable && globalChangedTokens?.Contains(token.Name) != false)
                    token.UpdateContext(this);
            }

            // update mod contexts
            foreach (ModTokenContext localContext in this.LocalTokens.Values)
                localContext.UpdateContext(globalChangedTokens);
        }

        /****
        ** IContext
        ****/
        /// <summary>Get whether a mod is installed.</summary>
        /// <param name="id">The mod ID.</param>
        public bool IsModInstalled(string id)
        {
            return this.InstalledMods.Contains(id);
        }

        /// <summary>Get whether the context contains the given token.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="enforceContext">Whether to only consider tokens that are available in the context.</param>
        public bool Contains(string name, bool enforceContext)
        {
            return this.GlobalContext.Contains(name, enforceContext);
        }

        /// <summary>Get the underlying token which handles a key.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="enforceContext">Whether to only consider tokens that are available in the context.</param>
        /// <returns>Returns the matching token, or <c>null</c> if none was found.</returns>
        public IToken GetToken(string name, bool enforceContext)
        {
            return this.GlobalContext.GetToken(name, enforceContext);
        }

        /// <summary>Get the underlying tokens.</summary>
        /// <param name="enforceContext">Whether to only consider tokens that are available in the context.</param>
        public IEnumerable<IToken> GetTokens(bool enforceContext)
        {
            return this.GlobalContext.GetTokens(enforceContext);
        }

        /// <summary>Get the current values of the given token for comparison.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="input">The input argument, if any.</param>
        /// <param name="enforceContext">Whether to only consider tokens that are available in the context.</param>
        /// <returns>Return the values of the matching token, or an empty list if the token doesn't exist.</returns>
        /// <exception cref="ArgumentNullException">The specified key is null.</exception>
        public IEnumerable<string> GetValues(string name, ITokenString input, bool enforceContext)
        {
            return this.GlobalContext.GetValues(name, input, enforceContext);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the global value providers with which to initialize the token manager.</summary>
        /// <param name="contentHelper">The content helper from which to load data assets.</param>
        /// <param name="installedMods">The installed mod IDs.</param>
        private IEnumerable<IValueProvider> GetGlobalValueProviders(IContentHelper contentHelper, InvariantHashSet installedMods)
        {
            bool NeedsBasicInfo() => this.IsBasicInfoLoaded;

            // date and weather
            yield return new ConditionTypeValueProvider(ConditionType.Day, () => SDate.Now().Day.ToString(CultureInfo.InvariantCulture), NeedsBasicInfo, allowedValues: Enumerable.Range(0, 29).Select(p => p.ToString())); // day 0 = new-game intro
            yield return new ConditionTypeValueProvider(ConditionType.DayEvent, () => this.GetDayEvent(contentHelper), NeedsBasicInfo);
            yield return new ConditionTypeValueProvider(ConditionType.DayOfWeek, () => SDate.Now().DayOfWeek.ToString(), NeedsBasicInfo, allowedValues: Enum.GetNames(typeof(DayOfWeek)));
            yield return new ConditionTypeValueProvider(ConditionType.DaysPlayed, () => Game1.stats.DaysPlayed.ToString(CultureInfo.InvariantCulture), NeedsBasicInfo);
            yield return new ConditionTypeValueProvider(ConditionType.Season, () => SDate.Now().Season, NeedsBasicInfo, allowedValues: new[] { "Spring", "Summer", "Fall", "Winter" });
            yield return new ConditionTypeValueProvider(ConditionType.Year, () => SDate.Now().Year.ToString(CultureInfo.InvariantCulture), NeedsBasicInfo);
            yield return new ConditionTypeValueProvider(ConditionType.Weather, this.GetCurrentWeather, NeedsBasicInfo, allowedValues: Enum.GetNames(typeof(Weather)));

            // player
            yield return new ConditionTypeValueProvider(ConditionType.HasFlag, this.GetFlags, NeedsBasicInfo);
            yield return new HasProfessionValueProvider(NeedsBasicInfo);
            yield return new ConditionTypeValueProvider(ConditionType.HasReadLetter, this.GetReadLetters, NeedsBasicInfo);
            yield return new ConditionTypeValueProvider(ConditionType.HasSeenEvent, this.GetEventsSeen, NeedsBasicInfo);
            yield return new ConditionTypeValueProvider(ConditionType.HasDialogueAnswer, this.GetDialogueAnswers, NeedsBasicInfo);
            yield return new HasWalletItemValueProvider(NeedsBasicInfo);
            yield return new ConditionTypeValueProvider(ConditionType.IsMainPlayer, () => Context.IsMainPlayer.ToString(), NeedsBasicInfo);
            yield return new ConditionTypeValueProvider(ConditionType.IsOutdoors, () => Game1.currentLocation?.IsOutdoors.ToString(), NeedsBasicInfo);
            yield return new ConditionTypeValueProvider(ConditionType.LocationName, () => Game1.currentLocation?.Name, NeedsBasicInfo);
            yield return new ConditionTypeValueProvider(ConditionType.PlayerGender, () => (Game1.player.IsMale ? Gender.Male : Gender.Female).ToString(), NeedsBasicInfo);
            yield return new ConditionTypeValueProvider(ConditionType.PlayerName, () => Game1.player.Name, NeedsBasicInfo);
            yield return new ConditionTypeValueProvider(ConditionType.PreferredPet, () => (Game1.player.catPerson ? PetType.Cat : PetType.Dog).ToString(), NeedsBasicInfo);
            yield return new SkillLevelValueProvider(NeedsBasicInfo);

            // relationships
            yield return new VillagerHeartsValueProvider();
            yield return new VillagerRelationshipValueProvider();
            yield return new ConditionTypeValueProvider(ConditionType.Spouse, () => Game1.player?.spouse, NeedsBasicInfo);

            // world
            yield return new ConditionTypeValueProvider(ConditionType.FarmCave, () => this.GetEnum(Game1.player.caveChoice.Value, FarmCaveType.None).ToString(), NeedsBasicInfo);
            yield return new ConditionTypeValueProvider(ConditionType.FarmhouseUpgrade, () => Game1.player.HouseUpgradeLevel.ToString(), NeedsBasicInfo);
            yield return new ConditionTypeValueProvider(ConditionType.FarmName, () => Game1.player.farmName.Value, NeedsBasicInfo);
            yield return new ConditionTypeValueProvider(ConditionType.FarmType, () => this.GetEnum(Game1.whichFarm, FarmType.Custom).ToString(), NeedsBasicInfo);
            yield return new ConditionTypeValueProvider(ConditionType.IsCommunityCenterComplete, () => this.GetIsCommunityCenterComplete().ToString(), NeedsBasicInfo);
            yield return new ConditionTypeValueProvider(ConditionType.IsJojaMartComplete, () => this.GetIsJojaMartComplete().ToString(), NeedsBasicInfo);
            yield return new HavingChildValueProvider(ConditionType.Pregnant, NeedsBasicInfo);
            yield return new HavingChildValueProvider(ConditionType.HavingChild, NeedsBasicInfo);

            // string manipulation
            yield return new RangeValueProvider();
            yield return new LetterCaseValueProvider(ConditionType.Lowercase);
            yield return new LetterCaseValueProvider(ConditionType.Uppercase);

            // other
            yield return new ImmutableValueProvider(ConditionType.HasMod.ToString(), installedMods, canHaveMultipleValues: true);
            yield return new HasValueValueProvider();
            yield return new ConditionTypeValueProvider(ConditionType.Language, () => contentHelper.CurrentLocaleConstant.ToString(), allowedValues: Enum.GetNames(typeof(LocalizedContentManager.LanguageCode)).Where(p => p != LocalizedContentManager.LanguageCode.th.ToString()));
        }

        /// <summary>Get the local value providers with which to initialize a local context.</summary>
        /// <param name="contentPack">The content pack for which to get tokens.</param>
        private IEnumerable<IValueProvider> GetLocalValueProviders(ManagedContentPack contentPack)
        {
            yield return new HasFileValueProvider(contentPack.HasFile);
            yield return new RandomValueProvider(); // per-pack for more reproducible selection when troubleshooting
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

        /// <summary>Get the current weather from the game state.</summary>
        private string GetCurrentWeather()
        {
            if (Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason) || (SaveGame.loaded?.weddingToday ?? Game1.weddingToday))
                return Weather.Sun.ToString();

            if (Game1.isSnowing)
                return Weather.Snow.ToString();
            if (Game1.isRaining)
                return (Game1.isLightning ? Weather.Storm : Weather.Rain).ToString();
            if (SaveGame.loaded?.isDebrisWeather ?? Game1.isDebrisWeather)
                return Weather.Wind.ToString();

            return Weather.Sun.ToString();
        }

        /// <summary>Get the event IDs seen by the player.</summary>
        private IEnumerable<string> GetEventsSeen()
        {
            Farmer player = Game1.player;
            if (player == null)
                return new string[0];

            return player.eventsSeen
                .OrderBy(p => p)
                .Select(p => p.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>Get the letter IDs read by the player.</summary>
        /// <remarks>See game logic in <see cref="Farmer.hasOrWillReceiveMail"/>.</remarks>
        private IEnumerable<string> GetReadLetters()
        {
            if (Game1.player == null)
                return new string[0];
            return Game1.player.mailReceived;
        }

        /// <summary>Get the letter IDs, mail flags, and world state IDs set for the player.</summary>
        /// <remarks>See mail logic in <see cref="Farmer.hasOrWillReceiveMail"/>.</remarks>
        private IEnumerable<string> GetFlags()
        {
            // mail flags
            if (Game1.player != null)
            {
                foreach (string flag in Game1.player.mailReceived.Union(Game1.player.mailForTomorrow).Union(Game1.player.mailbox))
                    yield return flag;
            }

            // world state flags
            foreach (string flag in Game1.worldStateIDs)
                yield return flag;
        }

        /// <summary>Get whether the community center is complete.</summary>
        /// <remarks>See game logic in <see cref="StardewValley.Locations.Town.resetLocalState"/>.</remarks>
        private bool GetIsCommunityCenterComplete()
        {
            return Game1.MasterPlayer.mailReceived.Contains("ccIsComplete") || Game1.MasterPlayer.hasCompletedCommunityCenter();
        }

        /// <summary>Get whether the JojaMart is complete.</summary>
        /// <remarks>See game logic in <see cref="GameLocation"/> for the 'C' precondition.</remarks>
        private bool GetIsJojaMartComplete()
        {
            if (!Game1.MasterPlayer.mailReceived.Contains("JojaMember"))
                return false;

            GameLocation town = Game1.getLocationFromName("Town");
            return this.Reflection.GetMethod(town, "checkJojaCompletePrerequisite").Invoke<bool>();

        }

        /// <summary>Get the name for today's day event (e.g. wedding or festival) from the game data.</summary>
        /// <param name="contentHelper">The content helper from which to load festival data.</param>
        private string GetDayEvent(IContentHelper contentHelper)
        {
            // marriage
            if (SaveGame.loaded?.weddingToday ?? Game1.weddingToday)
                return "wedding";

            // festival
            IDictionary<string, string> festivalDates = contentHelper.Load<Dictionary<string, string>>("Data\\Festivals\\FestivalDates", ContentSource.GameContent);
            if (festivalDates.TryGetValue($"{Game1.currentSeason}{Game1.dayOfMonth}", out string festivalName))
                return festivalName;

            return null;
        }

        /// <summary>Get the response IDs of dialogue answers given by the player.</summary>
        private IEnumerable<string> GetDialogueAnswers()
        {
            if (Game1.player == null)
                return new string[0];
            return Game1.player.dialogueQuestionsAnswered
                .OrderBy(p => p)
                .Select(p => p.ToString(CultureInfo.InvariantCulture));
        }
    }
}
