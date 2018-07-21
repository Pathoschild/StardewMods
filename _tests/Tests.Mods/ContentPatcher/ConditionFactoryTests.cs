using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework;
using ContentPatcher.Framework.Conditions;
using FluentAssertions;
using NUnit.Framework;
using Pathoschild.Stardew.Common.Utilities;
using StardewValley;

namespace Pathoschild.Stardew.Tests.Mods.ContentPatcher
{
    /// <summary>Unit tests for <see cref="ConditionFactory"/>.</summary>
    [TestFixture]
    internal class ConditionFactoryTests
    {
        /*********
        ** Properties
        *********/
        /// <summary>All possible values for each condition as a sorted and comma-delimited string.</summary>
        private readonly IDictionary<ConditionKey, string> CommaDelimitedValues = new Dictionary<ConditionKey, string>
        {
            [ConditionKey.Day] = "1, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 2, 20, 21, 22, 23, 24, 25, 26, 27, 28, 3, 4, 5, 6, 7, 8, 9",
            [ConditionKey.DayOfWeek] = $"{DayOfWeek.Friday}, {DayOfWeek.Monday}, {DayOfWeek.Saturday}, {DayOfWeek.Sunday}, {DayOfWeek.Thursday}, {DayOfWeek.Tuesday}, {DayOfWeek.Wednesday}",
            [ConditionKey.Language] = $"{LocalizedContentManager.LanguageCode.de}, {LocalizedContentManager.LanguageCode.en}, {LocalizedContentManager.LanguageCode.es}, {LocalizedContentManager.LanguageCode.ja}, {LocalizedContentManager.LanguageCode.pt}, {LocalizedContentManager.LanguageCode.ru}, {LocalizedContentManager.LanguageCode.zh}",
            [ConditionKey.Season] = "Fall, Spring, Summer, Winter",
            [ConditionKey.Weather] = $"{Weather.Rain}, {Weather.Snow}, {Weather.Storm}, {Weather.Sun}"
        };

        /// <summary>Condition keys which are guaranteed to only have one value and can be used in conditions.</summary>
        private readonly IEnumerable<ConditionKey> TokenisableConditions = new[] { ConditionKey.Day, ConditionKey.DayOfWeek, ConditionKey.Language, ConditionKey.Season, ConditionKey.Weather };

        /// <summary>The valid days of week.</summary>
        private readonly IEnumerable<DayOfWeek> DaysOfWeek = (from DayOfWeek value in Enum.GetValues(typeof(DayOfWeek)) select value).ToArray();

        /// <summary>The valid season names.</summary>
        private readonly IEnumerable<string> Seasons = new[] { "Spring", "Summer", "Fall", "Winter" };


        /*********
        ** Unit tests
        *********/
        /****
        ** GetValidValues
        ****/
        /// <summary>Test that <see cref="ConditionFactory.GetValidValues"/> returns the expected values.</summary>
        [TestCase(ConditionType.Day)]
        [TestCase(ConditionType.DayOfWeek)]
        [TestCase(ConditionType.Language)]
        [TestCase(ConditionType.Season)]
        [TestCase(ConditionType.Weather)]
        public void GetValidValues(ConditionType conditionType)
        {
            // arrange
            ConditionKey conditionKey = new ConditionKey(conditionType);

            // act
            IEnumerable<string> values = new ConditionFactory().GetValidValues(conditionKey);

            // assert
            this.SortAndCommaDelimit(values).Should().Be(this.CommaDelimitedValues[conditionKey]);
        }

        /****
        ** GetDaysFor
        ****/
        /// <summary>Test that <see cref="ConditionFactory.GetDaysFor"/> returns the expected values.</summary>
        [TestCase(DayOfWeek.Monday, ExpectedResult = "1, 8, 15, 22")]
        [TestCase(DayOfWeek.Tuesday, ExpectedResult = "2, 9, 16, 23")]
        [TestCase(DayOfWeek.Wednesday, ExpectedResult = "3, 10, 17, 24")]
        [TestCase(DayOfWeek.Thursday, ExpectedResult = "4, 11, 18, 25")]
        [TestCase(DayOfWeek.Friday, ExpectedResult = "5, 12, 19, 26")]
        [TestCase(DayOfWeek.Saturday, ExpectedResult = "6, 13, 20, 27")]
        [TestCase(DayOfWeek.Sunday, ExpectedResult = "7, 14, 21, 28")]
        public string GetDaysFor(DayOfWeek dayOfWeek)
        {
            // act
            IEnumerable<int> days = new ConditionFactory().GetDaysFor(dayOfWeek);

            // assert
            return this.CommaDelimit(days);
        }

