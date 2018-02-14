using System;
using System.Collections.Generic;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using StardewModdingAPI;
using StardewValley;
using SFarmer = StardewValley.Farmer;

namespace Pathoschild.Stardew.LookupAnything.Framework.Models
{
    /// <summary>Represents metadata about a recipe.</summary>
    internal class RecipeModel
    {
        /*********
        ** Properties
        *********/
        /// <summary>The item that be created by this recipe, given the ingredient.</summary>
        private readonly Func<Item, Item> Item;


        /*********
        ** Accessors
        *********/
        /// <summary>The recipe's lookup name (if any).</summary>
        public string Key { get; }

        /// <summary>The display name for the machine or building name for which the recipe applies.</summary>
        public string DisplayType { get; }

        /// <summary>The items needed to craft the recipe (item ID => number needed).</summary>
        public IDictionary<int, int> Ingredients { get; }

        /// <summary>The ingredients which can't be used in this recipe (typically exceptions for a category ingredient).</summary>
        public int[] ExceptIngredients { get; }

        /// <summary>Whether the recipe must be learned before it can be used.</summary>
        public bool MustBeLearned { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="recipe">The recipe to parse.</param>
        /// <param name="reflectionHelper">Simplifies access to private game code.</param>
        /// <param name="translations">Provides translations stored in the mod folder.</param>
        public RecipeModel(CraftingRecipe recipe, IReflectionHelper reflectionHelper, ITranslationHelper translations)
            : this(
                key: recipe.name,
                displayType: translations.Get(recipe.isCookingRecipe ? L10n.RecipeTypes.Cooking : L10n.RecipeTypes.Crafting),
                ingredients: reflectionHelper.GetField<Dictionary<int, int>>(recipe, "recipeList").GetValue(),
                item: item => recipe.createItem(),
                mustBeLearned: true
            )
        { }

        /// <summary>Construct an instance.</summary>
        /// <param name="key">The recipe's lookup name (if any).</param>
        /// <param name="displayType">The display name for the machine or building name for which the recipe applies.</param>
        /// <param name="ingredients">The items needed to craft the recipe (item ID => number needed).</param>
        /// <param name="item">The item that be created by this recipe.</param>
        /// <param name="mustBeLearned">Whether the recipe must be learned before it can be used.</param>
        /// <param name="exceptIngredients">The ingredients which can't be used in this recipe (typically exceptions for a category ingredient).</param>
        public RecipeModel(string key, string displayType, IDictionary<int, int> ingredients, Func<Item, Item> item, bool mustBeLearned, int[] exceptIngredients = null)
        {
            this.Key = key;
            this.DisplayType = displayType;
            this.Ingredients = ingredients;
            this.ExceptIngredients = exceptIngredients ?? new int[0];
            this.Item = item;
            this.MustBeLearned = mustBeLearned;
        }

        /// <summary>Create the item crafted by this recipe.</summary>
        /// <param name="ingredient">The ingredient for which to create an item.</param>
        public Item CreateItem(Item ingredient)
        {
            return this.Item(ingredient);
        }

        /// <summary>Get whether a player knows this recipe.</summary>
        /// <param name="farmer">The farmer to check.</param>
        public bool KnowsRecipe(SFarmer farmer)
        {
            return this.Key != null && farmer.knowsRecipe(this.Key);
        }
    }
}
