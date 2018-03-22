using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework;
using ContentPatcher.Framework.Conditions;
using NUnit.Framework;
using Pathoschild.Stardew.Common.Utilities;
using StardewValley;

namespace Pathoschild.Stardew.Tests.Mods.ContentPatcher
{
    /// <summary>Unit tests for <see cref="ConditionFactory"/>.</summary>
    [TestFixture]
    class ConditionFactoryTests
    {
        /*********
        ** Properties
        *********/
        /// <summary>All possible days as a sorted and comma-delimited string.</summary>
        private const string CommaDelimitedDays = "1, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 2, 20, 21, 22, 23, 24, 25, 26, 27, 28, 3, 4, 5, 6, 7, 8, 9";

        /// <summary>All possible days of week as a sorted and comma-delimited string.</summary>
        private const string CommaDelimitedDaysOfWeek = "Friday, Monday, Saturday, Sunday, Thursday, Tuesday, Wednesday";

        /// <summary>All possible language codes as a sorted and comma-delimited string.</summary>
        private const string CommaDelimitedLanguages = "de, en, es, ja, pt, ru, zh";

        /// <summary>All possible seasons as a sorted and comma-delimited string.</summary>
        private const string CommaDelimitedSeasons = "Fall, Spring, Summer, Winter";

        /// <summary>All possible weathers as a sorted and comma-delimited string.</summary>
        private const string CommaDelimitedWeathers = "Rain, Snow, Storm, Sun";

        /// <summary>The valid days of week.</summary>
        private readonly IEnumerable<DayOfWeek> DaysOfWeek = (from DayOfWeek value in Enum.GetValues(typeof(DayOfWeek)) select value).ToArray();

        /// <summary>The valid season names.</summary>
        private readonly IEnumerable<string> Seasons = new[] { "Spring", "Summer", "Fall", "Winter" };


        /*********
        ** Unit tests
        *********/
        /****
        ** GetValidConditions
        ****/
        /// <summary>Test that <see cref="ConditionFactory.GetValidConditions"/> returns all condition keys.</summary>
        [TestCase]
        public void GetValidConditions()
        {
            // act
            IEnumerable<ConditionKey> conditions = new ConditionFactory().GetValidConditions();

            // assert
            Assert.AreEqual(
                this.SortAndCommaDelimit(Enum.GetValues(typeof(ConditionKey)).Cast<ConditionKey>()),
                this.SortAndCommaDelimit(conditions)
            );
        }

        /****
        ** GetValidValues
        ****/
        /// <summary>Test that <see cref="ConditionFactory.GetValidValues"/> returns the expected values.</summary>
        [TestCase(ConditionKey.Day, ExpectedResult = ConditionFactoryTests.CommaDelimitedDays)]
        [TestCase(ConditionKey.DayOfWeek, ExpectedResult = ConditionFactoryTests.CommaDelimitedDaysOfWeek)]
        [TestCase(ConditionKey.Language, ExpectedResult = ConditionFactoryTests.CommaDelimitedLanguages)]
        [TestCase(ConditionKey.Season, ExpectedResult = ConditionFactoryTests.CommaDelimitedSeasons)]
        [TestCase(ConditionKey.Weather, ExpectedResult = ConditionFactoryTests.CommaDelimitedWeathers)]
        public string GetValidValues(ConditionKey condition)
        {
            // act
            IEnumerable<string> values = new ConditionFactory().GetValidValues(condition);

            // assert
            return this.SortAndCommaDelimit(values);
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
                Assert.AreEqual(expectedDayOfWeek, actual, $"{nameof(ConditionFactory.GetDayOfWeekFor)} returned {actual} for day {day}, expected {expectedDayOfWeek}.");
            }
        }

        /****
        ** GetPossibleValues
        ****/
        /// <summary>Test that <see cref="ConditionFactory.GetPossibleValues"/> returns the expected values given only implied conditions.</summary>
        [TestCase]
        public void GetPossibleValues_WithOnlyImpliedConditions()
        {
            // arrange
            ConditionFactory factory = new ConditionFactory();
            ConditionDictionary conditions = factory.BuildEmpty();

            // act
            IDictionary<ConditionKey, InvariantHashSet> possibleValues = factory.GetPossibleValues(conditions);

            // assert
            Assert.AreEqual(ConditionFactoryTests.CommaDelimitedDays, this.SortAndCommaDelimit(possibleValues[ConditionKey.Day]), $"Got unexpected results for {ConditionKey.Day}.");
            Assert.AreEqual(ConditionFactoryTests.CommaDelimitedDaysOfWeek, this.SortAndCommaDelimit(possibleValues[ConditionKey.DayOfWeek]), $"Got unexpected results for {ConditionKey.DayOfWeek}.");
            Assert.AreEqual(ConditionFactoryTests.CommaDelimitedLanguages, this.SortAndCommaDelimit(possibleValues[ConditionKey.Language]), $"Got unexpected results for {ConditionKey.Language}.");
            Assert.AreEqual(ConditionFactoryTests.CommaDelimitedSeasons, this.SortAndCommaDelimit(possibleValues[ConditionKey.Season]), $"Got unexpected results for {ConditionKey.Season}.");
            Assert.AreEqual(ConditionFactoryTests.CommaDelimitedWeathers, this.SortAndCommaDelimit(possibleValues[ConditionKey.Weather]), $"Got unexpected results for {ConditionKey.Weather}.");
        }