        /****
        ** GetDayOfWeekFor
        ****/
        /// <summary>Test that <see cref="ConditionFactory.GetDayOfWeekFor"/> returns the expected values.</summary>
        [TestCase(DayOfWeek.Monday, new[] { 1, 8, 15, 22 })]
        [TestCase(DayOfWeek.Tuesday, new[] { 2, 9, 16, 23 })]
        [TestCase(DayOfWeek.Wednesday, new[] { 3, 10, 17, 24 })]
        [TestCase(DayOfWeek.Thursday, new[] { 4, 11, 18, 25 })]
        [TestCase(DayOfWeek.Friday, new[] { 5, 12, 19, 26 })]
        [TestCase(DayOfWeek.Saturday, new[] { 6, 13, 20, 27 })]
        [TestCase(DayOfWeek.Sunday, new[] { 7, 14, 21, 28 })]
        public void GetDayOfWeekFor(DayOfWeek expectedDayOfWeek, IEnumerable<int> days)
        {
            // arrange
            ConditionFactory factory = new ConditionFactory();
            foreach (int day in days)
            {
                DayOfWeek actual = factory.GetDayOfWeekFor(day);

                // assert
                actual.Should().Be(expectedDayOfWeek, $"{nameof(ConditionFactory.GetDayOfWeekFor)} should return {expectedDayOfWeek} for day {day}");
            }
        }

        /****
        ** GetPossibleValues
        ****/
        /// <summary>Test that <see cref="ConditionFactory.GetPossibleTokenisableValues"/> returns the expected values given only implied conditions.</summary>
        [TestCase]
        public void GetPossibleValues_WithOnlyImpliedConditions()
        {
            // arrange
            ConditionFactory factory = new ConditionFactory();
            ConditionDictionary conditions = factory.BuildEmpty();

            // act
            IDictionary<ConditionKey, InvariantHashSet> possibleValues = factory.GetPossibleTokenisableValues(conditions);

            // assert
            foreach (ConditionKey key in this.TokenisableConditions)
                this.SortAndCommaDelimit(possibleValues[key]).Should().Be(this.CommaDelimitedValues[key], $"should match for {key}");
        }

        /// <summary>Test that <see cref="ConditionFactory.GetPossibleTokenisableValues"/> returns the expected values given only implied conditions and restricted days of week.</summary>
        [TestCase]
        public void GetPossibleValues_WithImpliedConditionsAndRestrictedDaysOfWeek()
        {
            // arrange
            ConditionFactory factory = new ConditionFactory();
            ConditionDictionary conditions = factory.BuildEmpty();
            conditions.Add(ConditionKey.DayOfWeek, new[] { DayOfWeek.Tuesday.ToString(), DayOfWeek.Wednesday.ToString() });

            // act
            IDictionary<ConditionKey, InvariantHashSet> possibleValues = factory.GetPossibleTokenisableValues(conditions);

            // assert
            this.SortAndCommaDelimit(possibleValues[ConditionKey.Day]).Should().Be("10, 16, 17, 2, 23, 24, 3, 9", $"should match for {ConditionType.Day}");
            this.SortAndCommaDelimit(possibleValues[ConditionKey.DayOfWeek]).Should().Be($"{DayOfWeek.Tuesday}, {DayOfWeek.Wednesday}", $"should match for {ConditionType.DayOfWeek}");
            this.SortAndCommaDelimit(possibleValues[ConditionKey.Language]).Should().Be(this.CommaDelimitedValues[ConditionKey.Language], $"should match for {ConditionType.Language}");
            this.SortAndCommaDelimit(possibleValues[ConditionKey.Season]).Should().Be(this.CommaDelimitedValues[ConditionKey.Season], $"should match for {ConditionType.Season}");
            this.SortAndCommaDelimit(possibleValues[ConditionKey.Weather]).Should().Be(this.CommaDelimitedValues[ConditionKey.Weather], $"should match for {ConditionType.Weather}");
        }

