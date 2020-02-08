using System.Linq;
using StardewValley;

namespace Pathoschild.Stardew.Common.Integrations.ProducerFrameworkMod
{
    /// <summary>Metadata about a recipe provided by Producer Framework Mod.</summary>
    internal class ProducerFrameworkRecipe
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The ID for the main input ingredient, or <c>null</c> for a context tag.</summary>
        public int? InputId { get; set; }

        /// <summary>The ID for the machine item.</summary>
        public int MachineId { get; set; }

        /// <summary>The items needed to craft the recipe (item ID => number needed).</summary>
        public ProducerFrameworkIngredient[] Ingredients { get; set; }

        /// <summary>The ingredients which can't be used in this recipe, including nulls for context tag ingredients.</summary>
        public int?[] ExceptIngredients { get; set; }

        /// <summary>The item ID produced by this recipe.</summary>
        public int OutputId { get; set; }

        /// <summary>The minimum number of items output by the recipe.</summary>
        public int MinOutput { get; set; }

        /// <summary>The maximum number of items output by the recipe.</summary>
        public int MaxOutput { get; set; }

        /// <summary>The percentage chance of this recipe being produced.</summary>
        public double OutputChance { get; set; }

        /// <summary>The produced preserve type, if any.</summary>
        public Object.PreserveType? PreserveType { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get whether the recipe uses context tags instead of IDs for some recipes, which aren't supported by the integration.</summary>
        public bool HasContextTags()
        {
            return
                this.InputId == null
                || this.Ingredients.Any(p => p.InputId == null)
                || this.ExceptIngredients.Any(p => p == null);
        }
    }
}
