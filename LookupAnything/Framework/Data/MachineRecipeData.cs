namespace Pathoschild.Stardew.LookupAnything.Framework.Data
{
    /// <summary>Metadata for a machine recipe.</summary>
    internal class MachineRecipeData
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The machine item ID.</summary>
        public int MachineID { get; set; }

        /// <summary>The items needed to craft the recipe (item ID => number needed).</summary>
        public MachineRecipeIngredientData[] Ingredients { get; set; }

        /// <summary>The ingredients which can't be used in this recipe (typically exceptions for a category ingredient).</summary>
        public MachineRecipeIngredientData[] ExceptIngredients { get; set; }

        /// <summary>The item created by the recipe.</summary>
        public int Output { get; set; }

        /// <summary>The minimum number of items produced by the recipe (or <c>null</c> for the default).</summary>
        public int? MinOutput { get; set; }

        /// <summary>The maximum number of items produced by the recipe (or <c>null</c> for the default).</summary>
        public int? MaxOutput { get; set; }

        /// <summary>The percentage chance of this recipe being produced (or <c>null</c> if the recipe is always used).</summary>
        public decimal? OutputChance { get; set; }
    }
}
