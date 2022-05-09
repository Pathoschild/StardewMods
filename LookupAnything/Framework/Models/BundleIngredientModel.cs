using Pathoschild.Stardew.LookupAnything.Framework.Constants;

namespace Pathoschild.Stardew.LookupAnything.Framework.Models
{
    /// <summary>An item slot for a bundle.</summary>
    /// <param name="Index">The ingredient's index in the bundle.</param>
    /// <param name="ItemID">The required item's parent sprite index (or -1 for a monetary bundle).</param>
    /// <param name="Stack">The number of items required.</param>
    /// <param name="Quality">The required item quality.</param>
    internal record BundleIngredientModel(int Index, int ItemID, int Stack, ItemQuality Quality);
}
