using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework
{
    /// <summary>Provides extension methods for <see cref="ITranslationHelper"/>.</summary>
    internal static class TranslationHelperExtensions
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Select the correct translation based on the plural form.</summary>
        /// <param name="translations">The translation helper.</param>
        /// <param name="count">The number.</param>
        /// <param name="singleKey">The singular form.</param>
        /// <param name="pluralKey">The plural form.</param>
        public static Translation GetPlural(this ITranslationHelper translations, int count, string singleKey, string pluralKey)
        {
            return translations.Get(count == 1 ? singleKey : pluralKey);
        }

        /// <summary>Get a translated season name from the game.</summary>
        /// <param name="translations">The translation helper.</param>
        /// <param name="season">The English season name.</param>
        public static string GetSeasonName(this ITranslationHelper translations, string season)
        {
            if (string.IsNullOrWhiteSpace(season))
                return season;

            int id = Utility.getSeasonNumber(season);
            if (id == -1)
                throw new InvalidOperationException($"Can't translate unknown season '{season}'.");
            return Utility.getSeasonNameFromNumber(id);
        }

        /// <summary>Get translated season names from the game.</summary>
        /// <param name="translations">The translation helper.</param>
        /// <param name="seasons">The English season names.</param>
        public static IEnumerable<string> GetSeasonNames(this ITranslationHelper translations, IEnumerable<string> seasons)
        {
            foreach (string season in seasons)
                yield return translations.GetSeasonName(season);
        }

        /// <summary>Get a human-readable representation of a value.</summary>
        /// <param name="translations">The translation helper.</param>
        /// <param name="date">The game date.</param>
        /// <param name="withYear">Whether to include the year number.</param>
        public static string Stringify(this ITranslationHelper translations, SDate date, bool withYear)
        {
            string key = withYear ? L10n.Generic.DateWithYear : L10n.Generic.Date;
            return translations.Get(key, new
            {
                seasonNumber = Utility.getSeasonNumber(date.Season),
                seasonName = Utility.getSeasonNameFromNumber(Utility.getSeasonNumber(date.Season)),
                dayNumber = date.Day,
                year = date.Year
            });
        }

        /// <summary>Get a human-readable representation of a value.</summary>
        /// <param name="translations">The translation helper.</param>
        /// <param name="value">The underlying value.</param>
        public static string Stringify(this ITranslationHelper translations, object value)
        {
            switch (value)
            {
                case null:
                    return null;

                // boolean
                case bool boolean:
                    return translations.Get(boolean ? L10n.Generic.Yes : L10n.Generic.No);

                // game date
                case SDate date:
                    return translations.Stringify(date, withYear: false);

                // time span
                case TimeSpan span:
                    {
                        List<string> parts = new List<string>();
                        if (span.Days > 0)
                            parts.Add(translations.Get(L10n.Generic.Days, new { count = span.Days }));
                        if (span.Hours > 0)
                            parts.Add(translations.Get(L10n.Generic.Hours, new { count = span.Hours }));
                        if (span.Minutes > 0)
                            parts.Add(translations.Get(L10n.Generic.Minutes, new { count = span.Minutes }));
                        return string.Join(", ", parts);
                    }

                // vector
                case Vector2 vector:
                    return $"({vector.X}, {vector.Y})";

                // rectangle
                case Rectangle rect:
                    return $"(x:{rect.X}, y:{rect.Y}, width:{rect.Width}, height:{rect.Height})";

                // array
                case IEnumerable array when !(value is string):
                    {
                        string[] values = (from val in array.Cast<object>() select translations.Stringify(val)).ToArray();
                        return "(" + string.Join(", ", values) + ")";
                    }

                // color
                case Color color:
                    return $"(r:{color.R} g:{color.G} b:{color.B} a:{color.A})";

                default:
                    // key/value pair
                    {
                        Type type = value.GetType();
                        if (type.IsGenericType)
                        {
                            Type genericType = type.GetGenericTypeDefinition();
                            if (genericType == typeof(KeyValuePair<,>))
                            {
                                string k = translations.Stringify(type.GetProperty(nameof(KeyValuePair<byte, byte>.Key)).GetValue(value));
                                string v = translations.Stringify(type.GetProperty(nameof(KeyValuePair<byte, byte>.Value)).GetValue(value));
                                return $"({k}: {v})";
                            }
                        }
                    }

                    // anything else
                    return value.ToString();
            }
        }
    }
}
