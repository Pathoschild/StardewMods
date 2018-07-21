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
        private readonly IDictionary<ConditionType, InvariantHashSet> Values = new Dictionary<ConditionType, InvariantHashSet>();

        /// <summary>Condition keys which are guaranteed to only have one value.</summary>
        private readonly HashSet<ConditionType> SingleValueConditions;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="locale">The current language.</param>
        /// <param name="singleValueConditions">Condition keys which are guaranteed to only have one value.</param>
        public ConditionContext(LocalizedContentManager.LanguageCode locale, HashSet<ConditionType> singleValueConditions)
        {
            this.SingleValueConditions = singleValueConditions;

            // set defaults
            this.Set(locale, null, null, null, null, null, null);
            this.Set(ConditionType.HasMod);
        }

        /// <summary>Update the current context.</summary>
        /// <param name="language">The current language.</param>
        /// <param name="date">The current in-game date (if applicable).</param>
        /// <param name="weather">The current in-game weather (if applicable).</param>
        /// <param name="dayEvent">The day event (e.g. wedding or festival) occurring today (if applicable).</param>
        /// <param name="spouse">The current player's internal spouse name (if applicable).</param>
        /// <param name="seenEvents">The event IDs which the player has seen.</param>
        /// <param name="mailFlags">The mail flags set for the player.</param>
        public void Set(LocalizedContentManager.LanguageCode language, SDate date, Weather? weather, string dayEvent, string spouse, int[] seenEvents, string[] mailFlags)
        {
            // optional date
            if (date != null)
            {
                this.Set(ConditionType.Day, date.Day.ToString(CultureInfo.InvariantCulture));
                this.Set(ConditionType.DayOfWeek, date.DayOfWeek.ToString().ToLower());
                this.Set(ConditionType.Season, date.Season.ToLower());
            }
            else
            {
                this.Set(ConditionType.Day);
                this.Set(ConditionType.DayOfWeek);
                this.Set(ConditionType.Season);
            }

            // other conditions
            this.Set(ConditionType.DayEvent, dayEvent);
            this.Set(ConditionType.HasFlag, mailFlags);
            this.Set(ConditionType.HasSeenEvent, seenEvents?.Select(p => p.ToString()).ToArray());
            this.Set(ConditionType.Language, language.ToString().ToLower());
            this.Set(ConditionType.Spouse, spouse);
            this.Set(ConditionType.Weather, weather?.ToString().ToLower());
        }

        /// <summary>Update the current context.</summary>
        /// <param name="key">The condition key to update.</param>
        /// <param name="values">The current values.</param>
        public void Set(ConditionType key, params string[] values)
        {
            // normalise
            values = values?.Where(p => p != null).ToArray() ?? new string[0];

            // validate
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
        public string GetValue(ConditionType key)
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
        public IEnumerable<string> GetValues(ConditionType key)
        {
            if (!this.Values.TryGetValue(key, out InvariantHashSet values))
                throw new NotSupportedException($"Unknown condition key '{key}'.");

            return values;
        }

        /// <summary>Get the current values for conditions guaranteed to have a single value.</summary>
        public IDictionary<ConditionType, string> GetSingleValueConditions()
        {
            IDictionary<ConditionType, string> data = new Dictionary<ConditionType, string>();

            foreach (KeyValuePair<ConditionType, InvariantHashSet> pair in this.Values)
            {
                if (this.SingleValueConditions.Contains(pair.Key))
                    data.Add(pair.Key, pair.Value.FirstOrDefault());
            }

            return data;
        }
    }
}
