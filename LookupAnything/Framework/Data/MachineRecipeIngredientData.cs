using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Data
{
    /// <summary>Metadata for an ingredient in a machine recipe.</summary>
    internal class MachineRecipeIngredientData
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The unique item IDs.</summary>
        public int[] PossibleIds { get; set; }

        /// <summary>The number required (or <c>null</c> for the default).</summary>
        public int? Count { get; set; }

        /// <summary>The <see cref="StardewValley.Object.preserve"/> value to match (or <c>null</c> to ignore it).</summary>
        public Object.PreserveType? PreserveType { get; set; }

        /// <summary>The <see cref="StardewValley.Object.preservedParentSheetIndex"/> value to match (or <c>null</c> to ignore it).</summary>
        public int? PreservedParentSheetIndex { get; set; }
    }
}