        /// <summary>Test that <see cref="ConditionFactory.GetPossibleTokenisableValues"/> returns the expected values given a subset of each condition's possible values.</summary>
        [TestCase]
        public void GetPossibleValues_WithSubsetForEachCondition()
        {
            // arrange
            ConditionFactory factory = new ConditionFactory();
            ConditionDictionary conditions = factory.BuildEmpty();
            conditions.Add(ConditionKey.Day, new[] { "1"/*Monday*/, "2"/*Tuesday*/, "17"/*Wednesday*/, "26"/*Friday*/, "28"/*Sunday*/ });
            conditions.Add(ConditionKey.DayOfWeek, new[] { DayOfWeek.Monday.ToString(), DayOfWeek.Thursday.ToString(), DayOfWeek.Saturday.ToString(), DayOfWeek.Sunday.ToString() });
            conditions.Add(ConditionKey.Language, new[] { LocalizedContentManager.LanguageCode.en.ToString(), LocalizedContentManager.LanguageCode.pt.ToString() });
            conditions.Add(ConditionKey.Season, new[] { "Spring", "Fall" });
            conditions.Add(ConditionKey.Weather, new InvariantHashSet { Weather.Rain.ToString(), Weather.Sun.ToString() });

            // act
            IDictionary<ConditionKey, InvariantHashSet> possibleValues = factory.GetPossibleTokenisableValues(conditions);

            // assert
            this.SortAndCommaDelimit(possibleValues[ConditionKey.Day]).Should().Be("1, 28", $"should match for {ConditionType.Day}");
            this.SortAndCommaDelimit(possibleValues[ConditionKey.DayOfWeek]).Should().Be($"{DayOfWeek.Monday}, {DayOfWeek.Sunday}", $"should match for {ConditionType.DayOfWeek}");
            this.SortAndCommaDelimit(possibleValues[ConditionKey.Language]).Should().Be($"{LocalizedContentManager.LanguageCode.en}, {LocalizedContentManager.LanguageCode.pt}", $"should match for {ConditionType.Language}");
            this.SortAndCommaDelimit(possibleValues[ConditionKey.Season]).Should().Be("Fall, Spring", $"should match for {ConditionType.Season}");
            this.SortAndCommaDelimit(possibleValues[ConditionKey.Weather]).Should().Be($"{Weather.Rain}, {Weather.Sun}", $"should match for {ConditionType.Weather}");
        }

        /****
        ** GetPossibleStrings
        ****/
        /// <summary>Test that <see cref="ConditionFactory.GetPossibleStrings"/> returns the expected values given a single token with no conditins.</summary>
        [TestCase(ConditionType.Day)]
        [TestCase(ConditionType.DayOfWeek)]
        [TestCase(ConditionType.Language)]
        [TestCase(ConditionType.Season)]
        [TestCase(ConditionType.Weather)]
        public void GetPossibleStrings_WithOneToken(ConditionType conditionType)
        {
            // arrange
            ConditionKey conditionKey = new ConditionKey(conditionType);
            ConditionFactory factory = new ConditionFactory();
            TokenString tokenStr = new TokenString("{{" + conditionType + "}}", new HashSet<ConditionKey> { conditionKey }, TokenStringBuilder.TokenPattern);
            ConditionDictionary conditions = factory.BuildEmpty();

            // act
            IEnumerable<string> actual = factory.GetPossibleStrings(tokenStr, conditions);

            // assert
            this.SortAndCommaDelimit(actual).Should().Be(this.CommaDelimitedValues[conditionKey]);
        }

        /// <summary>Test that <see cref="ConditionFactory.GetPossibleStrings"/> returns the expected values given only implied conditions.</summary>
        [TestCase("{{season}}_{{weather}}", ConditionType.Season, ConditionType.Weather, ExpectedResult = "Fall_Rain, Fall_Snow, Fall_Storm, Fall_Sun, Spring_Rain, Spring_Snow, Spring_Storm, Spring_Sun, Summer_Rain, Summer_Snow, Summer_Storm, Summer_Sun, Winter_Rain, Winter_Snow, Winter_Storm, Winter_Sun")]
        public string GetPossibleStrings_WithMultipleTokens(string raw, params ConditionType[] conditionTypes)
        {
            // arrange
            ConditionFactory factory = new ConditionFactory();
            TokenString tokenStr = new TokenString(raw, new HashSet<ConditionKey>(conditionTypes.Select(type => new ConditionKey(type))), TokenStringBuilder.TokenPattern);
            ConditionDictionary conditions = factory.BuildEmpty();

            // act
            IEnumerable<string> actual = factory.GetPossibleStrings(tokenStr, conditions);

            // assert
            return this.SortAndCommaDelimit(actual);
        }

