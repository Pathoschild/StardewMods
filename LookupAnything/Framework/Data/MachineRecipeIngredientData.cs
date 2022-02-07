using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.LookupAnything.Framework.Data
{
    /// <summary>Metadata for an ingredient in a machine recipe.</summary>
    /// <param name="PossibleIds">The unique item IDs.</param>
    /// <param name="Count">The number required (or <c>null</c> for the default).</param>
    /// <param name="PreserveType">The <see cref="SObject.preserve"/> value to match (or <c>null</c> to ignore it).</param>
    /// <param name="PreservedParentSheetIndex">The <see cref="SObject.preservedParentSheetIndex"/> value to match (or <c>null</c> to ignore it).</param>
    internal record MachineRecipeIngredientData(
        string[] PossibleIds,
        int? Count,
        SObject.PreserveType? PreserveType,
        string? PreservedItemId
    );
}
