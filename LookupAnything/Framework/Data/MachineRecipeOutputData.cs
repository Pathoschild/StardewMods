using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.LookupAnything.Framework.Data
{
    /// <summary>Metadata for an output item in a machine recipe.</summary>
    /// <param name="Ids">The item IDs created by the recipe.</param>
    /// <param name="MinOutput">The minimum number of items produced by the recipe (or <c>null</c> for the default).</param>
    /// <param name="MaxOutput">The maximum number of items produced by the recipe (or <c>null</c> for the default).</param>
    /// <param name="OutputChance">The percentage chance of this recipe being produced (or <c>null</c> if the recipe is always used).</param>
    /// <param name="PreserveType">The <see cref="SObject.preserve"/> value to match (or <c>null</c> to ignore it).</param>
    /// <param name="PreservedParentSheetIndex">The <see cref="SObject.preservedParentSheetIndex"/> value to match (or <c>null</c> to ignore it).</param>
    internal record MachineRecipeOutputData(
        string[] Ids,
        int? MinOutput,
        int? MaxOutput,
        decimal? OutputChance,
        SObject.PreserveType? PreserveType,
        string? PreservedParentSheetIndex
    );
}