        /****
        ** CanConditionsOverlap
        ****/
        /// <summary>Test that <see cref="ConditionFactory.CanConditionsOverlap"/> returns the expected values given only implied conditions.</summary>
        [TestCase]
        public void CanConditionsOverlap_WithImpliedConditions()
        {
            // arrange
            ConditionFactory factory = new ConditionFactory();
            ConditionDictionary left = factory.BuildEmpty();
            ConditionDictionary right = factory.BuildEmpty();

            // act
            bool canOverlap = factory.CanConditionsOverlap(left, right);

            // assert
            canOverlap.Should().BeTrue();
        }

        /// <summary>Test that <see cref="ConditionFactory.CanConditionsOverlap"/> returns the expected values when the seasons overlap.</summary>
        [TestCase]
        public void CanConditionsOverlap_WhenSeasonsOverlap()
        {
            // arrange
            ConditionFactory factory = new ConditionFactory();
            ConditionDictionary left = factory.BuildEmpty();
            ConditionDictionary right = factory.BuildEmpty();
            left.Add(ConditionKey.Season, new[] { "Spring", "SUMMer" }); // should match case-insensitively
            right.Add(ConditionKey.Season, new[] { "Summer", "Fall" });

            // act
            bool canOverlap = factory.CanConditionsOverlap(left, right);

            // assert
            canOverlap.Should().BeTrue();
        }

        /// <summary>Test that <see cref="ConditionFactory.CanConditionsOverlap"/> returns the expected values when the seasons do not overlap.</summary>
        [TestCase]
        public void CanConditionsOverlap_WhenSeasonsDistinct()
        {
            // arrange
            ConditionFactory factory = new ConditionFactory();
            ConditionDictionary left = factory.BuildEmpty();
            ConditionDictionary right = factory.BuildEmpty();
            left.Add(ConditionKey.Season, new[] { "Spring", "Summer" });
            right.Add(ConditionKey.Season, new[] { "Fall", "Winter" });

            // act
            bool canOverlap = factory.CanConditionsOverlap(left, right);

            // assert
            canOverlap.Should().BeFalse();
        }

        /// <summary>Test that <see cref="ConditionFactory.CanConditionsOverlap"/> returns the expected values when the days from one overlap the days-of-week from the other.</summary>
        [TestCase]
        public void CanConditionsOverlap_WhenDaysAndDaysOfWeekOverlap()
        {
            // arrange
            ConditionFactory factory = new ConditionFactory();
            ConditionDictionary left = factory.BuildEmpty();
            ConditionDictionary right = factory.BuildEmpty();
            left.Add(ConditionKey.Day, new[] { "1", "2" });
            right.Add(ConditionKey.DayOfWeek, new[] { DayOfWeek.Tuesday.ToString(), DayOfWeek.Wednesday.ToString() });

            // act
            bool canOverlap = factory.CanConditionsOverlap(left, right);

            // assert
            canOverlap.Should().BeTrue();
        }

        /// <summary>Test that <see cref="ConditionFactory.CanConditionsOverlap"/> returns the expected values when the days from one are distinct from the days-of-week of the other.</summary>
        [TestCase]
        public void CanConditionsOverlap_WhenDaysAndDaysOfWeekDistinct()
        {
            // arrange
            ConditionFactory factory = new ConditionFactory();
            ConditionDictionary left = factory.BuildEmpty();
            ConditionDictionary right = factory.BuildEmpty();
            left.Add(ConditionKey.Day, new[] { "1", "2" });
            right.Add(ConditionKey.DayOfWeek, new[] { DayOfWeek.Wednesday.ToString(), DayOfWeek.Thursday.ToString() });

            // act
            bool canOverlap = factory.CanConditionsOverlap(left, right);

            // assert
            canOverlap.Should().BeFalse();
        }

