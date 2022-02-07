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
        private readonly InvariantDictionary<CachedContext> LocalTokens = new();

        /// <summary>The installed mod IDs.</summary>
        private readonly IInvariantSet InstalledMods;

        /// <summary>Whether the next context update is the first one.</summary>
        private bool IsFirstUpdate = true;

        /// <summary>A cached local context for a content pack.</summary>
        /// <param name="ContentPack">The content pack for which the context was created.</param>
        /// <param name="Context">The token context containing dynamic tokens and aliases for the content pack.</param>
        private record CachedContext(IContentPack ContentPack, ModTokenContext Context);


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
        public Tuple<UpdateRate, string, IInvariantSet>[] TokensWithSpecialUpdateRates { get; } = {
            Tuple.Create(UpdateRate.OnLocationChange, "location tokens", InvariantSets.From(new[] { nameof(ConditionType.LocationContext), nameof(ConditionType.LocationName), nameof(ConditionType.LocationUniqueName), nameof(ConditionType.IsOutdoors) })),
            Tuple.Create(UpdateRate.OnTimeChange, "time tokens", InvariantSets.FromValue(nameof(ConditionType.Time)))
        };


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="contentHelper">The content helper from which to load data assets.</param>
        /// <param name="installedMods">The installed mod IDs.</param>
        /// <param name="modTokens">The custom tokens provided by mods.</param>
        public TokenManager(IGameContentHelper contentHelper, IInvariantSet installedMods, IEnumerable<IToken> modTokens)
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
            string scope = pack.Manifest.UniqueID.Trim();

            if (!this.LocalTokens.TryGetValue(scope, out CachedContext? cached))
            {
                ModTokenContext context = new ModTokenContext(scope, this);
                this.LocalTokens[scope] = cached = new CachedContext(pack, context);

                foreach (IValueProvider valueProvider in this.GetLocalValueProviders(pack))
                    context.AddLocalToken(new Token(valueProvider, scope));
            }

            return cached.Context;
        }

        /// <summary>Get the actual name referenced by a token alias.</summary>
        /// <param name="contentPackID">The content pack ID whose aliases to check.</param>
        /// <param name="tokenName">The token name to resolve.</param>
        /// <returns>Returns the resolved token name, or the input token name if it's not an alias.</returns>
        public string ResolveAlias(string contentPackID, string tokenName)
        {
            return this.LocalTokens.TryGetValue(contentPackID, out CachedContext? cached)
                ? cached.Context.ResolveAlias(tokenName)
                : tokenName;
        }

        /// <summary>Get the token context for a given mod ID.</summary>
        /// <param name="contentPackID">The content pack ID to search for.</param>
        /// <exception cref="KeyNotFoundException">There's no content pack registered with the given <paramref name="contentPackID"/>.</exception>
        public IContext GetContextFor(string contentPackID)
        {
            contentPackID = contentPackID.Trim();

            return this.LocalTokens.TryGetValue(contentPackID, out CachedContext? cached)
                ? cached.Context
                : throw new KeyNotFoundException($"There's no content pack registered for ID '{contentPackID}'.");
        }

        /// <summary>Update the current context.</summary>
        /// <param name="changedGlobalTokens">The global tokens which changed value.</param>
        public void UpdateContext(out IInvariantSet changedGlobalTokens)
        {
            this.UpdateTick++;

            // update tokens
            {
                MutableInvariantSet changedTokens = new();

                // update global tokens
                foreach (IToken token in this.GlobalContext.Tokens.Values)
                {
                    bool changed =
                        (token.IsMutable && token.UpdateContext(this)) // token changed state/value
                        || (this.IsFirstUpdate && token.IsReady); // tokens implicitly change to ready on their first update, even if they were ready from creation
                    if (changed)
                        changedTokens.Add(token.Name);
                }

                // special case: language change implies i18n change
                if (changedTokens.Contains(nameof(ConditionType.Language)))
                    changedTokens.Add(ConditionType.I18n.ToString());

                changedGlobalTokens = changedTokens.Lock();
            }

            // update mod contexts
            foreach (CachedContext cached in this.LocalTokens.Values)
                cached.Context.UpdateContext(changedGlobalTokens);

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
        public IToken? GetToken(string name, bool enforceContext)
        {
            return this.GlobalContext.GetToken(name, enforceContext);
        }

        /// <inheritdoc />
        public IEnumerable<IToken> GetTokens(bool enforceContext)
        {
            return this.GlobalContext.GetTokens(enforceContext);
        }

        /// <inheritdoc />
        public IInvariantSet GetValues(string name, IInputArguments input, bool enforceContext)
        {
            return this.GlobalContext.GetValues(name, input, enforceContext);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the global value providers with which to initialize the token manager.</summary>
        /// <param name="contentHelper">The content helper from which to load data assets.</param>
        /// <param name="installedMods">The installed mod IDs.</param>
        private IEnumerable<IValueProvider> GetGlobalValueProviders(IGameContentHelper contentHelper, IInvariantSet installedMods)
        {
            bool NeedsSave() => this.IsSaveParsed;
            var save = new TokenSaveReader(updateTick: () => this.UpdateTick, isSaveParsed: NeedsSave, isSaveBasicInfoLoaded: () => this.IsSaveBasicInfoLoaded);

            return new IValueProvider[]
            {
                // date and weather
                new ConditionTypeValueProvider(ConditionType.Day, () => save.GetDay().ToString(), NeedsSave, allowedValues: Enumerable.Range(0, 29).Select(p => p.ToString())), // day 0 = new-game intro
                new ConditionTypeValueProvider(ConditionType.DayEvent, save.GetDayEvent, NeedsSave),
                new ConditionTypeValueProvider(ConditionType.DayOfWeek, () => save.GetDayOfWeek().ToString(), NeedsSave, allowedValues: Enum.GetNames(typeof(DayOfWeek))),
                new ConditionTypeValueProvider(ConditionType.DaysPlayed, () => save.GetDaysPlayed().ToString(), NeedsSave),
                new ConditionTypeValueProvider(ConditionType.Season, save.GetSeason, NeedsSave, allowedValues: new[] { "Spring", "Summer", "Fall", "Winter" }),
                new ConditionTypeValueProvider(ConditionType.Year, () => save.GetYear().ToString(), NeedsSave),
                new WeatherValueProvider(save),
                new TimeValueProvider(save),

                // player
                new PerPlayerValueProvider(ConditionType.DailyLuck, player => save.GetDailyLuck(player).ToString(CultureInfo.InvariantCulture), save),
                new PerPlayerValueProvider(ConditionType.FarmhouseUpgrade, player => player.HouseUpgradeLevel.ToString(), save),
                new PerPlayerValueProvider(ConditionType.HasCaughtFish, player => player.fishCaught.Keys.Select(p => p.ToString()), save),
                new PerPlayerValueProvider(ConditionType.HasConversationTopic, player => player.activeDialogueEvents.Keys, save),
                new PerPlayerValueProvider(ConditionType.HasCookingRecipe, player => player.cookingRecipes.Keys, save),
                new PerPlayerValueProvider(ConditionType.HasCraftingRecipe, player => player.craftingRecipes.Keys, save),
                new PerPlayerValueProvider(ConditionType.HasDialogueAnswer, player => player.dialogueQuestionsAnswered.Select(p => p.ToString()), save),
                new PerPlayerValueProvider(ConditionType.HasFlag, player => save.GetFlags(player), save),
                new PerPlayerValueProvider(ConditionType.HasProfession, player => player.professions.Select(id => ((Profession)id).ToString()), save),
                new PerPlayerValueProvider(ConditionType.HasReadLetter, player => player.mailReceived, save),
                new PerPlayerValueProvider(ConditionType.HasSeenEvent, player => player.eventsSeen.Select(p => p.ToString()), save),
                new PerPlayerValueProvider(ConditionType.HasActiveQuest, player => player.questLog.Select(p => p.id.Value.ToString()), save),
                new ConditionTypeValueProvider(ConditionType.HasWalletItem, save.GetWalletItems, NeedsSave, allowedValues: Enum.GetNames(typeof(WalletItem))),
                new PerPlayerValueProvider(ConditionType.IsMainPlayer, player => player.IsMainPlayer.ToString(), save),
                new PerPlayerValueProvider(ConditionType.IsOutdoors, player => save.GetCurrentLocation(player)?.IsOutdoors.ToString(), save),
                new PerPlayerValueProvider(ConditionType.LocationContext, player => save.GetCurrentLocationContext(player).ToString(), save),
                new PerPlayerValueProvider(ConditionType.LocationName, player => save.GetCurrentLocation(player)?.Name, save),
                new PerPlayerValueProvider(ConditionType.LocationOwnerId, player => save.GetLocationOwnerId(save.GetCurrentLocation(player))?.ToString(), save),
                new PerPlayerValueProvider(ConditionType.LocationUniqueName, player => save.GetCurrentLocation(player)?.NameOrUniqueName, save),
                new PerPlayerValueProvider(ConditionType.PlayerGender, player => (player.IsMale ? Gender.Male : Gender.Female).ToString(), save),
                new PerPlayerValueProvider(ConditionType.PlayerName, player => player.Name, save),
                new ConditionTypeValueProvider(ConditionType.PreferredPet, () => (save.GetCurrentPlayer()?.catPerson == true ? PetType.Cat : PetType.Dog).ToString(), NeedsSave),
                new SkillLevelValueProvider(save),

                // relationships
                new PerPlayerValueProvider(ConditionType.ChildNames, player => save.GetChildValues(player, ConditionType.ChildNames), save),
                new PerPlayerValueProvider(ConditionType.ChildGenders, player => save.GetChildValues(player, ConditionType.ChildGenders), save),
                new VillagerHeartsValueProvider(save),
                new VillagerRelationshipValueProvider(save),
                new PerPlayerValueProvider(ConditionType.Roommate, player => player.hasRoommate() ? player.spouse : null, save),
                new PerPlayerValueProvider(ConditionType.Spouse, player => !player.hasRoommate() ? player.spouse : null, save),

                // world
                new ConditionTypeValueProvider(ConditionType.FarmCave, () => save.GetFarmCaveType().ToString(), NeedsSave),
                new ConditionTypeValueProvider(ConditionType.FarmName, save.GetFarmName, NeedsSave),
                new ConditionTypeValueProvider(ConditionType.FarmType, () => save.GetFarmType(), NeedsSave),
                new ConditionTypeValueProvider(ConditionType.IsCommunityCenterComplete, () => save.GetIsCommunityCenterComplete().ToString(), NeedsSave),
                new ConditionTypeValueProvider(ConditionType.IsJojaMartComplete, () => save.GetIsJojaMartComplete().ToString(), NeedsSave),
                new HavingChildValueProvider(ConditionType.Pregnant, save),
                new HavingChildValueProvider(ConditionType.HavingChild, save),

                // number manipulation
                new CountValueProvider(),
                new QueryValueProvider(),
                new RandomValueProvider(),
                new RangeValueProvider(),
                new RoundValueProvider(),

                // string manipulation
                new LetterCaseValueProvider(ConditionType.Lowercase),
                new LetterCaseValueProvider(ConditionType.Uppercase),
                new MergeValueProvider(),
                new PathPartValueProvider(),
                new RenderValueProvider(),

                // metadata
                new ImmutableValueProvider(nameof(ConditionType.HasMod), installedMods, canHaveMultipleValues: true),
                new HasValueValueProvider(),
                new ConditionTypeValueProvider(ConditionType.Language, () => this.GetLanguage(contentHelper)),

                // specialized
                new FormatAssetNameValueProvider()
            };
        }

        /// <summary>Get the local value providers with which to initialize a local context.</summary>
        /// <param name="contentPack">The content pack for which to get tokens.</param>
        private IEnumerable<IValueProvider> GetLocalValueProviders(IContentPack contentPack)
        {
            return new IValueProvider[]
            {
                new AbsoluteFilePathValueProvider(contentPack.DirectoryPath),
                new FirstValidFileValueProvider(contentPack.HasFile),
                new HasFileValueProvider(contentPack.HasFile),
                new InternalAssetKeyValueProvider(contentPack.ModContent.GetInternalAssetName),
                new TranslationValueProvider(contentPack.Translation)
            };
        }

        /// <summary>Get the current language code.</summary>
        /// <param name="contentHelper">The content helper from which to get the locale.</param>
        private IEnumerable<string> GetLanguage(IGameContentHelper contentHelper)
        {
            // get vanilla language
            LocalizedContentManager.LanguageCode language = contentHelper.CurrentLocaleConstant;
            string code = language.ToString();

            // handle custom language
            if (language == LocalizedContentManager.LanguageCode.mod)
                code = contentHelper.CurrentLocale ?? code;

            return new[] { code };
        }
    }
}
