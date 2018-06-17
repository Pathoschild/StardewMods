using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace ContentPatcher.Framework.Conditions
{
    /// <summary>The current condition context.</summary>
    internal class ConditionContext
    {
        /*********
        ** Properties
        *********/
        /// <summary>The condition values.</summary>
        private readonly IDictionary<ConditionKey, InvariantHashSet> Values = new Dictionary<ConditionKey, InvariantHashSet>();

        /// <summary>Condition keys which are guaranteed to only have one value.</summary>
        private readonly HashSet<ConditionKey> SingleValueConditions;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="locale">The current language.</param>
        /// <param name="singleValueConditions">Condition keys which are guaranteed to only have one value.</param>
        public ConditionContext(LocalizedContentManager.LanguageCode locale, HashSet<ConditionKey> singleValueConditions)
        {
            this.SingleValueConditions = singleValueConditions;
            this.Set(locale, null, null);
        }

        /// <summary>Update the current context.</summary>
        /// <param name="language">The current language.</param>
        /// <param name="date">The current in-game date (if applicable).</param>
        /// <param name="weather">The current in-game weather (if applicable).</param>
        public void Set(LocalizedContentManager.LanguageCode language, SDate date, Weather? weather)
        {
            // language
            this.Set(ConditionKey.Language, language.ToString().ToLower());

            // optional date
            if (date != null)
            {
                this.Set(ConditionKey.Day, date.Day.ToString(CultureInfo.InvariantCulture));
                this.Set(ConditionKey.DayOfWeek, date.DayOfWeek.ToString().ToLower());
                this.Set(ConditionKey.Season, date.Season.ToLower());
            }
            else
            {
                this.Set(ConditionKey.Day);
                this.Set(ConditionKey.DayOfWeek);
                this.Set(ConditionKey.Season);
            }

            // weather
            this.Set(ConditionKey.Weather, weather?.ToString().ToLower());
        }

        /// <summary>Update the current context.</summary>
        /// <param name="key">The condition key to update.</param>
        /// <param name="values">The current values.</param>
        public void Set(ConditionKey key, params string[] values)
        {
            // validate
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (this.SingleValueConditions.Contains(key) && values.Length > 1)
                throw new InvalidOperationException($"Can't set multiple values for condition {key}.");

            // add entry
            if (!this.Values.TryGetValue(key, out InvariantHashSet set))
            {
                set = new InvariantHashSet();
                this.Values[key] = set;
            }

            // update
            set.Clear();
            foreach (string value in values)
                set.Add(value);
        }

        /// <summary>Get a context value for comparison.</summary>
        /// <param name="key">The context key.</param>
        public string GetValue(ConditionKey key)
        {
            // validate
            if (!this.SingleValueConditions.Contains(key))
                throw new InvalidOperationException($"Can't get a single value for condition {key} because it's not guaranteed to have only one value.");
            if (!this.Values.TryGetValue(key, out InvariantHashSet values))
                throw new NotSupportedException($"Unknown condition key '{key}'.");
            if (values.Count > 1)
                throw new NotSupportedException($"Condition {key} has multiple values, which is not allowed for this condition.");

            return values.FirstOrDefault();
        }

        /// <summary>Get the context values for comparison.</summary>
        /// <param name="key">The context key.</param>
        public IEnumerable<string> GetValues(ConditionKey key)
        {
            if (!this.Values.TryGetValue(key, out InvariantHashSet values))
                throw new NotSupportedException($"Unknown condition key '{key}'.");

            return values;
        }

        /// <summary>Get the current values for conditions guaranteed to have a single value.</summary>
        public IDictionary<ConditionKey, string> GetSingleValueConditions()
        {
            IDictionary<ConditionKey, string> data = new Dictionary<ConditionKey, string>();

            foreach (KeyValuePair<ConditionKey, InvariantHashSet> pair in this.Values)
            {
                if (this.SingleValueConditions.Contains(pair.Key))
                    data.Add(pair.Key, pair.Value.FirstOrDefault());
            }

            return data;
        }
    }
}
