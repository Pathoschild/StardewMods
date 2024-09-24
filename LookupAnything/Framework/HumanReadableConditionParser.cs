using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework
{
    /// <summary>Parses game state queries into human-readable representations where possible.</summary>
    internal static class HumanReadableConditionParser
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get a human-readable representation of a game state query.</summary>
        /// <param name="condition">The raw game state query to parse.</param>
        public static string Format(string condition)
        {
            return
                HumanReadableConditionParser.Format(condition, null)
                ?? I18n.Condition_RawCondition(condition);
        }

        /// <summary>Get a human-readable representation of a game state query.</summary>
        /// <param name="condition">The raw game state query to parse.</param>
        /// <param name="defaultValue">The value to return if there's no human-readable representation available.</param>
        [return: NotNullIfNotNull(nameof(defaultValue))]
        public static string? Format(string condition, string? defaultValue)
        {
            // parse query
            // (If we get unexpected values at this point, bail early.)
            GameStateQuery.ParsedGameStateQuery query;
            {
                GameStateQuery.ParsedGameStateQuery[]? queries = GameStateQuery.Parse(condition);
                if (queries.Length != 1)
                    return defaultValue;

                query = queries[0];
                if (query.Error != null || query.Query.Length == 0 || string.IsNullOrWhiteSpace(query.Query[0]))
                    return defaultValue;
            }

            // apply parser
            string? parsed = null;
            switch (query.Query[0].ToUpperInvariant().Trim())
            {
                case nameof(GameStateQuery.DefaultResolvers.DAY_OF_MONTH):
                    parsed = HumanReadableConditionParser.ParseDayOfMonth(query.Query);
                    break;

                case nameof(GameStateQuery.DefaultResolvers.ITEM_CONTEXT_TAG):
                    parsed = HumanReadableConditionParser.ParseItemContextTag(query.Query);
                    break;

                case nameof(GameStateQuery.DefaultResolvers.ITEM_EDIBILITY):
                    parsed = HumanReadableConditionParser.ParseItemEdibility(query.Query);
                    break;
            }

            // format value
            if (parsed != null)
            {
                return query.Negated
                    ? I18n.ConditionOrContextTag_Negate(value: parsed)
                    : parsed;
            }

            return defaultValue;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Parse a <see cref="GameStateQuery.DefaultResolvers.DAY_OF_MONTH"/> query into a human-readable representation, if possible.</summary>
        /// <param name="query">The raw query arguments.</param>
        private static string ParseDayOfMonth(string[] query)
        {
            string[] days = [..  query.Skip(1)];

            for (int i = 0; i < days.Length; i++)
            {
                string day = days[i];

                if (string.Equals(day, "even", StringComparison.OrdinalIgnoreCase))
                    days[i] = I18n.Condition_DayOfMonth_Even();
                else if (string.Equals(day, "odd", StringComparison.OrdinalIgnoreCase))
                    days[i] = I18n.Condition_DayOfMonth_Odd();
            }

            return I18n.Condition_DayOfMonth(days: I18n.List(days));
        }

        /// <summary>Parse a <see cref="GameStateQuery.DefaultResolvers.ITEM_CONTEXT_TAG"/> query into a human-readable representation, if possible.</summary>
        /// <param name="query">The raw query arguments.</param>
        private static string? ParseItemContextTag(string[] query)
        {
            // format item type
            if (!HumanReadableConditionParser.TryTranslateItemTarget(ArgUtility.Get(query, 1), out string? itemType))
                return null;

            // format context tags
            string contextTags;
            {
                string[] rawTags = ArgUtility.GetSubsetOf(query, 2);
                for (int i = 0; i < rawTags.Length; i++)
                    rawTags[i] = I18n.Condition_ItemContextTag_Value(tag: rawTags[i]);
                contextTags = I18n.List(rawTags);
            }

            // build translation
            return query.Length > 3
                ? I18n.Condition_ItemContextTags(item: itemType, tags: contextTags)
                : I18n.Condition_ItemContextTag(item: itemType, tags: contextTags);
        }

        /// <summary>Parse a <see cref="GameStateQuery.DefaultResolvers.ITEM_EDIBILITY"/> query into a human-readable representation, if possible.</summary>
        /// <param name="query">The raw query arguments.</param>
        private static string? ParseItemEdibility(string[] query)
        {
            // format item type
            if (!HumanReadableConditionParser.TryTranslateItemTarget(ArgUtility.Get(query, 1), out string? itemType))
                return null;

            // format edibility
            if (!ArgUtility.HasIndex(query, 2))
                return I18n.Condition_ItemEdibility_Edible(item: itemType);

            int min = ArgUtility.GetInt(query, 2);
            int max = ArgUtility.GetInt(query, 3, int.MaxValue);
            return max != int.MaxValue
                ? I18n.Condition_ItemEdibility_Range(item: itemType, min: min, max: max)
                : I18n.Condition_ItemEdibility_Min(item: itemType, min: min);
        }

        /// <summary>Get the translated representation for an item target like 'input' or 'target', if known.</summary>
        /// <param name="itemType">The raw item type.</param>
        /// <param name="translated">The translated representation.</param>
        /// <returns>Returns whether a translation was found for the target type.</returns>
        private static bool TryTranslateItemTarget(string? itemType, [NotNullWhen(true)] out string? translated)
        {
            if (string.Equals(itemType, "Input", StringComparison.OrdinalIgnoreCase))
            {
                translated = I18n.Condition_ItemType_Input();
                return true;
            }

            if (string.Equals(itemType, "Target", StringComparison.OrdinalIgnoreCase))
            {
                translated = I18n.Condition_ItemType_Target();
                return true;
            }

            translated = null;
            return false;
        }
    }
}
