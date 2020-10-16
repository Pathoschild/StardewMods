using Pathoschild.Stardew.LookupAnything.Framework.Data;
using StardewValley;
using SObject = StardewValley.Object;

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

        /// <summary>The <see cref="StardewValley.Object.preserve"/> value to match (or <c>null</c> to ignore it).</summary>
        public SObject.PreserveType? PreserveType { get; }

        /// <summary>The <see cref="StardewValley.Object.preservedParentSheetIndex"/> value to match (or <c>null</c> to ignore it).</summary>
        public int? PreservedParentSheetIndex { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="id">The unique item ID.</param>
        /// <param name="count">The number required.</param>
        /// <param name="preserveType">The <see cref="StardewValley.Object.preserve"/> value to match (or <c>null</c> to ignore it).</param>
        /// <param name="preservedParentSheetIndex">The <see cref="StardewValley.Object.preservedParentSheetIndex"/> value to match (or <c>null</c> to ignore it).</param>
        public RecipeIngredientModel(int id, int count, SObject.PreserveType? preserveType = null, int? preservedParentSheetIndex = null)
        {
            this.ID = id;
            this.Count = count;
            this.PreserveType = preserveType;
            this.PreservedParentSheetIndex = preservedParentSheetIndex;
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="ingredient">The ingredient to copy.</param>
        public RecipeIngredientModel(MachineRecipeIngredientData ingredient)
            : this(
                id: ingredient.ID,
                count: ingredient.Count ?? 1,
                preserveType: ingredient.PreserveType,
                preservedParentSheetIndex: ingredient.PreservedParentSheetIndex
            )
        { }

        /// <summary>Get whether the ingredient matches a given item.</summary>
        /// <param name="item">The item to check.</param>
        public bool Matches(Item item)
        {
            // ignore if null
            if (item == null)
                return false;

            // item fields
            if (item.ParentSheetIndex != this.ID && item.Category != this.ID)
                return false;

            // object fields
            if (this.PreservedParentSheetIndex != null || this.PreserveType != null)
            {
                if (!(item is SObject obj))
                    return false;
                if (this.PreservedParentSheetIndex != null && this.PreservedParentSheetIndex != obj.preservedParentSheetIndex.Value)
                    return false;
                if (this.PreserveType != null && this.PreserveType != obj.preserve.Value)
                    return false;
            }

            return true;
        }
    }
}
