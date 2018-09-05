using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework;
using ContentPatcher.Framework.Conditions;
using FluentAssertions;
using NUnit.Framework;
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

        /*********
        ** Private methods
        *********/
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
