using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields
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
                return (bool)value ? "yes" : "no";

            // time span
            if (value is TimeSpan)
            {
                TimeSpan span = (TimeSpan)value;
                List<string> parts = new List<string>();
                if (span.Days > 0)
                    parts.Add($"{span.Days} {GrammarHelper.Pluralise(span.Days, "day")}");
                if (span.Hours > 0)
                    parts.Add($"{span.Hours} {GrammarHelper.Pluralise(span.Hours, "hour")}");
                if (span.Minutes > 0)
                    parts.Add($"{span.Minutes} {GrammarHelper.Pluralise(span.Minutes, "minute")}");
                return string.Join(", ", parts);
            }

            // else
            return value?.ToString();
        }

        /// <summary>Get the display value for sale price data.</summary>
        /// <param name="saleValue">The flat sale price.</param>
        /// <param name="stackSize">The number of items in the stack.</param>
        public static string GetSaleValueString(int saleValue, int stackSize)
        {
            return GenericField.GetSaleValueString(new Dictionary<ItemQuality, int> { [ItemQuality.Normal] = saleValue }, stackSize);
        }

        /// <summary>Get the display value for sale price data.</summary>
        /// <param name="saleValues">The sale price data.</param>
        /// <param name="stackSize">The number of items in the stack.</param>
        public static string GetSaleValueString(IDictionary<ItemQuality, int> saleValues, int stackSize)
        {
            // can't be sold
            if (saleValues == null || !saleValues.Any() || saleValues.Values.All(p => p == 0))
                return null;

            // one quality
            if (saleValues.Count == 1)
            {
                string result = $"{saleValues.First().Value}g";
                if (stackSize > 1)
                    result += $" ({saleValues.First().Value * stackSize}g for stack of {stackSize})";
                return result;
            }

            // prices by quality
            List<string> priceStrings = new List<string>();
            for (ItemQuality quality = ItemQuality.Normal; ; quality = quality.GetNext())
            {
                if (saleValues.ContainsKey(quality))
                    priceStrings.Add($"{saleValues[quality]}g" + (quality != ItemQuality.Normal ? $" ({quality.GetName()})" : ""));

                if (quality.GetNext() == quality)
                    break;
            }
            return string.Join(", ", priceStrings);
        }
    }
}