//using System.Linq;
//using StardewValley;
//using PreserveType = StardewValley.Object.PreserveType;

//namespace Pathoschild.Stardew.Common.Integrations.ProducerFrameworkMod
//{
//    /// <summary>Metadata about a recipe provided by Producer Framework Mod.</summary>
//    internal class ProducerFrameworkRecipe
//    {
//        /*********
//        ** Accessors
//        *********/
//        /// <summary>The ID for the main input ingredient, or <c>null</c> for a context tag.</summary>
//        public int? InputId { get; }

//        /// <summary>The ID for the machine item.</summary>
//        public int MachineId { get; }

//        /// <summary>The items needed to craft the recipe (item ID => number needed).</summary>
//        public ProducerFrameworkIngredient[] Ingredients { get; }

//        /// <summary>The ingredients which can't be used in this recipe, including nulls for context tag ingredients.</summary>
//        public int?[] ExceptIngredients { get; }

//        /// <summary>The item ID produced by this recipe.</summary>
//        public int OutputId { get; }

//        /// <summary>The minimum number of items output by the recipe.</summary>
//        public int MinOutput { get; }

//        /// <summary>The maximum number of items output by the recipe.</summary>
//        public int MaxOutput { get; }

//        /// <summary>The percentage chance of this recipe being produced.</summary>
//        public double OutputChance { get; }

//        /// <summary>The produced preserve type, if any.</summary>
//        public PreserveType? PreserveType { get; }


//        /*********
//        ** Public methods
//        *********/
//        /// <summary>Construct an instance.</summary>
//        /// <param name="inputId">The ID for the main input ingredient, or <c>null</c> for a context tag.</param>
//        /// <param name="machineId">The ID for the machine item.</param>
//        /// <param name="ingredients">The items needed to craft the recipe (item ID => number needed).</param>
//        /// <param name="exceptIngredients">The ingredients which can't be used in this recipe, including nulls for context tag ingredients.</param>
//        /// <param name="outputId">The item ID produced by this recipe.</param>
//        /// <param name="minOutput">The minimum number of items output by the recipe.</param>
//        /// <param name="maxOutput">The maximum number of items output by the recipe.</param>
//        /// <param name="outputChance">The percentage chance of this recipe being produced.</param>
//        /// <param name="preserveType">The produced preserve type, if any.</param>
//        public ProducerFrameworkRecipe(int? inputId, int machineId, ProducerFrameworkIngredient[] ingredients, int?[] exceptIngredients, int outputId, int minOutput, int maxOutput, double outputChance, PreserveType? preserveType)
//        {
//            this.InputId = inputId;
//            this.MachineId = machineId;
//            this.Ingredients = ingredients;
//            this.ExceptIngredients = exceptIngredients;
//            this.OutputId = outputId;
//            this.MinOutput = minOutput;
//            this.MaxOutput = maxOutput;
//            this.OutputChance = outputChance;
//            this.PreserveType = preserveType;
//        }

//        /// <summary>Get whether the recipe uses context tags instead of IDs for some recipes, which aren't supported by the integration.</summary>
//        public bool HasContextTags()
//        {
//            return
//                this.InputId == null
//                || this.Ingredients.Any(p => p.InputId == null)
//                || this.ExceptIngredients.Any(p => p == null);
//        }
//    }
//}
