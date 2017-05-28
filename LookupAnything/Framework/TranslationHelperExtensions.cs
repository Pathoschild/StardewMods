using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using StardewModdingAPI;

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
