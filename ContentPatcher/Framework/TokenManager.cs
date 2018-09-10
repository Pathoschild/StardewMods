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
        private readonly InvariantDictionary<IToken> GlobalTokens = new InvariantDictionary<IToken>();

        /// <summary>The available tokens defined within the context of each content pack.</summary>
        private readonly Dictionary<IContentPack, LocalContext> LocalTokens = new Dictionary<IContentPack, LocalContext>();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="contentHelper">The content helper from which to load data assets.</param>
        public TokenManager(IContentHelper contentHelper)
        {
            foreach (IToken token in this.GetGlobalTokens(contentHelper))
                this.GlobalTokens[token.Name] = token;
        }

        /// <summary>Set the list of installed mods and content packs.</summary>
        /// <param name="installedMods">The installed mod IDs.</param>
        public void SetInstalledMods(string[] installedMods)
        {
            this.GlobalTokens[ConditionType.HasMod.ToString()] = new StaticToken(name: ConditionType.HasMod.ToString(), canHaveMultipleValues: true, values: new InvariantHashSet(installedMods));
        }

        /// <summary>Get the tokens which are defined for a specific content pack. This returns a reference to the list, which can be held for a live view of the tokens. If the content pack isn't currently tracked, this will add it.</summary>
        /// <param name="contentPack">The content pack to manage.</param>
        public LocalContext TrackLocalTokens(IContentPack contentPack)
        {
            if (!this.LocalTokens.TryGetValue(contentPack, out LocalContext localTokens))
                this.LocalTokens[contentPack] = localTokens = new LocalContext(this);
            return localTokens;
        }

        /// <summary>Update the current context.</summary>
        public void UpdateContext()
        {
            foreach (IToken token in this.GlobalTokens.Values)
                token.UpdateContext(this);

            foreach (LocalContext localContext in this.LocalTokens.Values)
            {
                foreach (IToken localToken in localContext.LocalTokens.Values)
                    localToken.UpdateContext(localContext);
            }
        }

        /****
        ** IContext
        ****/
        /// <summary>Get the underlying token which handles a key.</summary>
        /// <param name="key">The token key.</param>
        /// <returns>Returns the matching token, or <c>null</c> if none was found.</returns>
        public IToken GetToken(TokenKey key)
        {
            return this.GlobalTokens.TryGetValue(key.Key, out IToken token)
                ? token
                : null;
        }

        /// <summary>Get the underlying tokens.</summary>
        public IEnumerable<IToken> GetTokens()
        {
            foreach (IToken token in this.GlobalTokens.Values)
                yield return token;
        }

        /// <summary>Get the current value of the given token for comparison. This is only valid for tokens where <see cref="IToken.CanHaveMultipleValues"/> is false; see <see cref="IContext.GetValues(TokenKey)"/> otherwise.</summary>
        /// <param name="key">The token key.</param>
        /// <exception cref="ArgumentNullException">The specified key is null.</exception>
        /// <exception cref="ArgumentException">The specified key does includes or doesn't include a subkey, depending on <see cref="IToken.RequiresSubkeys"/>.</exception>
        /// <exception cref="InvalidOperationException">The specified token allows multiple values; see <see cref="IContext.GetValues(TokenKey)"/> instead.</exception>
        /// <exception cref="KeyNotFoundException">The specified token key doesn't exist.</exception>
        public string GetValue(TokenKey key)
        {
            IToken token = this.GetToken(key);
            this.AssertToken(key.Key, key.Subkey, token);

            if (token.CanHaveMultipleValues)
                throw new InvalidOperationException($"The {key} token allows multiple values, so {nameof(this.GetValue)} is not valid.");

            return key.Subkey != null
                ? token.GetValues(key.Subkey).FirstOrDefault()
                : token.GetValues().FirstOrDefault();
        }

        /// <summary>Get the current values of the given token for comparison.</summary>
        /// <param name="key">The token key.</param>
        /// <exception cref="ArgumentNullException">The specified key is null.</exception>
        /// <exception cref="KeyNotFoundException">The specified token key doesn't exist.</exception>
        public IEnumerable<string> GetValues(TokenKey key)
        {
            IToken token = this.GetToken(key);
            this.AssertToken(key.Key, key.Subkey, token);
            return key.Subkey != null
                ? token.GetValues(key.Subkey)
                : token.GetValues();
        }

        /// <summary>Get the tokens that can only contain one value.</summary>
        public InvariantDictionary<IToken> GetSingleValues()
        {
            InvariantDictionary<IToken> values = new InvariantDictionary<IToken>();

            foreach (IToken token in this.GetTokens())
            {
                if (token.CanHaveMultipleValues)
                    continue;

                values[token.Name] = token;
            }

            return values;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the global tokens with which to initialise the token manager.</summary>
        /// <param name="contentHelper">The content helper from which to load data assets.</param>
        private IEnumerable<IToken> GetGlobalTokens(IContentHelper contentHelper)
        {
            // installed mods (placeholder)
            yield return new StaticToken(ConditionType.HasMod.ToString(), canHaveMultipleValues: true, values: new InvariantHashSet());

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

        /// <summary>Assert that a token is valid and matches the key.</summary>
        /// <param name="tokenKey">The token key.</param>
        /// <param name="subkey">The token subkey (if any).</param>
        /// <param name="token">The resolved token.</param>
        /// <exception cref="ArgumentException">The specified key does includes or doesn't include a subkey, depending on <see cref="IToken.RequiresSubkeys"/>.</exception>
        /// <exception cref="KeyNotFoundException">The specified token key doesn't exist.</exception>
        /// <remarks>This implementation is duplicated by <see cref="LocalContext"/>.</remarks>
        private void AssertToken(string tokenKey, string subkey, IToken token)
        {
            if (token == null)
                throw new KeyNotFoundException($"There's no token with key {tokenKey}.");
            if (token.RequiresSubkeys && subkey == null)
                throw new InvalidOperationException($"The {tokenKey} token requires a subkey, but none was provided.");
            if (!token.RequiresSubkeys && subkey != null)
                throw new InvalidOperationException($"The {tokenKey} token doesn't allow a subkey, but a '{subkey}' subkey was provided.");
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
