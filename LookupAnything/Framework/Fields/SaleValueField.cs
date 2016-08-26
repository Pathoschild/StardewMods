using System.Collections.Generic;
using System.Linq;
using Pathoschild.LookupAnything.Framework.Constants;

namespace Pathoschild.LookupAnything.Framework.Fields
{
    /// <summary>A metadata field which indicates an item's price based on its quality.</summary>
    public class SaleValueField : GenericField
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="label">A short field label.</param>
        /// <param name="saleValues">The sale values by quality.</param>
        /// <param name="stackSize">The number of items in the stack.</param>
        public SaleValueField(string label, IDictionary<ItemQuality, int> saleValues, int stackSize)
            : base(label, SaleValueField.GetValue(saleValues, stackSize)) { }

        /// <summary>Construct an instance.</summary>
        /// <param name="label">A short field label.</param>
        /// <param name="saleValue">The flat sale value.</param>
        /// <param name="stackSize">The number of items in the stack.</param>
        public SaleValueField(string label, int saleValue, int stackSize)
            : base(label, SaleValueField.GetValue(new Dictionary<ItemQuality, int> { [ItemQuality.Low] = saleValue }, stackSize)) { }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the display value for sale price data.</summary>
        /// <param name="saleValues">The sale price data.</param>
        /// <param name="stackSize">The number of items in the stack.</param>
        private static string GetValue(IDictionary<ItemQuality, int> saleValues, int stackSize)
        {
            // can't be sold
            if (saleValues == null || !saleValues.Any() || saleValues.Values.All(p => p == 0))
                return null;

            // one quality
            if (saleValues.Count == 1)
            {
                string result = $"{saleValues.First().Value}g";
                if (stackSize > 1)
                    result += $" (stack: {saleValues.First().Value * stackSize}g)";
                return result;
            }

            // prices by quality
            return $"{saleValues[ItemQuality.Low]}g (low quality), {saleValues[ItemQuality.Medium]}g (medium), {saleValues[ItemQuality.High]}g (high)";
        }
    }
}