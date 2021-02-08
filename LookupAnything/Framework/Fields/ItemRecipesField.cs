using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.LookupAnything.Framework.Models;
using StardewValley;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields
{
    /// <summary>
    /// A metadata field which shows a list of recipes that
    /// contain an ingredient, or create an output, or are for a given machine.
    /// </summary>
    internal class ItemRecipesField : GenericField
    {
        /*********
        ** Fields
        *********/
        /// <summary>Metadata for a drawn input or output item.</summary>
        private struct ItemEntry
        {
            /// <summary>The sprite to display.</summary>
            public SpriteInfo Sprite;

            /// <summary>The display text for the item name and count.</summary>
            public string DisplayText;

            /// <summary>The pixel size of the display text.</summary>
            public Vector2 DisplayTextSize;
        }

        /// <summary>Metadata needed to draw a recipe.</summary>
        private struct RecipeEntry
        {
            /// <summary>The recipe name.</summary>
            public string Name;

            /// <summary>The recipe type.</summary>
            public string Type;

            /// <summary>Whether the player knows the recipe.</summary>
            public bool IsKnown;

            /// <summary>The input items.</summary>
            public ItemEntry[] Inputs;

            /// <summary>The output items.</summary>
            public ItemEntry Output;
        }

        /// <summary>The recipe data to list (type => recipe => {player knows recipe, number required for recipe}).</summary>
        private readonly RecipeEntry[] Recipes;

        /// <summary>Whether to align input/output columns.</summary>
        private readonly bool AlignColumns;

        /// <summary>The input column widths, if <see cref="AlignColumns"/> is true.</summary>
        private readonly int[] InputColumnWidths;

        /// <summary>Whether or not to group by recipe type.</summary>
        private readonly bool GroupByType;

        /// <summary>Whether or not to show all ingredients and output, or just the ingredient.</summary>
        private readonly bool ShowFullRecipe;

        /// <summary>The indent from the left when drawing the lookup UI.</summary>
        private readonly int LeftIndent = 16;

        /// <summary>The margin from the bottom when drawing the lookup UI.</summary>
        private readonly int BottomMargin = 5;

        /*********
        ** Public methods
        *********/
        /// <summary>Create an item recipe field that shows recipes that use an item as an ingredient and the number of items needed.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="label">A short field label.</param>
        /// <param name="ingredient">The ingredient item.</param>
        /// <param name="recipes">The recipe to list.</param>
        /// <returns></returns>
        public static ItemRecipesField ForIngredient(GameHelper gameHelper, string label, Item ingredient,
            RecipeModel[] recipes)
        {
            return new(gameHelper, label, ingredient, recipes);
        }

        /// <summary>Create an item recipe field that shows recipes for an item, with ingredients and the output.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="label">A short field label.</param>
        /// <param name="recipes">The recipes to list.</param>
        /// <returns></returns>
        public static ItemRecipesField ForOutput(GameHelper gameHelper, string label, RecipeModel[] recipes)
        {
            return new(gameHelper, label, recipes, true);
        }

        /// <summary>Create an item recipe field that shows recipes for a machine, with ingredients and the output.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="label">A short field label.</param>
        /// <param name="recipes">The recipes to list.</param>
        /// <returns></returns>
        public static ItemRecipesField ForMachine(GameHelper gameHelper, string label, RecipeModel[] recipes)
        {
            return new(gameHelper, label, recipes, false);
        }

        /// <inheritdoc />
        public override Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
        {
            float topOffset = 0;

            // get icon size
            float textHeight = font.MeasureString("ABC").Y;
            float spaceWidth = font.MeasureString("W").X;
            Vector2 iconSize = new Vector2(textHeight);

            // draw recipes
            string lastType = null;
            foreach (RecipeEntry entry in this.Recipes)
            {
                float leftOffset = this.LeftIndent;
                float bottomMargin = this.BottomMargin;
                Color iconColor = entry.IsKnown ? Color.White : Color.White * .5f;
                Color textColor = entry.IsKnown ? Color.Black : Color.Gray;
                float lineHeight = iconSize.Y + bottomMargin;

                // draw type
                if (this.GroupByType && entry.Type != lastType)
                {
                    spriteBatch.DrawTextBlock(font, $"{entry.Type}:",
                        position + new Vector2(0, topOffset + bottomMargin), wrapWidth, Color.Black);
                    topOffset += lineHeight;
                    lastType = entry.Type;
                }

                // draw inputs
                for (int i = 0, last = entry.Inputs.Length - 1; i <= last; i++)
                {
                    ItemEntry input = entry.Inputs[i];

                    // icon
                    if (input.Sprite != null)
                        spriteBatch.DrawSpriteWithin(input.Sprite, position.X + leftOffset, position.Y + topOffset,
                            iconSize, iconColor);
                    leftOffset += iconSize.X;

                    // display text
                    Vector2 textSize = spriteBatch.DrawTextBlock(font, input.DisplayText,
                        position + new Vector2(leftOffset, topOffset + 5), wrapWidth - leftOffset, textColor);
                    leftOffset += this.AlignColumns ? this.InputColumnWidths[i] : textSize.X;
                    lineHeight = Math.Max(lineHeight, textSize.Y);

                    // joiner
                    if (i == last)
                        continue;

                    Vector2 joinerSize = spriteBatch.DrawTextBlock(font, "+",
                        position + new Vector2(leftOffset + spaceWidth, topOffset + 5), wrapWidth - leftOffset,
                        textColor);
                    leftOffset += joinerSize.X + (2 * spaceWidth);
                    lineHeight = Math.Max(lineHeight, joinerSize.Y);
                }

                // draw output
                if (this.ShowFullRecipe)
                {
                    // joiner
                    Vector2 joinerSize = spriteBatch.DrawTextBlock(font, ">",
                        position + new Vector2(leftOffset + spaceWidth, topOffset + 5), wrapWidth - leftOffset,
                        textColor);
                    leftOffset += joinerSize.X + (2 * spaceWidth);
                    lineHeight = Math.Max(lineHeight, joinerSize.Y);

                    // icon
                    if (entry.Output.Sprite != null)
                        spriteBatch.DrawSpriteWithin(entry.Output.Sprite, position.X + leftOffset,
                            position.Y + topOffset, iconSize, iconColor);
                    leftOffset += iconSize.X;

                    // name + count + chance
                    Vector2 textSize = spriteBatch.DrawTextBlock(font, entry.Output.DisplayText,
                        position + new Vector2(leftOffset, topOffset + 5), wrapWidth - leftOffset, textColor);
                    lineHeight = Math.Max(lineHeight, textSize.Y);
                }

                topOffset += lineHeight;
            }

            return new Vector2(wrapWidth, topOffset);
        }

        /// <summary>Create an item recipe field that shows recipes with ingredients and the output.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="label">A short field label.</param>
        /// <param name="recipes">The recipes to list.</param>
        /// <param name="groupByType">Whether or not to group recipes by type.</param>
        private ItemRecipesField(GameHelper gameHelper, string label, RecipeModel[] recipes, bool groupByType)
            : base(label, true)
        {
            this.GroupByType = groupByType;
            this.ShowFullRecipe = true;

            // get recipe data
            var availableRecipes = new List<RecipeEntry>();
            foreach (RecipeModel recipe in recipes)
            {
                bool isKnown = !recipe.MustBeLearned || recipe.KnowsRecipe(Game1.player);

                foreach (int[] inputIds in this.GetCartesianInputs(recipe))
                {
                    // get inputs
                    var inputs = new ItemEntry[inputIds.Length];
                    for (int i = 0; i < inputIds.Length; i++)
                    {
                        int id = inputIds[i];
                        RecipeIngredientModel ingredient = recipe.Ingredients[i];

                        // category
                        if (id < 0)
                        {
                            Item sampleInput = gameHelper.GetObjectsByCategory(id).FirstOrDefault();
                            if (sampleInput == null)
                                continue;

                            string displayText = this.GetRecipeItemDisplayText(name: sampleInput.getCategoryName(),
                                minCount: ingredient.Count, maxCount: ingredient.Count, chance: 100);
                            inputs[i] = new ItemEntry
                            {
                                Sprite = null,
                                DisplayText = displayText,
                                DisplayTextSize = Game1.smallFont.MeasureString(displayText)
                            };
                        }

                        // item
                        else
                        {
                            Item input = gameHelper.GetObjectBySpriteIndex(id);
                            if (input is SObject obj)
                            {
                                if (ingredient.PreservedParentSheetIndex != null)
                                    obj.preservedParentSheetIndex.Value = ingredient.PreservedParentSheetIndex.Value;
                                if (ingredient.PreserveType != null)
                                    obj.preserve.Value = ingredient.PreserveType.Value;
                            }

                            if (input == null)
                                continue;

                            string displayText = this.GetRecipeItemDisplayText(name: input.DisplayName,
                                minCount: ingredient.Count, maxCount: ingredient.Count, chance: 100);
                            inputs[i] = new ItemEntry
                            {
                                Sprite = gameHelper.GetSprite(input),
                                DisplayText = displayText,
                                DisplayTextSize = Game1.smallFont.MeasureString(displayText)
                            };
                        }
                    }

                    // build recipe
                    availableRecipes.Add(new RecipeEntry
                    {
                        Name = null,
                        Type = recipe.DisplayType,
                        IsKnown = isKnown,
                        Inputs = inputs.ToArray(),
                        Output = this.CreateOutputEntryItem(gameHelper, recipe),
                    });
                }
            }

            this.Recipes = availableRecipes
                .OrderBy(entry => string.Join(", ", entry.Inputs.SelectMany(input => input.DisplayText)))
                .ThenBy(entry => entry.Output.DisplayText)
                .ToArray();

            // calculate column alignment
            if (!this.Recipes.Any())
                return;

            RecipeEntry firstRecipe = this.Recipes.First();
            int inputCount = firstRecipe.Inputs.Length;

            if (this.Recipes.Any(p => p.Inputs.Length != inputCount))
                return;

            this.AlignColumns = true;
            this.InputColumnWidths = firstRecipe.Inputs.Select(p => (int)p.DisplayTextSize.X).ToArray();

            foreach (RecipeEntry recipe in this.Recipes.Skip(1))
            {
                for (int i = 0; i < recipe.Inputs.Length; i++)
                {
                    int inputWidth = (int)recipe.Inputs[i].DisplayTextSize.X;
                    if (inputWidth > this.InputColumnWidths[i])
                        this.InputColumnWidths[i] = inputWidth;
                }
            }
        }

        /// <summary>Create an item recipe field that shows recipes that use an item as an ingredient and the number of items needed.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="label">A short field label.</param>
        /// <param name="item">The ingredient item.</param>
        /// <param name="recipes">The recipe to list.</param>
        private ItemRecipesField(GameHelper gameHelper, string label, Item item, RecipeModel[] recipes)
            : base(label, hasValue: true)
        {
            this.GroupByType = true;
            this.ShowFullRecipe = false;

            var availableRecipes = new List<RecipeEntry>();
            foreach (RecipeModel recipe in recipes)
            {
                Item output = recipe.CreateItem(item);
                RecipeIngredientModel ingredient =
                    recipe.Ingredients.FirstOrDefault(p =>
                        p.PossibleIds.Contains(item.ParentSheetIndex) && p.Matches(item))
                    ?? recipe.Ingredients.FirstOrDefault(p => p.PossibleIds.Contains(item.Category) && p.Matches(item));

                var inputs = new List<ItemEntry>();
                string displayText =
                    I18n.Item_RecipesForIngredient_Entry(name: output.DisplayName, count: ingredient?.Count ?? 1);
                var input1 = new ItemEntry
                {
                    Sprite = gameHelper.GetSprite(output),
                    DisplayText = displayText,
                    DisplayTextSize = Game1.smallFont.MeasureString(displayText)
                };

                inputs.Add(input1);
                availableRecipes.Add(new RecipeEntry
                {
                    Name = output.DisplayName,
                    Type = recipe.DisplayType,
                    IsKnown = !recipe.MustBeLearned || recipe.KnowsRecipe(Game1.player),
                    Inputs = inputs.ToArray(),
                    Output = this.CreateOutputEntryItem(gameHelper, recipe),
                }
                );
            }

            this.Recipes = this.DistinctBy(availableRecipes, p => new { p.Name, p.Type })
                .OrderBy(p => p.Type)
                .ThenBy(p => p.Name)
                .ToArray();
        }

        /*********
        ** Private methods
        *********/
        private ItemEntry CreateOutputEntryItem(GameHelper gameHelper, RecipeModel recipe)
        {
            Item outputItem = recipe.CreateItem(null);
            string displayText = this.GetRecipeItemDisplayText(name: outputItem.DisplayName, minCount: recipe.MinOutput,
                maxCount: recipe.MaxOutput, chance: recipe.OutputChance);
            var output = new ItemEntry
            {
                Sprite = gameHelper.GetSprite(outputItem),
                DisplayText = displayText,
                DisplayTextSize = Game1.smallFont.MeasureString(displayText)
            };
            return output;
        }

        /// <summary>Get the display text for an input or output item.</summary>
        /// <param name="name">The item name.</param>
        /// <param name="minCount">The minimum number needed or produced.</param>
        /// <param name="maxCount">The maximum number needed or produced.</param>
        /// <param name="chance">The chance of the item being produced.</param>
        /// <returns>A string representing the item, min and max, and chance.</returns>
        private string GetRecipeItemDisplayText(string name, int minCount, int maxCount, decimal chance)
        {
            // get name + count
            string text;
            if (minCount != maxCount)
                text = I18n.Item_RecipesForMachine_MultipleItems(name: name,
                    count: I18n.Generic_Range(min: minCount, max: maxCount));
            else if (minCount > 1)
                text = I18n.Item_RecipesForMachine_MultipleItems(name: name, count: minCount);
            else
                text = name;

            // add chance
            if (chance > 0 && chance < 100)
                text += " (" + I18n.Generic_Percent(chance) + ")";

            return text;
        }

        /// <summary>Get the cartesian product of the possible input ingredients for a recipe.</summary>
        /// <param name="recipe">The recipe whose input sets to list.</param>
        /// <returns>An enumerable containing each set of item ids.</returns>
        private IEnumerable<int[]> GetCartesianInputs(RecipeModel recipe)
        {
            int[][] sets = recipe.Ingredients.Select(p => p.PossibleIds.ToArray()).ToArray();
            return this.GetCartesianProduct(sets);
        }

        /// <summary>Get the cartesian product of an arbitrary number of arrays.</summary>
        /// <typeparam name="T">The array value type.</typeparam>
        /// <param name="arrays">The arrays to combine.</param>
        /// <returns>An enumerable containing each set of item ids.</returns>
        /// <remarks>Derived from <a href="https://stackoverflow.com/a/33106054/262123">code by Peter Almazov</a>.</remarks>
        private IEnumerable<T[]> GetCartesianProduct<T>(IReadOnlyList<T[]> arrays)
        {
            int[] lengths = arrays.Select(a => a.Length).ToArray();
            int length = arrays.Count;
            int[] inds = new int[length];

            while (inds[0] != lengths[0])
            {
                var result = new T[length];
                for (int i = 0; i != length; i++)
                    result[i] = arrays[i][inds[i]];
                yield return result;

                int j = length - 1;
                inds[j]++;
                while (j > 0 && inds[j] == lengths[j])
                {
                    inds[j--] = 0;
                    inds[j]++;
                }
            }
        }

        private IEnumerable<TSource> DistinctBy<TSource, TKey>(IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }
}
