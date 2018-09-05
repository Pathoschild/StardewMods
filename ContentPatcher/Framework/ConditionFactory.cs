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
        ** Properties
        *********/
        /// <summary>The minimum day of month.</summary>
        private const int MinDay = 1;

        /// <summary>The maximum day of month.</summary>
        private const int MaxDay = 28;

        /// <summary>The valid condition values for tokenisable conditions.</summary>
        private readonly IDictionary<ConditionKey, InvariantHashSet> ValidValues = new Dictionary<ConditionKey, InvariantHashSet>
        {
            [ConditionKey.Day] = new InvariantHashSet(Enumerable.Range(ConditionFactory.MinDay, ConditionFactory.MaxDay).Select(p => p.ToString())),
            [ConditionKey.DayOfWeek] = new InvariantHashSet(Enum.GetNames(typeof(DayOfWeek))),
            [ConditionKey.Language] = new InvariantHashSet(Enum.GetNames(typeof(LocalizedContentManager.LanguageCode)).Where(p => p != LocalizedContentManager.LanguageCode.th.ToString())),
            [ConditionKey.Season] = new InvariantHashSet(new[] { "Spring", "Summer", "Fall", "Winter" }),
            [ConditionKey.Weather] = new InvariantHashSet(Enum.GetNames(typeof(Weather)))
        };

        /// <summary>Condition keys which are guaranteed to only have one value and can be used in conditions.</summary>
        private readonly HashSet<ConditionKey> TokenisableConditions = new HashSet<ConditionKey>(new[]
        {
            ConditionKey.Day,
            ConditionKey.DayOfWeek,
            ConditionKey.Language,
            ConditionKey.Season,
            ConditionKey.Weather,
            ConditionKey.Year
        });


        /*********
        ** Public methods
        *********/
        /// <summary>Get an empty condition set.</summary>
        public ConditionDictionary BuildEmpty()
        {
            return new ConditionDictionary(this.ValidValues);
        }

        /// <summary>Construct a condition context.</summary>
        /// <param name="locale">The current language.</param>
        public ConditionContext BuildContext(LocalizedContentManager.LanguageCode locale)
        {
            return new ConditionContext(locale, this.TokenisableConditions);
        }

        /// <summary>Get the valid values for a condition key.</summary>
        /// <param name="key">The condition keys.</param>
        public IEnumerable<string> GetValidValues(ConditionKey key)
        {
            return this.ValidValues.TryGetValue(key, out InvariantHashSet values)
                ? values
                : null;
        }
    }
}
