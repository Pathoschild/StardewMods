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
        public IFormattedText[] Value { get; protected set; }

        /// <summary>Whether the field should be displayed.</summary>
        public bool HasValue { get; protected set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="label">A short field label.</param>
        /// <param name="value">The field value.</param>
        /// <param name="hasValue">Whether the field should be displayed (or <c>null</c> to check the <paramref name="value"/>).</param>
        public GenericField(string label, string value, bool? hasValue = null)
        {
            this.Label = label;
            this.Value = this.FormatValue(value);
            this.HasValue = hasValue ?? this.Value?.Any() == true;
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="label">A short field label.</param>
        /// <param name="value">The field value.</param>
        /// <param name="hasValue">Whether the field should be displayed (or <c>null</c> to check the <paramref name="value"/>).</param>
        public GenericField(string label, IFormattedText value, bool? hasValue = null)
            : this(label, new[] { value }, hasValue) { }

        /// <summary>Construct an instance.</summary>
        /// <param name="label">A short field label.</param>
        /// <param name="value">The field value.</param>
        /// <param name="hasValue">Whether the field should be displayed (or <c>null</c> to check the <paramref name="value"/>).</param>
        public GenericField(string label, IEnumerable<IFormattedText> value, bool? hasValue = null)
        {
            this.Label = label;
            this.Value = value.ToArray();
            this.HasValue = hasValue ?? this.Value?.Any() == true;
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
        /// <summary>Construct an instance.</summary>
        /// <param name="label">A short field label.</param>
        /// <param name="hasValue">Whether the field should be displayed.</param>
        protected GenericField(string label, bool hasValue = false)
            : this(label, null as string, hasValue) { }

        /// <summary>Wrap text into a list of formatted snippets.</summary>
        /// <param name="value">The text to wrap.</param>
        protected IFormattedText[] FormatValue(string value)
        {
            return !string.IsNullOrWhiteSpace(value)
                ? new IFormattedText[] { new FormattedText(value) }
                : new IFormattedText[0];
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
                string result = I18n.Generic_Price(price: saleValues.First().Value);
                if (stackSize > 1 && stackSize <= Constant.MaxStackSizeForPricing)
                    result += $" ({I18n.Generic_PriceForStack(price: saleValues.First().Value * stackSize, count: stackSize)})";
                return result;
            }

            // prices by quality
            List<string> priceStrings = new List<string>();
            for (ItemQuality quality = ItemQuality.Normal; ; quality = quality.GetNext())
            {
                if (saleValues.ContainsKey(quality))
                {
                    priceStrings.Add(quality == ItemQuality.Normal
                        ? I18n.Generic_Price(price: saleValues[quality])
                        : I18n.Generic_PriceForQuality(price: saleValues[quality], quality: I18n.For(quality))
                    );
                }

                if (quality.GetNext() == quality)
                    break;
            }
            return string.Join(", ", priceStrings);
        }
    }
}
