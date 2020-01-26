using System.Collections.Generic;
using StardewValley;

namespace Pathoschild.Stardew.Common.Integrations.ProducerFrameworkMod
{
    /// <summary>Metadata about a recipe provided by Producer Framework Mod.</summary>
    internal class ProducerFrameworkRecipe
    {
        /// <summary>The ID for the main input ingredient, or <c>null</c> for a context tag.</summary>
        public int? InputId { get; set; }

        /// <summary>The ID for the machine item.</summary>
        public int MachineId { get; set; }

        /// <summary>The items needed to craft the recipe (item ID => number needed).</summary>
        public IDictionary<int, int> Ingredients { get; set; }

        /// <summary>>The ingredients which can't be used in this recipe</summary>
        public int[] ExceptIngredients { get; set; }

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
    }
}
