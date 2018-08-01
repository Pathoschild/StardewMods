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

            // set defaults
            this.Set(locale, null, null, null, null, null, null, null);
            this.Set(ConditionKey.HasMod);
        }

        /// <summary>Update the current context.</summary>
        /// <param name="language">The current language.</param>
        /// <param name="date">The current in-game date (if applicable).</param>
        /// <param name="weather">The current in-game weather (if applicable).</param>
        /// <param name="dayEvent">The day event (e.g. wedding or festival) occurring today (if applicable).</param>
        /// <param name="spouse">The current player's internal spouse name (if applicable).</param>
        /// <param name="seenEvents">The event IDs which the player has seen.</param>
        /// <param name="mailFlags">The mail flags set for the player.</param>
        /// <param name="friendships">The current player's friendship details.</param>
        public void Set(LocalizedContentManager.LanguageCode language, SDate date, Weather? weather, string dayEvent, string spouse, int[] seenEvents, string[] mailFlags, IEnumerable<KeyValuePair<string, Friendship>> friendships)
        {
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

            // other basic conditions
            this.Set(ConditionKey.DayEvent, dayEvent);
            this.Set(ConditionKey.HasFlag, mailFlags);
            this.Set(ConditionKey.HasSeenEvent, seenEvents?.Select(p => p.ToString()).ToArray());
            this.Set(ConditionKey.Language, language.ToString().ToLower());
            this.Set(ConditionKey.Spouse, spouse);
            this.Set(ConditionKey.Weather, weather?.ToString().ToLower());

            // NPC friendship conditions
            this.RemoveAll(ConditionType.Hearts, ConditionType.Relationship);
            if (friendships != null)
            {
                foreach (KeyValuePair<string, Friendship> friendship in friendships)
                {
                    this.Set(new ConditionKey(ConditionType.Hearts, friendship.Key), (friendship.Value.Points / NPC.friendshipPointsPerHeartLevel).ToString());
                    this.Set(new ConditionKey(ConditionType.Relationship, friendship.Key), friendship.Value.Status.ToString());
                }
            }
        }

        /// <summary>Update the current context.</summary>
        /// <param name="key">The condition key to update.</param>
        /// <param name="values">The current values.</param>
        public void Set(ConditionKey key, params string[] values)
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

        /// <summary>Get all context values.</summary>
        public IDictionary<ConditionKey, IEnumerable<string>> GetValues()
        {
            return this.Values
                .ToDictionary(p => p.Key, p => (IEnumerable<string>)p.Value);
        }

        /// <summary>Get the context values for comparison.</summary>
        /// <param name="key">The context key.</param>
        public IEnumerable<string> GetValues(ConditionKey key)
        {
            if (this.Values.TryGetValue(key, out InvariantHashSet values))
                return values;
            return Enumerable.Empty<string>();
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


        /*********
        ** Private methods
        *********/
        /// <summary>Remove all values matching a given condition type.</summary>
        /// <param name="types">The condition types to remove.</param>
        private void RemoveAll(params ConditionType[] types)
        {
            this.RemoveAll(key => types.Contains(key.Type));
        }

        /// <summary>Remove all values matching a given lambda.</summary>
        /// <param name="keySelector">A lambda which returns whether a given key should be removed.</param>
        private void RemoveAll(Func<ConditionKey, bool> keySelector)
        {
            foreach (ConditionKey key in this.Values.Keys.Where(keySelector).ToArray())
                this.Values.Remove(key);
        }
    }
}
