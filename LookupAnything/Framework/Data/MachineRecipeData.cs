namespace Pathoschild.Stardew.LookupAnything.Framework.Data
{
    /// <summary>Metadata for a machine recipe.</summary>
    /// <param name="Ingredients">The items needed to craft the recipe (item ID => number needed).</param>
    /// <param name="ExceptIngredients">The ingredients which can't be used in this recipe (typically exceptions for a category ingredient).</param>
    /// <param name="PossibleOutputs">Metadata for possible output items in a machine recipe.</param>
    internal record MachineRecipeData(
        MachineRecipeIngredientData[] Ingredients,
        MachineRecipeIngredientData[]? ExceptIngredients,
        MachineRecipeOutputData[] PossibleOutputs
    );
}
