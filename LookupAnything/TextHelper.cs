using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using StardewModdingAPI;

namespace Pathoschild.Stardew.LookupAnything
{
    /// <summary>Provides methods for fetching translations and generating text.</summary>
    internal class TextHelper
    {
        /*********
        ** Properties
        *********/
        /// <summary>Provides translations stored in the mod's <c>i18n</c> folder.</summary>
        private readonly ITranslationHelper Translation;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="translation">Provides translations stored in the mod's <c>i18n</c> folder.</param>
        public TextHelper(ITranslationHelper translation)
        {
            this.Translation = translation;
        }

        /// <summary>Get a translation for the current locale.</summary>
        /// <param name="key">The translation key.</param>
        /// <param name="tokens">An anonymous object containing token key/value pairs, like <c>new { value = 42, name = "Cranberries" }</c>.</param>
        /// <exception cref="KeyNotFoundException">The <paramref name="key" /> doesn't match an available translation.</exception>
        public Translation Translate(string key, object tokens = null)
        {
            Translation translation = this.Translation.Translate(key);
            return tokens != null
                ? translation.Tokens(tokens)
                : translation;
        }

        /// <summary>Select the correct plural form for a word.</summary>
        /// <param name="count">The number.</param>
        /// <param name="single">The singular form.</param>
        /// <param name="plural">The plural form.</param>
        public string Pluralise(int count, string single, string plural = null)
        {
            return count == 1 ? single : (plural ?? single + "s");
        }

        /// <summary>Get a human-readable representation of a value.</summary>
        /// <param name="value">The underlying value.</param>
        public string Stringify(object value)
        {
            switch (value)
            {
                case null:
                    return null;

                // boolean
                case bool boolean:
                    return this.Translate(boolean ? L10n.Generic.Yes : L10n.Generic.No);

                // time span
                case TimeSpan span:
                    {
                        List<string> parts = new List<string>();
                        if (span.Days > 0)
                            parts.Add(this.Translate(L10n.Generic.Days, new { count = span.Days }));
                        if (span.Hours > 0)
                            parts.Add(this.Translate(L10n.Generic.Hours, new { count = span.Hours }));
                        if (span.Minutes > 0)
                            parts.Add(this.Translate(L10n.Generic.Minutes, new { count = span.Minutes }));
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
                        string[] values = (from val in array.Cast<object>() select this.Stringify(val)).ToArray();
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
                                string k = this.Stringify(type.GetProperty(nameof(KeyValuePair<byte, byte>.Key)).GetValue(value));
                                string v = this.Stringify(type.GetProperty(nameof(KeyValuePair<byte, byte>.Value)).GetValue(value));
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
