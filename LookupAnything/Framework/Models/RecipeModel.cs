using System;
using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Common.Items.ItemData;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace Pathoschild.Stardew.LookupAnything.Framework.Models
{
    /// <summary>Represents metadata about a recipe.</summary>
    internal class RecipeModel
    {
        /*********
        ** Fields
        *********/
        /// <summary>The item that be created by this recipe, given the ingredient.</summary>
        private readonly Func<Item, Item> Item;


        /*********
        ** Accessors
        *********/
        /// <summary>The recipe's lookup name (if any).</summary>
        public string Key { get; }

        /// <summary>The object parent sheet index for the machine, if applicable.</summary>
        public int? MachineParentSheetIndex { get; }

        /// <summary>Get whether this recipe is for the given machine.</summary>
        public Func<object, bool> IsForMachine { get; }

        /// <summary>The recipe type.</summary>
        public RecipeType Type { get; }

        /// <summary>The display name for the machine or building name for which the recipe applies.</summary>
        public string DisplayType { get; }

        /// <summary>The items needed to craft the recipe (item ID => number needed).</summary>
        public RecipeIngredientModel[] Ingredients { get; }

        /// <summary>The item ID produced by this recipe, if applicable.</summary>
        public int? OutputItemIndex { get; }

        /// <summary>The item type produced by this recipe, if applicable.</summary>
        public ItemType? OutputItemType { get; }

        /// <summary>The minimum number of items output by the recipe.</summary>
        public int MinOutput { get; }

        /// <summary>The maximum number of items output by the recipe.</summary>
        public int MaxOutput { get; }

        /// <summary>The percentage chance of this recipe being produced.</summary>
        public decimal OutputChance { get; set; }

        /// <summary>The ingredients which can't be used in this recipe (typically exceptions for a category ingredient).</summary>
        public RecipeIngredientModel[] ExceptIngredients { get; }

        /// <summary>Whether the recipe must be learned before it can be used.</summary>
        public bool MustBeLearned { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="key">The recipe's lookup name (if any).</param>
        /// <param name="type">The recipe type.</param>
        /// <param name="displayType">The display name for the machine or building name for which the recipe applies.</param>
        /// <param name="ingredients">The items needed to craft the recipe (item ID => number needed).</param>
        /// <param name="item">The item that's created by this recipe, given an optional input.</param>
        /// <param name="mustBeLearned">Whether the recipe must be learned before it can be used.</param>
        /// <param name="machineParentSheetIndex">The object parent sheet index for the machine, if applicable.</param>
        /// <param name="isForMachine">Get whether this recipe is for the given machine.</param>
        /// <param name="exceptIngredients">The ingredients which can't be used in this recipe (typically exceptions for a category ingredient).</param>
        /// <param name="outputItemIndex">The item ID produced by this recipe, if applicable.</param>
        /// <param name="outputItemType">The item type produced by this recipe, if applicable.</param>
        /// <param name="minOutput">The minimum number of items output by the recipe.</param>
        /// <param name="maxOutput">The maximum number of items output by the recipe.</param>
        /// <param name="outputChance">The percentage chance of this recipe being produced (or <c>null</c> if the recipe is always used).</param>
        public RecipeModel(string key, RecipeType type, string displayType, IEnumerable<RecipeIngredientModel> ingredients, Func<Item, Item> item, bool mustBeLearned, int? machineParentSheetIndex, Func<object, bool> isForMachine, IEnumerable<RecipeIngredientModel> exceptIngredients = null, int? outputItemIndex = null, ItemType? outputItemType = null, int? minOutput = null, int? maxOutput = null, decimal? outputChance = null)
        {
            // normalize values
            if (minOutput == null && maxOutput == null)
            {
                minOutput = 1;
                maxOutput = 1;
            }
            else if (minOutput == null)
                minOutput = maxOutput;
            else if (maxOutput == null)
                maxOutput = minOutput;

            // save values
            this.Key = key;
            this.Type = type;
            this.DisplayType = displayType;
            this.Ingredients = ingredients.ToArray();
            this.MachineParentSheetIndex = machineParentSheetIndex;
            this.IsForMachine = isForMachine;
            this.ExceptIngredients = exceptIngredients?.ToArray() ?? new RecipeIngredientModel[0];
            this.Item = item;
            this.MustBeLearned = mustBeLearned;
            this.OutputItemIndex = outputItemIndex;
            this.OutputItemType = outputItemType;
            this.MinOutput = minOutput.Value;
            this.MaxOutput = maxOutput.Value;
            this.OutputChance = outputChance > 0 && outputChance < 100 ? outputChance.Value : 100;
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="recipe">The recipe to parse.</param>
        /// <param name="reflectionHelper">Simplifies access to private game code.</param>
        public RecipeModel(CraftingRecipe recipe, IReflectionHelper reflectionHelper)
            : this(
                key: recipe.name,
                type: recipe.isCookingRecipe ? RecipeType.Cooking : RecipeType.Crafting,
                displayType: recipe.isCookingRecipe ? L10n.RecipeTypes.Cooking() : L10n.RecipeTypes.Crafting(),
                ingredients: reflectionHelper
                    .GetField<Dictionary<int, int>>(recipe, "recipeList").GetValue()
                    .Select(p => new RecipeIngredientModel(p.Key, p.Value)),
                item: item => recipe.createItem(),
                mustBeLearned: true,
                minOutput: recipe.numberProducedPerCraft,
                machineParentSheetIndex: null,
                isForMachine: obj => false
            )
        {
            this.OutputItemIndex = reflectionHelper.GetField<List<int>>(recipe, "itemToProduce").GetValue()[0];
            this.OutputItemType = this.GetItemType(recipe, this.OutputItemIndex.Value);
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="other">The other recipe to clone.</param>
        public RecipeModel(RecipeModel other)
            : this(
                key: other.Key,
                type: other.Type,
                displayType: other.DisplayType,
                ingredients: other.Ingredients,
                item: other.Item,
                mustBeLearned: other.MustBeLearned,
                exceptIngredients: other.ExceptIngredients,
                outputItemIndex: other.OutputItemIndex,
                outputItemType: other.OutputItemType,
                minOutput: other.MinOutput,
                machineParentSheetIndex: other.MachineParentSheetIndex,
                isForMachine: other.IsForMachine
            )
        { }

        /// <summary>Create the item crafted by this recipe.</summary>
        /// <param name="ingredient">The optional ingredient for which to create an item.</param>
        public Item CreateItem(Item ingredient)
        {
            return this.Item(ingredient);
        }

        /// <summary>Get whether a player knows this recipe.</summary>
        /// <param name="farmer">The farmer to check.</param>
        public bool KnowsRecipe(Farmer farmer)
        {
            return this.Key != null && farmer.knowsRecipe(this.Key);
        }

        /// <summary>Get the number of times this player has crafted the recipe.</summary>
        /// <returns>Returns the times crafted, or -1 if unknown (e.g. some recipe types like furnace aren't tracked).</returns>
        public int GetTimesCrafted(Farmer player)
        {
            switch (this.Type)
            {
                case RecipeType.Cooking:
                    return this.OutputItemIndex.HasValue && player.recipesCooked.TryGetValue(this.OutputItemIndex.Value, out int timesCooked) ? timesCooked : 0;

                case RecipeType.Crafting:
                    return player.craftingRecipes.TryGetValue(this.Key, out int timesCrafted) ? timesCrafted : 0;

                default:
                    return -1;
            }
        }

        /// <summary>Get the item type produced by a recipe.</summary>
        /// <param name="recipe">The recipe.</param>
        /// <param name="itemID">The produced item ID.</param>
        /// <remarks>Derived from <see cref="CraftingRecipe.createItem"/>.</remarks>
        private ItemType GetItemType(CraftingRecipe recipe, int itemID)
        {
            if (recipe.bigCraftable)
                return ItemType.BigCraftable;

            if (itemID >= Ring.ringLowerIndexRange && itemID <= Ring.ringUpperIndexRange || itemID == 801)
                return ItemType.Ring;

            return ItemType.Object;
        }
    }
}
