using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pathoschild.LookupAnything.Framework.Fields
{
    /// <summary>A generic metadata field shown as an extended property in the lookup UI.</summary>
    internal class GenericField : ICustomField
    {
        /*********
        ** Accessors
        *********/
        /// <summary>A short field label.</summary>
        public string Label { get; protected set; }

        /// <summary>The field value.</summary>
        public string Value { get; protected set; }

        /// <summary>Whether the field should be displayed.</summary>
        public bool HasValue { get; protected set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="label">A short field label.</param>
        /// <param name="value">The field value.</param>
        /// <param name="hasValue">Whether the field should be displayed (or <c>null</c> to check the <paramref name="value"/>).</param>
        public GenericField(string label, object value, bool? hasValue = null)
        {
            this.Label = label;
            this.Value = GenericField.GetString(value);
            this.HasValue = hasValue ?? !string.IsNullOrWhiteSpace(this.Value);
        }

        /// <summary>Draw the value (or return <c>null</c> to render the <see cref="Value"/> using the default format).</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        /// <param name="font">The recommended font.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="wrapWidth">The maximum width before which content should be wrapped.</param>
        /// <returns>Returns the drawn dimensions, or <c>null</c> to draw the <see cref="Value"/> using the default format.</returns>
        public virtual Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
        {
            return null;
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Get a human-readable representation of a value.</summary>
        /// <param name="value">The underlying value.</param>
        public static string GetString(object value)
        {
            // boolean
            if (value is bool)
                return (bool) value ? "yes" : "no";

            // time span
            if (value is TimeSpan)
            {
                TimeSpan span = (TimeSpan)value;
                List<string> parts = new List<string>();
                if (span.Days > 0)
                    parts.Add($"{span.Days} {GameHelper.Pluralise(span.Days, "day")}");
                if (span.Hours > 0)
                    parts.Add($"{span.Hours} {GameHelper.Pluralise(span.Hours, "hour")}");
                if (span.Minutes > 0)
                    parts.Add($"{span.Minutes} {GameHelper.Pluralise(span.Minutes, "minute")}");
                return string.Join(", ", parts);
            }

            // else
            return value?.ToString();
        }
    }
}