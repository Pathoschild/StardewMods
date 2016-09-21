using System.Collections.Generic;

namespace Pathoschild.LookupAnything.Framework.Data
{
    /// <summary>Metadata for a recipe that isn't available from the game data directly.</summary>
    internal class RecipeData
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The name of the recipe.</summary>
        public string Name { get; set; }

        /// <summary>How the recipe is used to create an object.</summary>
        public RecipeType Type { get; set; }

        /// <summary>The items needed to craft the recipe (item ID => number needed).</summary>
        public IDictionary<int, int> Ingredients { get; set; }

        /// <summary>The ingredients which can't be used in this recipe (typically exceptions for a category ingredient).</summary>
        public int[] ExceptIngredients { get; set; }

        /// <summary>The item created by the recipe.</summary>
        public int Output { get; set; }
    }
}
