using System.Collections.Generic;
using StardewValley;

namespace Pathoschild.LookupAnything.Framework.Models
{
    /// <summary>Represents metadata about a recipe.</summary>
    internal class RecipeData
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The underlying recipe.</summary>
        public CraftingRecipe Recipe { get; }

        /// <summary>The name of the recipe.</summary>
        public string Name { get; }

        /// <summary>How the recipe is used to create an object.</summary>
        public RecipeType Type { get; }

        /// <summary>The items needed to craft the recipe (item ID => number needed).</summary>
        public IDictionary<int, int> Ingredients { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="recipe">The recipe to parse.</param>
        public RecipeData(CraftingRecipe recipe)
        {
            this.Recipe = recipe;
            this.Name = recipe.name;
            this.Type = recipe.isCookingRecipe ? RecipeType.Cooking : RecipeType.Crafting;
            this.Ingredients = GameHelper.GetPrivateField<Dictionary<int, int>>(recipe, "recipeList");
        }

        /// <summary>Get whether a player knows this recipe.</summary>
        /// <param name="farmer">The farmer to check.</param>
        public bool KnowsRecipe(Farmer farmer)
        {
            return farmer.knowsRecipe(this.Name);
        }
    }
}