        /// <summary>Test that <see cref="ConditionFactory.GetPossibleValues"/> returns the expected values given only implied conditions and restricted days of week.</summary>
        [TestCase]
        public void GetPossibleValues_WithImpliedConditionsAndRestrictedDaysOfWeek()
        {
            // arrange
            ConditionFactory factory = new ConditionFactory();
            ConditionDictionary conditions = factory.BuildEmpty();
            conditions.Add(ConditionKey.DayOfWeek, new[] { DayOfWeek.Tuesday.ToString(), DayOfWeek.Wednesday.ToString() });

            // act
            IDictionary<ConditionKey, InvariantHashSet> possibleValues = factory.GetPossibleValues(conditions);

            // assert
            Assert.AreEqual("10, 16, 17, 2, 23, 24, 3, 9", this.SortAndCommaDelimit(possibleValues[ConditionKey.Day]), $"Got unexpected results for {ConditionKey.Day}.");
            Assert.AreEqual($"{DayOfWeek.Tuesday}, {DayOfWeek.Wednesday}", this.SortAndCommaDelimit(possibleValues[ConditionKey.DayOfWeek]), $"Got unexpected results for {ConditionKey.DayOfWeek}.");
            Assert.AreEqual(ConditionFactoryTests.CommaDelimitedLanguages, this.SortAndCommaDelimit(possibleValues[ConditionKey.Language]), $"Got unexpected results for {ConditionKey.Language}.");
            Assert.AreEqual(ConditionFactoryTests.CommaDelimitedSeasons, this.SortAndCommaDelimit(possibleValues[ConditionKey.Season]), $"Got unexpected results for {ConditionKey.Season}.");
            Assert.AreEqual(ConditionFactoryTests.CommaDelimitedWeathers, this.SortAndCommaDelimit(possibleValues[ConditionKey.Weather]), $"Got unexpected results for {ConditionKey.Weather}.");
        }

        /// <summary>Test that <see cref="ConditionFactory.GetPossibleValues"/> returns the expected values given a subset of each condition's possible values.</summary>
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
            IDictionary<ConditionKey, InvariantHashSet> possibleValues = factory.GetPossibleValues(conditions);

            // assert
            Assert.AreEqual("1, 28", this.SortAndCommaDelimit(possibleValues[ConditionKey.Day]), $"Got unexpected results for {ConditionKey.Day}.");
            Assert.AreEqual($"{DayOfWeek.Monday}, {DayOfWeek.Sunday}", this.SortAndCommaDelimit(possibleValues[ConditionKey.DayOfWeek]), $"Got unexpected results for {ConditionKey.DayOfWeek}.");
            Assert.AreEqual($"{LocalizedContentManager.LanguageCode.en}, {LocalizedContentManager.LanguageCode.pt}", this.SortAndCommaDelimit(possibleValues[ConditionKey.Language]), $"Got unexpected results for {ConditionKey.Language}.");
            Assert.AreEqual("Fall, Spring", this.SortAndCommaDelimit(possibleValues[ConditionKey.Season]), $"Got unexpected results for {ConditionKey.Season}.");
            Assert.AreEqual($"{Weather.Rain}, {Weather.Sun}", this.SortAndCommaDelimit(possibleValues[ConditionKey.Weather]), $"Got unexpected results for {ConditionKey.Weather}.");
        }

        /****
        ** GetPossibleStrings
        ****/
        /// <summary>Test that <see cref="ConditionFactory.GetPossibleStrings"/> returns the expected values given only implied conditions.</summary>
        [TestCase("{{dayOfWeek}}", ConditionKey.DayOfWeek, ExpectedResult = ConditionFactoryTests.CommaDelimitedDaysOfWeek)]
        [TestCase("{{day}}", ConditionKey.Day, ExpectedResult = ConditionFactoryTests.CommaDelimitedDays)]
        [TestCase("{{language}}", ConditionKey.Language, ExpectedResult = ConditionFactoryTests.CommaDelimitedLanguages)]
        [TestCase("{{season}}", ConditionKey.Season, ExpectedResult = ConditionFactoryTests.CommaDelimitedSeasons)]
        [TestCase("{{weather}}", ConditionKey.Weather, ExpectedResult = ConditionFactoryTests.CommaDelimitedWeathers)]
        [TestCase("{{season}}_{{weather}}", ConditionKey.Season, ConditionKey.Weather, ExpectedResult = "Fall_Rain, Fall_Snow, Fall_Storm, Fall_Sun, Spring_Rain, Spring_Snow, Spring_Storm, Spring_Sun, Summer_Rain, Summer_Snow, Summer_Storm, Summer_Sun, Winter_Rain, Winter_Snow, Winter_Storm, Winter_Sun")]
        public string GetPossibleStrings_WithOnlyImpliedConditions(string raw, params ConditionKey[] conditionKeys)
        {
            // arrange
            ConditionFactory factory = new ConditionFactory();
            TokenString tokenStr = new TokenString(raw, new HashSet<ConditionKey>(conditionKeys), TokenStringBuilder.TokenPattern);
            ConditionDictionary conditions = factory.BuildEmpty();

            // act
            IEnumerable<string> actual = factory.GetPossibleStrings(tokenStr, conditions);

            // assert
            return this.SortAndCommaDelimit(actual);
        }

        /****
        ** GetApplicablePermutationsForTheseConditions
        ****/
        /// <summary>Test that <see cref="ConditionFactory.GetApplicablePermutationsForTheseConditions"/> returns the expected values given only implied conditions.</summary>
        [TestCase]
        public void GetApplicablePermutationsForTheseConditions_WithOnlyImpliedConditions()
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
                    expected.Add($"({ConditionKey.DayOfWeek}:{dayOfWeek}, {ConditionKey.Season}:{season})");
            }

            Assert.AreEqual(
                this.SortAndCommaDelimit(expected),
                this.SortAndCommaDelimit(actual)
            );
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
            Assert.AreEqual(
                this.SortAndCommaDelimit(expected),
                this.SortAndCommaDelimit(actual)
            );
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
