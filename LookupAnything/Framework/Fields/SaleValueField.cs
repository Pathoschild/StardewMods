using System.Collections.Generic;
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
            : base(label, $"{saleValues[ItemQuality.Low]}g (low quality), {saleValues[ItemQuality.Medium]}g (medium), {saleValues[ItemQuality.High]}g (high)") { }
    }
}