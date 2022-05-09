namespace Pathoschild.Stardew.LookupAnything.Framework.Data
{
    /// <summary>Metadata for a machine's recipes.</summary>
    /// <param name="MachineID">The machine item ID.</param>
    /// <param name="Recipes">The machine recipes.</param>
    internal record MachineRecipesData(int MachineID, MachineRecipeData[] Recipes);
}