        /// <summary>Test that <see cref="ConditionFactory.CanConditionsOverlap"/> returns the expected values when the weathers are distinct.</summary>
        [TestCase]
        public void CanConditionsOverlap_WhenWeathersDistinct()
        {
            // arrange
            ConditionFactory factory = new ConditionFactory();
            ConditionDictionary left = factory.BuildEmpty();
            ConditionDictionary right = factory.BuildEmpty();
            left.Add(ConditionKey.Weather, new[] { Weather.Rain.ToString(), Weather.Snow.ToString() });
            right.Add(ConditionKey.Weather, new[] { Weather.Storm.ToString(), Weather.Sun.ToString() });

            // act
            bool canOverlap = factory.CanConditionsOverlap(left, right);

            // assert
            canOverlap.Should().BeFalse();
        }

        /****
        ** GetApplicablePermutationsForTheseConditions
        ****/
        /// <summary>Test that <see cref="ConditionFactory.GetApplicablePermutationsForTheseConditions"/> returns the expected values given only implied conditions.</summary>
        [TestCase]
        public void GetApplicablePermutationsForTheseConditions()
        {
            // arrange
            ConditionFactory factory = new ConditionFactory();
            HashSet<ConditionKey> keys = new HashSet<ConditionKey> { ConditionKey.DayOfWeek, ConditionKey.Season };
            ConditionDictionary conditions = factory.BuildEmpty();

            // act
            IEnumerable<IDictionary<ConditionKey, string>> permutations = factory.GetApplicablePermutationsForTheseConditions(keys, conditions);

            // assert
            IEnumerable<string> actual = permutations.Select(permutation => "(" + this.SortAndCommaDelimit(permutation.Select(p => $"{p.Key}:{p.Value}")) + ")");
            List<string> expected = new List<string>();
            foreach (DayOfWeek dayOfWeek in this.DaysOfWeek)
            {
                foreach (string season in this.Seasons)
                    expected.Add($"({ConditionType.DayOfWeek}:{dayOfWeek}, {ConditionType.Season}:{season})");
            }

            this.SortAndCommaDelimit(actual).Should().Be(this.SortAndCommaDelimit(expected));
        }

        /****
        ** GetPermutations
        ****/
        /// <summary>Test that <see cref="ConditionFactory.GetPermutations"/> returns the expected values.</summary>
        [TestCase]
        public void GetPermutations()
        {
            // arrange
            InvariantDictionary<InvariantHashSet> values = new InvariantDictionary<InvariantHashSet>
            {
                ["a"] = new InvariantHashSet { "a1", "a2", "a3" },
                ["b"] = new InvariantHashSet { "b1", "b2", "b3" },
                ["c"] = new InvariantHashSet { "c1", "c2", "c3" }
            };

            // act
            IEnumerable<InvariantDictionary<string>> permutations = new ConditionFactory().GetPermutations(values);

            // assert
            IEnumerable<string> actual = permutations.Select(permutation => "(" + this.SortAndCommaDelimit(permutation.Values.OrderBy(p => p)) + ")");
            IList<string> expected = new List<string>();
            for (int a = 1; a <= 3; a++)
            {
                for (int b = 1; b <= 3; b++)
                {
                    for (int c = 1; c <= 3; c++)
                        expected.Add($"(a{a}, b{b}, c{c})");
                }
            }
            this.SortAndCommaDelimit(actual).Should().Be(this.SortAndCommaDelimit(expected));
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get a comma-delimited representation of a list.</summary>
        /// <typeparam name="T">The list item type.</typeparam>
        /// <param name="list">The list to represent.</param>
        private string CommaDelimit<T>(IEnumerable<T> list)
        {
            return list != null
                ? string.Join(", ", list.Select(p => p?.ToString()))
                : null;
        }

        /// <summary>Convert a comma-delimited representation of a list.</summary>
        /// <typeparam name="T">The list item type.</typeparam>
        /// <param name="list">The list to represent.</param>
        private string SortAndCommaDelimit<T>(IEnumerable<T> list)
        {
            return list != null
                ? string.Join(", ", list.Select(p => p?.ToString()).OrderBy(p => p))
                : null;
        }
    }
}
