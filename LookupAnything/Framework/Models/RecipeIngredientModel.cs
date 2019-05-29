using Pathoschild.Stardew.LookupAnything.Framework.Data;

namespace Pathoschild.Stardew.LookupAnything.Framework.Models
{
    /// <summary>Metadata for an ingredient in a machine recipe.</summary>
    internal class RecipeIngredientModel
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The unique item ID.</summary>
        public int ID { get; }

        /// <summary>The number required.</summary>
        public int Count { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="id">The unique item ID.</param>
        /// <param name="count">The number required.</param>
        public RecipeIngredientModel(int id, int count)
        {
            this.ID = id;
            this.Count = count;
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="ingredient">The ingredient to copy.</param>
        public RecipeIngredientModel(MachineRecipeIngredientData ingredient)
            : this(
                id: ingredient.ID,
                count: ingredient.Count ?? 1
            )
        { }
    }
}
