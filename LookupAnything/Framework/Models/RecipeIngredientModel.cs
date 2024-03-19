using System.Collections.Generic;
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
        /// <summary>The unique item IDs that can be used for this ingredient slot.</summary>
        public ISet<string> PossibleIds { get; }

        /// <summary>The number required.</summary>
        public int Count { get; }

        /// <summary>The <see cref="SObject.preserve"/> value to match (or <c>null</c> to ignore it).</summary>
        public SObject.PreserveType? PreserveType { get; }

        /// <summary>The <see cref="SObject.preservedParentSheetIndex"/> value to match (or <c>null</c> to ignore it).</summary>
        public string? PreservedItemId { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="inputId">The unique item ID that can be used for this ingredient slot.</param>
        /// <param name="count">The number required.</param>
        /// <param name="preserveType">The <see cref="SObject.preserve"/> value to match (or <c>null</c> to ignore it).</param>
        /// <param name="preservedItemId">The <see cref="SObject.preservedParentSheetIndex"/> value to match (or <c>null</c> to ignore it).</param>
        public RecipeIngredientModel(string inputId, int count, SObject.PreserveType? preserveType = null, string? preservedItemId = null)
        {
            this.PossibleIds = new HashSet<string> { inputId };
            this.Count = count;
            this.PreserveType = preserveType;
            this.PreservedItemId = preservedItemId;
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="possibleIds">The unique item IDs that can be used for this ingredient slot.</param>
        /// <param name="count">The number required.</param>
        /// <param name="preserveType">The <see cref="SObject.preserve"/> value to match (or <c>null</c> to ignore it).</param>
        /// <param name="preservedItemId">The <see cref="SObject.preservedParentSheetIndex"/> value to match (or <c>null</c> to ignore it).</param>
        public RecipeIngredientModel(string[] possibleIds, int count, SObject.PreserveType? preserveType = null, string? preservedItemId = null)
        {
            this.PossibleIds = new HashSet<string>(possibleIds);
            this.Count = count;
            this.PreserveType = preserveType;
            this.PreservedItemId = preservedItemId;
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="ingredient">The ingredient to copy.</param>
        public RecipeIngredientModel(MachineRecipeIngredientData ingredient)
            : this(
                possibleIds: ingredient.PossibleIds,
                count: ingredient.Count ?? 1,
                preserveType: ingredient.PreserveType,
                preservedItemId: ingredient.PreservedItemId
            )
        { }

        /// <summary>Get whether the ingredient matches a given item.</summary>
        /// <param name="item">The item to check.</param>
        public bool Matches(Item? item)
        {
            // ignore if null
            if (item == null)
                return false;

            // item fields
            bool matchesId =
                this.PossibleIds.Contains(item.Category.ToString())
                || this.PossibleIds.Contains(item.ItemId)
                || this.PossibleIds.Contains(item.QualifiedItemId);
            if (!matchesId)
                return false;

            // object fields
            if (this.PreservedItemId != null || this.PreserveType != null)
            {
                if (item is not SObject obj)
                    return false;
                if (this.PreservedItemId != null && this.PreservedItemId != obj.preservedParentSheetIndex.Value)
                    return false;
                if (this.PreserveType != null && this.PreserveType != obj.preserve.Value)
                    return false;
            }

            return true;
        }
    }
}
