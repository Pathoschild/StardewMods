using System;
using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.LookupAnything.Framework.Fields.Models;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.Buildings;
using StardewValley.TokenizableStrings;

namespace Pathoschild.Stardew.LookupAnything.Framework.Models
{
    /// <summary>Represents metadata about a recipe.</summary>
    internal class RecipeModel
    {
        /*********
        ** Fields
        *********/
        /// <summary>The item that can be created by this recipe, given the ingredient.</summary>
        private readonly Func<Item?, Item?>? Item;


        /*********
        ** Accessors
        *********/
        /// <summary>The recipe's lookup name (if any).</summary>
        public string? Key { get; }

        /// <summary>The machine's unqualified item ID, if applicable.</summary>
        public string? MachineId { get; }

        /// <summary>Get whether this recipe is for the given machine.</summary>
        public Func<object, bool> IsForMachine { get; }

        /// <summary>The recipe type.</summary>
        public RecipeType Type { get; }

        /// <summary>The display name for the machine or building name for which the recipe applies.</summary>
        public string DisplayType { get; }

        /// <summary>The items needed to craft the recipe (item ID => number needed).</summary>
        public RecipeIngredientModel[] Ingredients { get; }

        /// <summary>The qualified item ID produced by this recipe, if applicable.</summary>
        public string? OutputQualifiedItemId { get; }

        /// <summary>The minimum number of items output by the recipe.</summary>
        public int MinOutput { get; }

        /// <summary>The maximum number of items output by the recipe.</summary>
        public int MaxOutput { get; }

        /// <summary>The percentage chance of this recipe being produced.</summary>
        public decimal OutputChance { get; set; }

        /// <summary>The ingredients which can't be used in this recipe (typically exceptions for a category ingredient).</summary>
        public RecipeIngredientModel[] ExceptIngredients { get; }

        /// <summary>Whether the player knows this recipe.</summary>
        public Func<bool> IsKnown { get; }

        /// <summary>The sprite and display text for a non-standard recipe output.</summary>
        public RecipeItemEntry? SpecialOutput { get; }

        /// <summary>The item quality that will be produced, if applicable.</summary>
        public int? Quality { get; }

