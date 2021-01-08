namespace Pathoschild.Stardew.LookupAnything.Framework.Data
{
    /// <summary>Metadata for a machine recipe.</summary>
    internal class MachineRecipeData
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The items needed to craft the recipe (item ID => number needed).</summary>
        public MachineRecipeIngredientData[] Ingredients { get; set; }

        /// <summary>The ingredients which can't be used in this recipe (typically exceptions for a category ingredient).</summary>
        public MachineRecipeIngredientData[] ExceptIngredients { get; set; }

        /// <summary>Metadata for possible output items in a machine recipe.</summary>
        public MachineRecipeOutputData[] PossibleOutputs { get; set; }
    }
}
