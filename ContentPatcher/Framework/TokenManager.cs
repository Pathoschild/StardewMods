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
        private readonly Dictionary<IContentPack, ModTokenContext> LocalTokens = new();

        /// <summary>The installed mod IDs.</summary>
        private readonly InvariantHashSet InstalledMods;

        /// <summary>Whether the next context update is the first one.</summary>
        private bool IsFirstUpdate = true;


        /*********
        ** Accessors
        *********/
        /// <inheritdoc />
        public int UpdateTick { get; private set; }

        /// <summary>Whether the save file has been parsed into <see cref="SaveGame.loaded"/> (regardless of whether the game started loading it yet).</summary>
        public bool IsSaveParsed { get; set; }

        /// <summary>Whether the basic save info is loaded (including the date, weather, and player info). The in-game locations and world may not exist yet.</summary>
        public bool IsSaveBasicInfoLoaded { get; set; }

        /// <summary>The tokens which should always be used with a specific update rate.</summary>
        public Tuple<UpdateRate, string, InvariantHashSet>[] TokensWithSpecialUpdateRates { get; } = {
            Tuple.Create(UpdateRate.OnLocationChange, "location tokens", new InvariantHashSet { ConditionType.LocationContext.ToString(), ConditionType.LocationName.ToString(), ConditionType.LocationUniqueName.ToString(), ConditionType.IsOutdoors.ToString() }),
            Tuple.Create(UpdateRate.OnTimeChange, "time tokens", new InvariantHashSet { ConditionType.Time.ToString() })
        };


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="contentHelper">The content helper from which to load data assets.</param>
        /// <param name="installedMods">The installed mod IDs.</param>
        /// <param name="modTokens">The custom tokens provided by mods.</param>
        public TokenManager(IContentHelper contentHelper, InvariantHashSet installedMods, IEnumerable<IToken> modTokens)
        {
            this.InstalledMods = installedMods;
            this.GlobalContext = new GenericTokenContext(this.IsModInstalled, () => this.UpdateTick);

            foreach (IToken modToken in modTokens)
                this.GlobalContext.Save(modToken);
            foreach (IValueProvider valueProvider in this.GetGlobalValueProviders(contentHelper, installedMods))
                this.GlobalContext.Save(new Token(valueProvider));
        }

        /// <summary>Get the tokens which are defined for a specific content pack. This returns a reference to the list, which can be held for a live view of the tokens. If the content pack isn't currently tracked, this will add it.</summary>
        /// <param name="pack">The content pack to manage.</param>
        public ModTokenContext TrackLocalTokens(IContentPack pack)
        {
            string scope = pack.Manifest.UniqueID;

            if (!this.LocalTokens.TryGetValue(pack, out ModTokenContext localTokens))
            {
                this.LocalTokens[pack] = localTokens = new ModTokenContext(scope, this);
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
            this.UpdateTick++;

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

            // special case: language change implies i18n change
            if (changedGlobalTokens.Contains(ConditionType.Language.ToString()))
                changedGlobalTokens.Add(ConditionType.I18n.ToString());

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
            bool NeedsSave() => this.IsSaveParsed;
            var save = new TokenSaveReader(isSaveParsed: NeedsSave, isSaveBasicInfoLoaded: () => this.IsSaveBasicInfoLoaded);

            // date and weather
            yield return new ConditionTypeValueProvider(ConditionType.Day, () => save.GetDay().ToString(), NeedsSave, allowedValues: Enumerable.Range(0, 29).Select(p => p.ToString())); // day 0 = new-game intro
            yield return new ConditionTypeValueProvider(ConditionType.DayEvent, save.GetDayEvent, NeedsSave);
            yield return new ConditionTypeValueProvider(ConditionType.DayOfWeek, () => save.GetDayOfWeek().ToString(), NeedsSave, allowedValues: Enum.GetNames(typeof(DayOfWeek)));
            yield return new ConditionTypeValueProvider(ConditionType.DaysPlayed, () => save.GetDaysPlayed().ToString(), NeedsSave);
            yield return new ConditionTypeValueProvider(ConditionType.Season, save.GetSeason, NeedsSave, allowedValues: new[] { "Spring", "Summer", "Fall", "Winter" });
            yield return new ConditionTypeValueProvider(ConditionType.Year, () => save.GetYear().ToString(), NeedsSave);
            yield return new WeatherValueProvider(save);
            yield return new TimeValueProvider(save);

            // player
            yield return new LocalOrHostPlayerValueProvider(ConditionType.DailyLuck, player => save.GetDailyLuck(player).ToString(CultureInfo.InvariantCulture), save);
            yield return new LocalOrHostPlayerValueProvider(ConditionType.FarmhouseUpgrade, player => player.HouseUpgradeLevel.ToString(), save);
            yield return new LocalOrHostPlayerValueProvider(ConditionType.HasCaughtFish, player => player.fishCaught.Keys.Select(p => p.ToString()), save);
            yield return new LocalOrHostPlayerValueProvider(ConditionType.HasConversationTopic, player => player.activeDialogueEvents.Keys, save);
            yield return new LocalOrHostPlayerValueProvider(ConditionType.HasDialogueAnswer, player => player.dialogueQuestionsAnswered.Select(p => p.ToString()), save);
            yield return new LocalOrHostPlayerValueProvider(ConditionType.HasFlag, player => save.GetFlags(player), save);
            yield return new LocalOrHostPlayerValueProvider(ConditionType.HasProfession, player => player.professions.Select(id => ((Profession)id).ToString()), save);
            yield return new LocalOrHostPlayerValueProvider(ConditionType.HasReadLetter, player => player.mailReceived, save);
            yield return new LocalOrHostPlayerValueProvider(ConditionType.HasSeenEvent, player => player.eventsSeen.Select(p => p.ToString()), save);
            yield return new LocalOrHostPlayerValueProvider(ConditionType.HasActiveQuest, player => player.questLog.Select(p => p.id.Value.ToString()), save);
            yield return new ConditionTypeValueProvider(ConditionType.HasWalletItem, save.GetWalletItems, NeedsSave, allowedValues: Enum.GetNames(typeof(WalletItem)));
            yield return new ConditionTypeValueProvider(ConditionType.IsMainPlayer, () => Context.IsMainPlayer.ToString(), NeedsSave);
            yield return new ConditionTypeValueProvider(ConditionType.IsOutdoors, () => save.GetCurrentLocation()?.IsOutdoors.ToString(), NeedsSave);
            yield return new ConditionTypeValueProvider(ConditionType.LocationContext, () => save.GetCurrentLocationContext()?.ToString(), NeedsSave);
            yield return new ConditionTypeValueProvider(ConditionType.LocationName, () => save.GetCurrentLocation()?.Name, NeedsSave);
            yield return new ConditionTypeValueProvider(ConditionType.LocationUniqueName, () => save.GetCurrentLocation()?.NameOrUniqueName, NeedsSave);
            yield return new ConditionTypeValueProvider(ConditionType.PlayerGender, () => (save.GetCurrentPlayer().IsMale ? Gender.Male : Gender.Female).ToString(), NeedsSave);
            yield return new ConditionTypeValueProvider(ConditionType.PlayerName, () => save.GetCurrentPlayer().Name, NeedsSave);
            yield return new ConditionTypeValueProvider(ConditionType.PreferredPet, () => (save.GetCurrentPlayer().catPerson ? PetType.Cat : PetType.Dog).ToString(), NeedsSave);
            yield return new SkillLevelValueProvider(save);

            // relationships
            yield return new LocalOrHostPlayerValueProvider(ConditionType.ChildNames, player => save.GetChildValues(player, ConditionType.ChildNames), save);
            yield return new LocalOrHostPlayerValueProvider(ConditionType.ChildGenders, player => save.GetChildValues(player, ConditionType.ChildGenders), save);
            yield return new VillagerHeartsValueProvider(save);
            yield return new VillagerRelationshipValueProvider(save);
            yield return new ConditionTypeValueProvider(ConditionType.Spouse, save.GetSpouse, NeedsSave);

            // world
            yield return new ConditionTypeValueProvider(ConditionType.FarmCave, () => save.GetFarmCaveType().ToString(), NeedsSave);
            yield return new ConditionTypeValueProvider(ConditionType.FarmName, save.GetFarmName, NeedsSave);
            yield return new ConditionTypeValueProvider(ConditionType.FarmType, () => save.GetFarmType().ToString(), NeedsSave);
            yield return new ConditionTypeValueProvider(ConditionType.IsCommunityCenterComplete, () => save.GetIsCommunityCenterComplete().ToString(), NeedsSave);
            yield return new ConditionTypeValueProvider(ConditionType.IsJojaMartComplete, () => save.GetIsJojaMartComplete().ToString(), NeedsSave);
            yield return new HavingChildValueProvider(ConditionType.Pregnant, save);
            yield return new HavingChildValueProvider(ConditionType.HavingChild, save);

            // number manipulation
            yield return new CountValueProvider();
            yield return new QueryValueProvider();
            yield return new RandomValueProvider();
            yield return new RangeValueProvider();
            yield return new RoundValueProvider();

            // string manipulation
            yield return new LetterCaseValueProvider(ConditionType.Lowercase);
            yield return new LetterCaseValueProvider(ConditionType.Uppercase);
            yield return new PathPartValueProvider();
            yield return new RenderValueProvider();

            // metadata
            yield return new ImmutableValueProvider(ConditionType.HasMod.ToString(), installedMods, canHaveMultipleValues: true);
            yield return new HasValueValueProvider();
            yield return new ConditionTypeValueProvider(ConditionType.Language, () => contentHelper.CurrentLocaleConstant.ToString(), allowedValues: Enum.GetNames(typeof(LocalizedContentManager.LanguageCode)).Where(p => p != LocalizedContentManager.LanguageCode.th.ToString()));
        }

        /// <summary>Get the local value providers with which to initialize a local context.</summary>
        /// <param name="contentPack">The content pack for which to get tokens.</param>
        private IEnumerable<IValueProvider> GetLocalValueProviders(IContentPack contentPack)
        {
            yield return new FirstValidFileValueProvider(contentPack.HasFile);
            yield return new HasFileValueProvider(contentPack.HasFile);
            yield return new TranslationValueProvider(contentPack.Translation);
        }
    }
}