        /// <summary>The game state queries which indicate when this recipe is available, if any.</summary>
        public string[] Conditions { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="key">The recipe's lookup name (if any).</param>
        /// <param name="type">The recipe type.</param>
        /// <param name="displayType">The display name for the machine or building name for which the recipe applies.</param>
        /// <param name="ingredients">The items needed to craft the recipe (item ID => number needed).</param>
        /// <param name="item">The item that's created by this recipe, given an optional input.</param>
        /// <param name="isKnown">Whether the player knows this recipe.</param>
        /// <param name="machineId">The machine's unqualified item ID, if applicable.</param>
        /// <param name="isForMachine">Get whether this recipe is for the given machine.</param>
        /// <param name="exceptIngredients">The ingredients which can't be used in this recipe (typically exceptions for a category ingredient).</param>
        /// <param name="outputQualifiedItemId">The qualified item ID produced by this recipe, if applicable.</param>
        /// <param name="minOutput">The minimum number of items output by the recipe.</param>
        /// <param name="maxOutput">The maximum number of items output by the recipe.</param>
        /// <param name="quality">The item quality that will be produced, if applicable.</param>
        /// <param name="outputChance">The percentage chance of this recipe being produced (or <c>null</c> if the recipe is always used).</param>
        /// <param name="conditions">The game state queries which indicate when this recipe is available, if any.</param>
        public RecipeModel(string? key, RecipeType type, string displayType, IEnumerable<RecipeIngredientModel> ingredients, Func<Item?, Item?>? item, Func<bool> isKnown, string? machineId, Func<object, bool> isForMachine, IEnumerable<RecipeIngredientModel>? exceptIngredients = null, string? outputQualifiedItemId = null, int? minOutput = null, int? maxOutput = null, decimal? outputChance = null, int? quality = null, string[]? conditions = null)
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
            this.MachineId = machineId;
            this.IsForMachine = isForMachine;
            this.ExceptIngredients = exceptIngredients?.ToArray() ?? Array.Empty<RecipeIngredientModel>();
            this.Item = item;
            this.IsKnown = isKnown;
            this.OutputQualifiedItemId = outputQualifiedItemId;
            this.MinOutput = minOutput!.Value;
            this.MaxOutput = maxOutput!.Value;
            this.OutputChance = outputChance is > 0 and < 100 ? outputChance.Value : 100;
            this.Quality = quality;
            this.Conditions = conditions ?? Array.Empty<string>();
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="recipe">The recipe to parse.</param>
        /// <param name="outputQualifiedItemId">The qualified item ID produced by this recipe.</param>
        /// <param name="ingredients">The items needed to craft the recipe, or <c>null</c> to parse them from the recipe.</param>
        public RecipeModel(CraftingRecipe recipe, string? outputQualifiedItemId, RecipeIngredientModel[]? ingredients = null)
            : this(
                key: recipe.name,
                type: recipe.isCookingRecipe ? RecipeType.Cooking : RecipeType.Crafting,
                displayType: recipe.isCookingRecipe ? I18n.RecipeType_Cooking() : I18n.RecipeType_Crafting(),
                ingredients: ingredients ?? RecipeModel.ParseIngredients(recipe),
                item: _ => recipe.createItem(),
                isKnown: () => recipe.name != null && Game1.player.knowsRecipe(recipe.name),
                minOutput: recipe.numberProducedPerCraft,
                machineId: null,
                isForMachine: _ => false,
                outputQualifiedItemId: RecipeModel.QualifyRecipeOutputId(recipe, outputQualifiedItemId) ?? outputQualifiedItemId
            ) { }

        /// <summary>Construct an instance.</summary>
        /// <param name="building">A sample building constructed by the blueprint.</param>
        /// <param name="ingredients">The items needed to construct the building.</param>
        public RecipeModel(Building building, RecipeIngredientModel[] ingredients)
            : this(
                key: TokenParser.ParseText(building.GetData()?.Name) ?? building.buildingType.Value,
                type: RecipeType.BuildingBlueprint,
                displayType: I18n.Building_Construction(),
                ingredients: ingredients,
                item: _ => null,
                isKnown: () => true,
                machineId: null,
                isForMachine: _ => false
            )
        {
            this.SpecialOutput = new RecipeItemEntry(
                Sprite: new SpriteInfo(building.texture.Value, building.getSourceRectForMenu() ?? building.getSourceRect()),
                DisplayText: TokenParser.ParseText(building.GetData()?.Name) ?? building.buildingType.Value,
                Quality: null
            );
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
                isKnown: other.IsKnown,
                exceptIngredients: other.ExceptIngredients,
                outputQualifiedItemId: other.OutputQualifiedItemId,
                minOutput: other.MinOutput,
                machineId: other.MachineId,
                isForMachine: other.IsForMachine,
                conditions: other.Conditions
            )
        { }

        /// <summary>Parse the ingredients for a recipe.</summary>
        /// <param name="recipe">The crafting recipe.</param>
        public static RecipeIngredientModel[] ParseIngredients(CraftingRecipe recipe)
        {
            return recipe.recipeList
                .Select(p => new RecipeIngredientModel(p.Key, p.Value))
                .ToArray();
        }

        /// <summary>Parse the ingredients for a recipe.</summary>
        /// <param name="building">The building data.</param>
        public static RecipeIngredientModel[] ParseIngredients(BuildingData? building)
        {
            if (building?.BuildMaterials?.Count > 0)
            {
                return building.BuildMaterials
                    .Select(ingredient => new RecipeIngredientModel(ingredient.ItemId, ingredient.Amount))
                    .ToArray();
            }
            else
                return Array.Empty<RecipeIngredientModel>();
        }

        /// <summary>Create the item crafted by this recipe if it's valid.</summary>
        /// <param name="ingredient">The optional ingredient for which to create an item.</param>
        public Item? TryCreateItem(Item? ingredient)
        {
            if (this.Item is null)
                return null;

            try
            {
                return this.Item(ingredient);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>Get the number of times this player has crafted the recipe.</summary>
        /// <returns>Returns the times crafted, or -1 if unknown (e.g. some recipe types like furnace aren't tracked).</returns>
        public int GetTimesCrafted(Farmer player)
        {
            switch (this.Type)
            {
                case RecipeType.Cooking:
                    {
                        string? localId = ItemRegistry.GetData(this.OutputQualifiedItemId)?.ItemId;
                        if (localId != null)
                            return player.recipesCooked.TryGetValue(localId, out int timesCooked) ? timesCooked : 0;
                    }
                    return 0;

                case RecipeType.Crafting:
                    return player.craftingRecipes.TryGetValue(this.Key, out int timesCrafted) ? timesCrafted : 0;

                default:
                    return -1;
            }
        }

        /// <summary>Qualify an item ID produced by a recipe, if needed.</summary>
        /// <param name="recipe">The recipes whose output is being qualified.</param>
        /// <param name="itemId">The item ID to qualify.</param>
        public static string? QualifyRecipeOutputId(CraftingRecipe recipe, string? itemId)
        {
            return recipe.bigCraftable
                ? ItemRegistry.ManuallyQualifyItemId(itemId, ItemRegistry.type_bigCraftable)
                : ItemRegistry.QualifyItemId(itemId);
        }
    }
}
