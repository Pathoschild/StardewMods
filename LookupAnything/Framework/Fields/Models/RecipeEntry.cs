namespace Pathoschild.Stardew.LookupAnything.Framework.Fields.Models
{
    /// <summary>A recipe model for a recipe list.</summary>
    internal struct RecipeEntry
    {
        /// <summary>The recipe name or key.</summary>
        public string Name;

        /// <summary>The recipe type.</summary>
        public string Type;

        /// <summary>Whether the player knows the recipe.</summary>
        public bool IsKnown;

        /// <summary>The input items.</summary>
        public RecipeItemEntry[] Inputs;

        /// <summary>The output items.</summary>
        public RecipeItemEntry Output;
    }
}
