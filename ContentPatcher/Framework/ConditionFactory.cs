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

        /// <summary>The number of days in a week.</summary>
        private const int DaysPerWeek = 7;

        /// <summary>The valid condition values for tokenisable conditions.</summary>
        private readonly IDictionary<ConditionType, InvariantHashSet> ValidValues = new Dictionary<ConditionType, InvariantHashSet>
        {
            [ConditionType.Day] = new InvariantHashSet(Enumerable.Range(ConditionFactory.MinDay, ConditionFactory.MaxDay).Select(p => p.ToString())),
            [ConditionType.DayOfWeek] = new InvariantHashSet(Enum.GetNames(typeof(DayOfWeek))),
            [ConditionType.Language] = new InvariantHashSet(Enum.GetNames(typeof(LocalizedContentManager.LanguageCode)).Where(p => p != LocalizedContentManager.LanguageCode.th.ToString())),
            [ConditionType.Season] = new InvariantHashSet(new[] { "Spring", "Summer", "Fall", "Winter" }),
            [ConditionType.Weather] = new InvariantHashSet(Enum.GetNames(typeof(Weather)))
        };

        /// <summary>Condition keys which are guaranteed to only have one value and can be used in conditions.</summary>
        private readonly HashSet<ConditionType> TokenisableConditions = new HashSet<ConditionType>(new[] {
            ConditionType.Day,
            ConditionType.DayOfWeek,
            ConditionType.Language,
            ConditionType.Season,
            ConditionType.Weather
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

        /// <summary>Get all valid condition keys.</summary>
        public IEnumerable<ConditionType> GetValidConditions()
        {
            foreach (ConditionType key in Enum.GetValues(typeof(ConditionType)))
                yield return key;
        }

        /// <summary>Get all valid condition keys which can be used in tokens.</summary>
        public IEnumerable<ConditionType> GetTokenisableConditions()
        {
            return this.TokenisableConditions;
        }

        /// <summary>Get the valid values for a condition key.</summary>
        /// <param name="key">The condition keys.</param>
        public IEnumerable<string> GetValidValues(ConditionType key)
        {
            return this.ValidValues.TryGetValue(key, out InvariantHashSet values)
                ? values
                : null;
        }

        /// <summary>Get all possible values of a tokenable string.</summary>
        /// <param name="tokenable">The tokenable string.</param>
        /// <param name="conditions">The conditions for which to filter permutations.</param>
        public IEnumerable<string> GetPossibleStrings(TokenString tokenable, ConditionDictionary conditions)
        {
            // no tokens: return original string
            if (!tokenable.ConditionTokens.Any())
            {
                yield return tokenable.Raw;
                yield break;
            }

            // yield token permutations
            foreach (IDictionary<ConditionType, string> permutation in this.GetApplicablePermutationsForTheseConditions(tokenable.ConditionTokens, conditions))
                yield return tokenable.GetStringWithTokens(permutation);
        }

        /// <summary>Get the filtered permutations of conditions for the given keys.</summary>
        /// <param name="keys">The condition keys to include.</param>
        /// <param name="conditions">The conditions for which to filter permutations.</param>
        public IEnumerable<IDictionary<ConditionType, string>> GetApplicablePermutationsForTheseConditions(ISet<ConditionType> keys, ConditionDictionary conditions)
        {
            // get possible values for given conditions
            IDictionary<ConditionType, InvariantHashSet> possibleValues = this.GetPossibleTokenisableValues(conditions);

            // restrict permutations to relevant keys
            foreach (ConditionType key in possibleValues.Keys.ToArray())
            {
                if (!keys.Contains(key))
                    possibleValues.Remove(key);
            }

            // get permutations
            var rawValues = new InvariantDictionary<InvariantHashSet>(possibleValues.ToDictionary(p => p.Key.ToString(), p => p.Value));
            foreach (var permutation in this.GetPermutations(rawValues))
                yield return permutation.ToDictionary(p => (ConditionType)Enum.Parse(typeof(ConditionType), p.Key, ignoreCase: true), p => p.Value);
        }

        /// <summary>Get all days possible for the given day of week.</summary>
        /// <param name="dayOfWeek">The day of week.</param>
        public IEnumerable<int> GetDaysFor(DayOfWeek dayOfWeek)
        {
            return Enumerable
                .Range(ConditionFactory.MinDay, ConditionFactory.MaxDay)
                .Where(p => p % ConditionFactory.DaysPerWeek == (int)dayOfWeek);
        }

        /// <summary>Get the day of week for a given day.</summary>
        /// <param name="day">The day of month.</param>
        public DayOfWeek GetDayOfWeekFor(int day)
        {
            return (DayOfWeek)(day % ConditionFactory.DaysPerWeek);
        }

        /// <summary>Get all possible values of the given conditions, taking into account the interactions between them (e.g. day one isn't possible without mondays).</summary>
        /// <param name="conditions">The conditions to check.</param>
        public IDictionary<ConditionType, InvariantHashSet> GetPossibleTokenisableValues(ConditionDictionary conditions)
        {
            // get applicable values
            IDictionary<ConditionType, InvariantHashSet> values = new Dictionary<ConditionType, InvariantHashSet>();
            foreach (ConditionType key in this.GetTokenisableConditions())
                values[key] = new InvariantHashSet(conditions.GetImpliedValues(key));

            // filter days per days-of-week
            if (values[ConditionType.DayOfWeek].Count < this.GetValidValues(ConditionType.DayOfWeek).Count())
            {
                // get covered days
                HashSet<string> possibleDays = new HashSet<string>();
                foreach (string str in values[ConditionType.DayOfWeek])
                {
                    DayOfWeek dayOfWeek = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), str, ignoreCase: true);
                    foreach (int day in this.GetDaysFor(dayOfWeek))
                        possibleDays.Add(day.ToString());
                }

                // remove non-covered days
                values[ConditionType.Day].RemoveWhere(p => !possibleDays.Contains(p));
            }

            // filter days-of-week per days
            if (values[ConditionType.Day].Count < this.GetValidValues(ConditionType.Day).Count())
            {
                // get covered days of week
                HashSet<string> possibleDaysOfWeek = new HashSet<string>();
                foreach (string str in values[ConditionType.Day])
                {
                    int day = int.Parse(str);
                    possibleDaysOfWeek.Add(this.GetDayOfWeekFor(day).ToString());
                }

                // remove non-covered days of week
                values[ConditionType.DayOfWeek].RemoveWhere(p => !possibleDaysOfWeek.Contains(p));
            }

            return values;
        }

        /// <summary>Get whether two sets of conditions can potentially both match in some contexts.</summary>
        /// <param name="left">The first set of conditions to compare.</param>
        /// <param name="right">The second set of conditions to compare.</param>
        public bool CanConditionsOverlap(ConditionDictionary left, ConditionDictionary right)
        {
            IDictionary<ConditionType, InvariantHashSet> leftValues = this.GetPossibleTokenisableValues(left);
            IDictionary<ConditionType, InvariantHashSet> rightValues = this.GetPossibleTokenisableValues(right);

            foreach (ConditionType key in this.GetTokenisableConditions())
            {
                if (!leftValues[key].Intersect(rightValues[key], StringComparer.InvariantCultureIgnoreCase).Any())
                    return false;
            }

            return true;
        }

        /// <summary>Get every permutation of the given values.</summary>
        /// <param name="values">The possible values.</param>
        public IEnumerable<InvariantDictionary<string>> GetPermutations(InvariantDictionary<InvariantHashSet> values)
        {
            // no permutations possible
            if (!values.Any())
                return new InvariantDictionary<string>[0];

            // recursively find permutations
            InvariantDictionary<string> curPermutation = new InvariantDictionary<string>();
            IEnumerable<InvariantDictionary<string>> GetPermutations(string[] keyQueue)
            {
                if (!keyQueue.Any())
                {
                    yield return new InvariantDictionary<string>(curPermutation);
                    yield break;
                }

                string key = keyQueue[0];
                foreach (string value in values[key])
                {
                    curPermutation[key] = value;
                    foreach (var permutation in GetPermutations(keyQueue.Skip(1).ToArray()))
                        yield return permutation;
                }
            }

            return GetPermutations(values.Keys.ToArray());
        }
    }
}
