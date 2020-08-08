using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Constants;
using ContentPatcher.Framework.Tokens;
using ContentPatcher.Framework.Tokens.ValueProviders;
using ContentPatcher.Framework.Tokens.ValueProviders.Players;
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

        /// <summary>Whether the next context update is the first one.</summary>
        private bool IsFirstUpdate = true;


        /*********
        ** Accessors
        *********/
        /// <summary>Whether the basic save info is loaded (including the date, weather, and player info). The in-game locations and world may not exist yet.</summary>
        public bool IsBasicInfoLoaded { get; set; }

        /// <summary>The tokens which are linked to the player's current location.</summary>
        public IEnumerable<string> LocationTokens { get; } = new InvariantHashSet
        {
            ConditionType.LocationName.ToString(),
            ConditionType.IsOutdoors.ToString()
        };


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
                this.GlobalContext.Save(modToken);
            foreach (IValueProvider valueProvider in this.GetGlobalValueProviders(contentHelper, installedMods))
                this.GlobalContext.Save(new Token(valueProvider));
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
                    localTokens.AddLocalToken(new Token(valueProvider, scope));
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
                if (pair.Key.Manifest.UniqueID.Equals(contentPackID, StringComparison.OrdinalIgnoreCase))
                    return pair.Value;
            }

            throw new KeyNotFoundException($"There's no content pack registered for ID '{contentPackID}'.");
        }

        /// <summary>Update the current context.</summary>
        /// <param name="changedGlobalTokens">The global tokens which changed value.</param>
        public void UpdateContext(out InvariantHashSet changedGlobalTokens)
        {
            // update global tokens
            changedGlobalTokens = new InvariantHashSet();
            foreach (IToken token in this.GlobalContext.Tokens.Values)
            {
                bool changed =
                    (token.IsMutable && token.UpdateContext(this)) // token changed state/value
                    || (this.IsFirstUpdate && token.IsReady); // tokens implicitly change to ready on their first update, even if they were ready from creation
                if (changed)
                    changedGlobalTokens.Add(token.Name);
            }

            // update mod contexts
            foreach (ModTokenContext localContext in this.LocalTokens.Values)
                localContext.UpdateContext(changedGlobalTokens);

            this.IsFirstUpdate = false;
        }

        /****
        ** IContext
        ****/
        /// <inheritdoc />
        public bool IsModInstalled(string id)
        {
            return this.InstalledMods.Contains(id);
        }

        /// <inheritdoc />
        public bool Contains(string name, bool enforceContext)
        {
            return this.GlobalContext.Contains(name, enforceContext);
        }

        /// <inheritdoc />
        public IToken GetToken(string name, bool enforceContext)
        {
            return this.GlobalContext.GetToken(name, enforceContext);
        }

        /// <inheritdoc />
        public IEnumerable<IToken> GetTokens(bool enforceContext)
        {
            return this.GlobalContext.GetTokens(enforceContext);
        }

        /// <inheritdoc />
        public IEnumerable<string> GetValues(string name, IInputArguments input, bool enforceContext)
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
            yield return new LocalOrHostPlayerValueProvider(ConditionType.HasConversationTopic, player => player.activeDialogueEvents.Keys, NeedsBasicInfo);
            yield return new LocalOrHostPlayerValueProvider(ConditionType.HasDialogueAnswer, this.GetDialogueAnswers, NeedsBasicInfo);
            yield return new LocalOrHostPlayerValueProvider(ConditionType.HasFlag, this.GetFlags, NeedsBasicInfo);
            yield return new LocalOrHostPlayerValueProvider(ConditionType.HasProfession, this.GetProfessions, NeedsBasicInfo);
            yield return new LocalOrHostPlayerValueProvider(ConditionType.HasReadLetter, player => player.mailReceived, NeedsBasicInfo);
            yield return new LocalOrHostPlayerValueProvider(ConditionType.HasSeenEvent, this.GetEventsSeen, NeedsBasicInfo);
            yield return new ConditionTypeValueProvider(ConditionType.HasWalletItem, this.GetWalletItems, NeedsBasicInfo, allowedValues: Enum.GetNames(typeof(WalletItem)));
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

            // number manipulation
            yield return new QueryValueProvider();
            yield return new RangeValueProvider();
            yield return new RoundValueProvider();

            // string manipulation
            yield return new LetterCaseValueProvider(ConditionType.Lowercase);
            yield return new LetterCaseValueProvider(ConditionType.Uppercase);

            // metadata
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
        /// <param name="player">The player whose values to get.</param>
        private IEnumerable<string> GetEventsSeen(Farmer player)
        {
            return player.eventsSeen
                .OrderBy(p => p)
                .Select(p => p.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>Get the letter IDs, mail flags, and world state IDs set for the player.</summary>
        /// <param name="player">The player whose values to get.</param>
        /// <remarks>See mail logic in <see cref="Farmer.hasOrWillReceiveMail"/>.</remarks>
        private IEnumerable<string> GetFlags(Farmer player)
        {
            // mail flags
            foreach (string flag in player.mailReceived.Union(player.mailForTomorrow).Union(player.mailbox))
                yield return flag;

            // world state flags
            foreach (string flag in Game1.worldStateIDs)
                yield return flag;
        }

        /// <summary>Get the professions for the player.</summary>
        /// <param name="player">The player whose values to get.</param>
        private IEnumerable<string> GetProfessions(Farmer player)
        {
            foreach (int professionID in player.professions)
            {
                yield return Enum.IsDefined(typeof(Profession), professionID)
                    ? ((Profession)professionID).ToString()
                    : professionID.ToString();
            }
        }

        /// <summary>Get the wallet items for the current player.</summary>
        private IEnumerable<string> GetWalletItems()
        {
            Farmer player = Game1.player;
            if (player == null)
                yield break;

            if (player.canUnderstandDwarves)
                yield return WalletItem.DwarvishTranslationGuide.ToString();
            if (player.hasRustyKey)
                yield return WalletItem.RustyKey.ToString();
            if (player.hasClubCard)
                yield return WalletItem.ClubCard.ToString();
            if (player.hasSpecialCharm)
                yield return WalletItem.SpecialCharm.ToString();
            if (player.hasSkullKey)
                yield return WalletItem.SkullKey.ToString();
            if (player.hasMagnifyingGlass)
                yield return WalletItem.MagnifyingGlass.ToString();
            if (player.hasDarkTalisman)
                yield return WalletItem.DarkTalisman.ToString();
            if (player.hasMagicInk)
                yield return WalletItem.MagicInk.ToString();
            if (player.eventsSeen.Contains(2120303))
                yield return WalletItem.BearsKnowledge.ToString();
            if (player.eventsSeen.Contains(3910979))
                yield return WalletItem.SpringOnionMastery.ToString();
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
        /// <param name="player">The player whose values to get.</param>
        private IEnumerable<string> GetDialogueAnswers(Farmer player)
        {
            return player.dialogueQuestionsAnswered
                .OrderBy(p => p)
                .Select(p => p.ToString(CultureInfo.InvariantCulture));
        }
    }
}
