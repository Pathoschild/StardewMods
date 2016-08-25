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
        public SaleValueField(string label, IDictionary<ItemQuality, int> saleValues)
            : base(label, SaleValueField.GetValue(saleValues)) { }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the display value for sale price data.</summary>
        /// <param name="saleValues">The sale price data.</param>
        private static string GetValue(IDictionary<ItemQuality, int> saleValues)
        {
            // can't be sold
            if (saleValues == null || !saleValues.Any() || saleValues.Values.All(p => p == 0))
                return null;

            // else show price by quality
            return saleValues.Count == 1
                ? $"{saleValues.First().Value}g"
                : $"{saleValues[ItemQuality.Low]}g (low quality), {saleValues[ItemQuality.Medium]}g (medium), {saleValues[ItemQuality.High]}g (high)";
        }
    }
}