using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using Pathoschild.Stardew.Common.Utilities;
using StardewValley;

namespace ContentPatcher.Framework
{
    /// <summary>Handles constructing, permuting, and updating condition dictionaries.</summary>
    internal class ConditionFactory
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The valid condition values.</summary>
        public readonly IDictionary<ConditionKey, InvariantHashSet> ValidValues = new Dictionary<ConditionKey, InvariantHashSet>
        {
            [ConditionKey.Day] = new InvariantHashSet(Enumerable.Range(1, 28).Select(p => p.ToString())),
            [ConditionKey.DayOfWeek] = new InvariantHashSet(Enum.GetNames(typeof(DayOfWeek))),
            [ConditionKey.Language] = new InvariantHashSet(Enum.GetNames(typeof(LocalizedContentManager.LanguageCode)).Where(p => p != LocalizedContentManager.LanguageCode.th.ToString())),
            [ConditionKey.Season] = new InvariantHashSet(new[] { "Spring", "Summer", "Fall", "Winter" }),
            [ConditionKey.Weather] = new InvariantHashSet(Enum.GetNames(typeof(Weather)))
        };


        /*********
        ** Public methods
        *********/
        /// <summary>Get an empty condition set.</summary>
        public ConditionDictionary BuildEmpty()
        {
            return new ConditionDictionary(this.ValidValues);
        }

        /// <summary>Get all valid condition keys.</summary>
        public IEnumerable<ConditionKey> GetValidConditions()
        {
            foreach (ConditionKey key in Enum.GetValues(typeof(ConditionKey)))
                yield return key;
        }

        /// <summary>Get the valid values for a condition key.</summary>
        /// <param name="key">The condition keys.</param>
        public IEnumerable<string> GetValidValues(ConditionKey key)
        {
            if (!this.ValidValues.TryGetValue(key, out InvariantHashSet values))
                throw new KeyNotFoundException($"No valid values defined for condition key {key}.");
            return values;
        }

        /// <summary>Get every permutation of the potential condition values.</summary>
        /// <param name="keys">The condition keys to include.</param>
        /// <param name="conditions">The conditions for which to filter permutations.</param>
        public IEnumerable<IDictionary<ConditionKey, string>> GetConditionPermutations(HashSet<ConditionKey> keys, ConditionDictionary conditions)
        {
            // no permutations possible
            if (!keys.Any())
                return new Dictionary<ConditionKey, string>[0];

            // recursively find permutations
            IDictionary<ConditionKey, string> curPermutation = new Dictionary<ConditionKey, string>();
            IEnumerable<IDictionary<ConditionKey, string>> GetPermutations(ConditionKey[] keyQueue)
            {
                if (!keyQueue.Any())
                    yield return new Dictionary<ConditionKey, string>(curPermutation);

                foreach (ConditionKey key in keyQueue)
                {
                    foreach (string value in conditions.GetImpliedValues(key))
                    {
                        curPermutation[key] = value;
                        foreach (var permutation in GetPermutations(keyQueue.Skip(1).ToArray()))
                            yield return permutation;
                    }
                }
            }

            return GetPermutations(keys.ToArray());
        }
    }
}
