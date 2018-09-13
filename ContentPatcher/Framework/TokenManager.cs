using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Tokens;
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
        ** Properties
        *********/
        /// <summary>The available global tokens.</summary>
        private readonly GenericTokenContext GlobalContext = new GenericTokenContext();

        /// <summary>The available tokens defined within the context of each content pack.</summary>
        private readonly Dictionary<IContentPack, ModTokenContext> LocalTokens = new Dictionary<IContentPack, ModTokenContext>();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="contentHelper">The content helper from which to load data assets.</param>
        public TokenManager(IContentHelper contentHelper)
        {
            foreach (IToken token in this.GetGlobalTokens(contentHelper))
                this.GlobalContext.Tokens[token.Name] = token;
        }

        /// <summary>Set the list of installed mods and content packs.</summary>
        /// <param name="installedMods">The installed mod IDs.</param>
        public void SetInstalledMods(string[] installedMods)
        {
            this.GlobalContext.Tokens[new TokenName(ConditionType.HasMod)] = new ImmutableToken(ConditionType.HasMod.ToString(), new InvariantHashSet(installedMods), canHaveMultipleValues: true);
        }

        /// <summary>Get the tokens which are defined for a specific content pack. This returns a reference to the list, which can be held for a live view of the tokens. If the content pack isn't currently tracked, this will add it.</summary>
        /// <param name="contentPack">The content pack to manage.</param>
        public ModTokenContext TrackLocalTokens(IContentPack contentPack)
        {
            if (!this.LocalTokens.TryGetValue(contentPack, out ModTokenContext localTokens))
            {
                this.LocalTokens[contentPack] = localTokens = new ModTokenContext(this);
                foreach (IToken token in this.GetLocalTokens(contentPack))
                    localTokens.Add(token);
            }

            return localTokens;
        }

        /// <summary>Update the current context.</summary>
        public void UpdateContext()
        {
            foreach (IToken token in this.GlobalContext.Tokens.Values)
            {
                if (token.IsMutable)
                    token.UpdateContext(this);
            }

            foreach (ModTokenContext localContext in this.LocalTokens.Values)
                localContext.UpdateContext(this);
        }

        /****
        ** IContext
        ****/
        /// <summary>Get whether the context contains the given token.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="enforceContext">Whether to only consider tokens that are available in the context.</param>
        public bool Contains(TokenName name, bool enforceContext)
        {
            return this.GlobalContext.Contains(name, enforceContext);
        }

        /// <summary>Get the underlying token which handles a key.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="enforceContext">Whether to only consider tokens that are available in the context.</param>
        /// <returns>Returns the matching token, or <c>null</c> if none was found.</returns>
        public IToken GetToken(TokenName name, bool enforceContext)
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
        /// <param name="enforceContext">Whether to only consider tokens that are available in the context.</param>
        /// <returns>Return the values of the matching token, or an empty list if the token doesn't exist.</returns>
        /// <exception cref="ArgumentNullException">The specified key is null.</exception>
        public IEnumerable<string> GetValues(TokenName name, bool enforceContext)
        {
            return this.GlobalContext.GetValues(name, enforceContext);
        }

        /// <summary>Get the tokens that can only contain one value.</summary>
        /// <param name="enforceContext">Whether to only consider tokens that are available in the context.</param>
        public IEnumerable<IToken> GetSingleValues(bool enforceContext)
        {
            return this.GlobalContext.GetSingleValues(enforceContext);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the global tokens with which to initialise the token manager.</summary>
        /// <param name="contentHelper">The content helper from which to load data assets.</param>
        private IEnumerable<IToken> GetGlobalTokens(IContentHelper contentHelper)
        {
            // installed mods (placeholder)
            // mark token mutable until the mod list is final
            yield return new ManualToken(new TokenName(ConditionType.HasMod.ToString()), isMutable: true)
            {
                CanHaveMultipleValues = true,
                Values = new InvariantHashSet()
            };

            // language
            yield return new ContextualToken(ConditionType.Language.ToString(), () => contentHelper.CurrentLocaleConstant.ToString(), needsLoadedSave: false)
            {
                AllowedValues = new InvariantHashSet(Enum.GetNames(typeof(LocalizedContentManager.LanguageCode)).Where(p => p != LocalizedContentManager.LanguageCode.th.ToString()))
            };

            // in-game date
            yield return new ContextualToken(ConditionType.Season.ToString(), () => SDate.Now().Season, needsLoadedSave: true)
            {
                AllowedValues = new InvariantHashSet(new[] { "Spring", "Summer", "Fall", "Winter" })
            };
            yield return new ContextualToken(ConditionType.Day.ToString(), () => SDate.Now().Day.ToString(CultureInfo.InvariantCulture), needsLoadedSave: true)
            {
                AllowedValues = new InvariantHashSet(Enumerable.Range(1, 28).Select(p => p.ToString()))
            };
            yield return new ContextualToken(ConditionType.DayOfWeek.ToString(), () => SDate.Now().DayOfWeek.ToString(), needsLoadedSave: true)
            {
                AllowedValues = new InvariantHashSet(Enum.GetNames(typeof(DayOfWeek)))
            };
            yield return new ContextualToken(ConditionType.Year.ToString(), () => SDate.Now().Year.ToString(CultureInfo.InvariantCulture), needsLoadedSave: true);

            // other in-game conditions
            yield return new ContextualToken(ConditionType.DayEvent.ToString(), () => this.GetDayEvent(contentHelper), needsLoadedSave: true);
            yield return new ContextualToken(ConditionType.HasFlag.ToString(), () => this.GetMailFlags(), needsLoadedSave: true);
            yield return new ContextualToken(ConditionType.HasSeenEvent.ToString(), () => this.GetEventsSeen(), needsLoadedSave: true);
            yield return new ContextualToken(ConditionType.Spouse.ToString(), () => Game1.player?.spouse, needsLoadedSave: true);
            yield return new ContextualToken(ConditionType.Weather.ToString(), () => this.GetCurrentWeather(), needsLoadedSave: true)
            {
                AllowedValues = new InvariantHashSet(Enum.GetNames(typeof(Weather)))
            };
            yield return new VillagerRelationshipToken();
            yield return new VillagerHeartsToken();
        }

        /// <summary>Get the local tokens with which to initialise a local context.</summary>
        /// <param name="contentPack">The content pack for which to get tokens.</param>
        private IEnumerable<IToken> GetLocalTokens(IContentPack contentPack)
        {
            yield return new HasFileToken(contentPack.DirectoryPath);
        }

        /// <summary>Get the current weather from the game state.</summary>
        private string GetCurrentWeather()
        {
            if (Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason) || Game1.weddingToday)
                return Weather.Sun.ToString();

            if (Game1.isSnowing)
                return Weather.Snow.ToString();
            if (Game1.isRaining)
                return (Game1.isLightning ? Weather.Storm : Weather.Rain).ToString();

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

        /// <summary>Get the letter IDs and mail flags set for the player.</summary>
        /// <remarks>See game logic in <see cref="Farmer.hasOrWillReceiveMail"/>.</remarks>
        private IEnumerable<string> GetMailFlags()
        {
            Farmer player = Game1.player;
            if (player == null)
                return new string[0];

            return player
                .mailReceived
                .Union(player.mailForTomorrow)
                .Union(player.mailbox);
        }

        /// <summary>Get the name for today's day event (e.g. wedding or festival) from the game data.</summary>
        /// <param name="contentHelper">The content helper from which to load festival data.</param>
        private string GetDayEvent(IContentHelper contentHelper)
        {
            // marriage
            if (Game1.weddingToday)
                return "wedding";

            // festival
            IDictionary<string, string> festivalDates = contentHelper.Load<Dictionary<string, string>>("Data\\Festivals\\FestivalDates", ContentSource.GameContent);
            if (festivalDates.TryGetValue($"{Game1.currentSeason}{Game1.dayOfMonth}", out string festivalName))
                return festivalName;

            return null;
        }
    }
}
