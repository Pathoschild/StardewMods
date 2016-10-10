using System.Collections.Generic;
using System.Linq;
using Pathoschild.LookupAnything.Framework.Constants;

namespace Pathoschild.LookupAnything.Framework.Fields
{
    /// <summary>A metadata field which indicates an item's price based on its quality.</summary>
    internal class SaleValueField : GenericField
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="label">A short field label.</param>
        /// <param name="saleValues">The sale values by quality.</param>
        /// <param name="stackSize">The number of items in the stack.</param>
        public SaleValueField(string label, IDictionary<ItemQuality, int> saleValues, int stackSize)
            : base(label, SaleValueField.GetSummary(saleValues, stackSize)) { }

        /// <summary>Construct an instance.</summary>
        /// <param name="label">A short field label.</param>
        /// <param name="saleValue">The flat sale value.</param>
        /// <param name="stackSize">The number of items in the stack.</param>
        public SaleValueField(string label, int saleValue, int stackSize)
            : base(label, SaleValueField.GetSummary(new Dictionary<ItemQuality, int> { [ItemQuality.Normal] = saleValue }, stackSize)) { }

        /// <summary>Get the display value for sale price data.</summary>
        /// <param name="saleValues">The sale price data.</param>
        /// <param name="stackSize">The number of items in the stack.</param>
        public static string GetSummary(IDictionary<ItemQuality, int> saleValues, int stackSize)
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
            return $"{saleValues[ItemQuality.Normal]}g, {saleValues[ItemQuality.Silver]}g (silver), {saleValues[ItemQuality.Gold]}g (gold), {saleValues[ItemQuality.Iridium]}g (iridium)";
        }
    }
}