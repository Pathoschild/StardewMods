using System;
using System.Collections.Generic;
using System.Globalization;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace ContentPatcher.Framework.Conditions
{
    /// <summary>The current condition context.</summary>
    internal class ConditionContext
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The condition values.</summary>
        public IDictionary<ConditionKey, string> Values { get; } = new Dictionary<ConditionKey, string>();



        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="locale">The current language.</param>
        public ConditionContext(LocalizedContentManager.LanguageCode locale)
        {
            this.Update(locale, null);
        }

        /// <summary>Update the current context.</summary>
        /// <param name="language">The current language.</param>
        /// <param name="date">The current in-game date (if applicable).</param>
        public void Update(LocalizedContentManager.LanguageCode language, SDate date)
        {
            // language
            this.Values[ConditionKey.Language] = language.ToString().ToLower();

            // optional date
            this.Values[ConditionKey.Day] = date?.Day.ToString(CultureInfo.InvariantCulture);
            this.Values[ConditionKey.DayOfWeek] = date?.DayOfWeek.ToString().ToLower();
            this.Values[ConditionKey.Season] = date?.Season.ToLower();
        }

        /// <summary>Get a context value for comparison.</summary>
        /// <param name="key">The context key.</param>
        public string GetValue(ConditionKey key)
        {
            if (this.Values.TryGetValue(key, out string value))
                return value;

            throw new NotSupportedException($"Unknown condition key '{key}'.");
        }
    }
}
