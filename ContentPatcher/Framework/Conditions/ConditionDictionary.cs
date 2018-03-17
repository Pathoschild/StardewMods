using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;

namespace ContentPatcher.Framework.Conditions
{
    /// <summary>A set of conditions that can be checked against the context.</summary>
    internal class ConditionDictionary : Dictionary<ConditionKey, Condition>
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The valid condition values.</summary>
        public readonly IDictionary<ConditionKey, HashSet<string>> ValidValues = new Dictionary<ConditionKey, HashSet<string>>
        {
            [ConditionKey.Day] = new HashSet<string>(Enumerable.Range(1, 28).Select(p => p.ToString())),
            [ConditionKey.DayOfWeek] = new HashSet<string>((from string name in Enum.GetNames(typeof(DayOfWeek)) select name.ToLower()), StringComparer.InvariantCultureIgnoreCase),
            [ConditionKey.Language] = new HashSet<string>((from string name in Enum.GetNames(typeof(LocalizedContentManager.LanguageCode)) where name != LocalizedContentManager.LanguageCode.th.ToString() select name.ToLower()), StringComparer.InvariantCultureIgnoreCase),
            [ConditionKey.Season] = new HashSet<string>(new[] { "spring", "summer", "fall", "winter" }, StringComparer.InvariantCultureIgnoreCase)
        };


        /*********
        ** Public methods
        *********/
        /// <summary>Get the explicit values for a condition, or the implied range of values if not explicitly set.</summary>
        /// <param name="key">The condition key.</param>
        public IEnumerable<string> GetImpliedValues(ConditionKey key)
        {
            // explicit values
            if (this.TryGetValue(key, out Condition condition))
            {
                foreach (string value in condition.Values)
                    yield return value;
            }

            // implied range
            else
            {
                foreach (string value in this.ValidValues[key])
                    yield return value;
            }
        }
    }
}
