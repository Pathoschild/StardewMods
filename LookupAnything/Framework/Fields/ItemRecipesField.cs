using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.LookupAnything.Framework.Fields.Models;
using Pathoschild.Stardew.LookupAnything.Framework.Models;
using StardewValley;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields
{
    /// <summary>A metadata field which shows a list of recipes.</summary>
    internal class ItemRecipesField : GenericField
    {
        /*********
        ** Fields
        *********/
        /// <summary>The recipe data to list (type => recipe => {player knows recipe, number required for recipe}).</summary>
        private readonly RecipeEntry[] Recipes;

        /// <summary>Whether to align input/output columns.</summary>
        private readonly bool AlignColumns;

        /// <summary>The column widths, if <see cref="AlignColumns"/> is true.</summary>
        private readonly float[] ColumnWidths;

        /// <summary>Provides utility methods for interacting with the game code.</summary>
        private readonly GameHelper GameHelper;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="label">A short field label.</param>
        /// <param name="ingredient">The ingredient item.</param>
        /// <param name="recipes">The recipes to list.</param>
        public ItemRecipesField(GameHelper gameHelper, string label, Item ingredient, RecipeModel[] recipes)
            : base(label, true)
        {
            this.GameHelper = gameHelper;

            // build recipe models
            this.Recipes = recipes
                // split into specific recipes that match the item
                // (e.g. a recipe with several possible inputs => several recipes with one possible input)
                .SelectMany(recipe =>
                {
                    Item outputItem = recipe.CreateItem(ingredient);
                    RecipeItemEntry output = this.CreateItemEntry(
                        name: outputItem.DisplayName,
                        item: outputItem,
                        minCount: recipe.MinOutput,
                        maxCount: recipe.MaxOutput,
                        chance: recipe.OutputChance,
                        isOutput: true
                    );

                    return this.GetCartesianInputs(recipe)
                        .Select(inputIds =>
                        {
                            RecipeItemEntry[] inputs = inputIds
                                .Select((inputId, index) => this.TryCreateItemEntry(inputId, recipe.Ingredients[index]))
                                .Where(p => p != null)
                                .OrderBy(entry => entry.DisplayText)
                                .ToArray();

                            return new RecipeEntry(
                                name: recipe.Key,
                                type: recipe.DisplayType,
                                isKnown: !recipe.MustBeLearned || recipe.KnowsRecipe(Game1.player),
                                inputs: inputs,
                                output: output
                            );
                        });
                })

                // group by unique recipe
                // (e.g. two recipe matches => one recipe)
                .GroupBy(recipe => recipe.UniqueKey)
                .Select(item => item.First())

                // sort
                .OrderBy(recipe => recipe.Type)
                .ThenBy(recipe => recipe.Output.DisplayText)
                .ToArray();
            if (!this.Recipes.Any())
                return;

            // align items if all recipes have the same number of inputs
            int inputCount = this.Recipes.First().Inputs.Length;
            this.AlignColumns = this.Recipes.All(p => p.Inputs.Length == inputCount);
            if (this.AlignColumns)
            {
                this.ColumnWidths = new float[inputCount + 1];
                foreach (RecipeEntry recipe in this.Recipes)
                {
                    for (int col = -1; col < inputCount; col++)
                    {
                        RecipeItemEntry itemEntry = col == -1
                            ? recipe.Output
                            : recipe.Inputs[col];

                        float width = itemEntry.DisplayTextSize.X;

                        if (this.ColumnWidths[col + 1] < width)
                            this.ColumnWidths[col + 1] = width;
                    }
                }
            }
        }

        /// <inheritdoc />
        public override Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
        {
            // get margins
            const int groupTopMargin = 6;
            const int groupLeftMargin = 0;
            const int firstRecipeTopMargin = 5;
            const int firstRecipeLeftMargin = 14;
            const int otherRecipeTopMargin = 2;
            float inputDividerWidth = font.MeasureString("+").X;
            float itemSpacer = inputDividerWidth;

            // current drawing position
            Vector2 curPos = position;
            float absoluteWrapWidth = position.X + wrapWidth;

            // icon size and line height
            float lineHeight = font.MeasureString("ABC").Y;
            var iconSize = new Vector2(lineHeight);
            float joinerWidth = inputDividerWidth + (itemSpacer * 2);

            // draw recipes
            string lastGroupType = null;
            foreach (RecipeEntry entry in this.Recipes)
            {
                // fade recipes which aren't known
                Color iconColor = entry.IsKnown ? Color.White : Color.White * .5f;
                Color textColor = entry.IsKnown ? Color.Black : Color.Gray;

                // draw group label
                if (entry.Type != lastGroupType)
                {
                    curPos = new Vector2(
                        x: position.X + groupLeftMargin,
                        y: curPos.Y + (lastGroupType == null ? groupTopMargin : lineHeight)
                    );
                    curPos += this.DrawIconText(spriteBatch, font, curPos, absoluteWrapWidth, $"{entry.Type}:", Color.Black);

                    lastGroupType = entry.Type;
                }

                // reset position for recipe output
                curPos = new Vector2(
                    position.X + firstRecipeLeftMargin,
                    curPos.Y + firstRecipeTopMargin
                );

                // draw output item (icon + name + count + chance)
                float inputLeft;
                {
                    var outputSize = this.DrawIconText(spriteBatch, font, curPos, absoluteWrapWidth, entry.Output.DisplayText, textColor, entry.Output.Sprite, iconSize, iconColor);
                    float outputWidth = Math.Max(outputSize.X, this.AlignColumns ? this.ColumnWidths[0] + iconSize.X : 0);

                    inputLeft = curPos.X + outputWidth + itemSpacer;
                    curPos.X = inputLeft;
                }

                // draw input items
                for (int i = 0, last = entry.Inputs.Length - 1; i <= last; i++)
                {
                    RecipeItemEntry input = entry.Inputs[i];

                    // move the draw position down to a new line if the next item would be drawn off the right edge
                    float itemWidth = this.AlignColumns
                        ? this.ColumnWidths[i + 1] + iconSize.X
                        : input.DisplayTextSize.X + iconSize.X;

                    if (curPos.X + itemWidth > absoluteWrapWidth)
                    {
                        curPos = new Vector2(
                            x: inputLeft,
                            y: curPos.Y + lineHeight + otherRecipeTopMargin
                        );
                    }

                    // draw input item (icon + name + count)
                    Vector2 inputSize = this.DrawIconText(spriteBatch, font, curPos, absoluteWrapWidth, input.DisplayText, textColor, input.Sprite, iconSize, iconColor);
                    curPos = new Vector2(
                        x: curPos.X + itemWidth,
                        y: curPos.Y
                    );

                    // draw input item joiner
                    if (i != last)
                    {
                        // move draw position to next line if needed
                        if (curPos.X + joinerWidth > absoluteWrapWidth)
                        {
                            curPos = new Vector2(
                                x: inputLeft,
                                y: curPos.Y + lineHeight + otherRecipeTopMargin
                            );
                        }
                        else
                            curPos.X += itemSpacer;

                        // draw the input item joiner
                        var joinerSize = this.DrawIconText(spriteBatch, font, curPos, absoluteWrapWidth, "+", textColor);
                        curPos.X += joinerSize.X + itemSpacer;
                    }
                }

                curPos.Y += lineHeight;
            }

            // vertical spacer at the bottom of the recipes
            curPos.Y += groupTopMargin;

            // get drawn dimensions
            return new Vector2(wrapWidth, curPos.Y - position.Y);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Draw text with an icon.</summary>
        /// <param name="batch">The sprite batch.</param>
        /// <param name="font">The sprite font.</param>
        /// <param name="position">The position at which to draw the text.</param>
        /// <param name="absoluteWrapWidth">The width at which to wrap the text.</param>
        /// <param name="text">The block of text to write.</param>
        /// <param name="textColor">The text color.</param>
        /// <param name="icon">The sprite to draw.</param>
        /// <param name="iconSize">The size to draw.</param>
        /// <param name="iconColor">The color to tint the sprite.</param>
        /// <returns>Returns the drawn size.</returns>
        private Vector2 DrawIconText(SpriteBatch batch, SpriteFont font, Vector2 position, float absoluteWrapWidth, string text, Color textColor, SpriteInfo icon = null, Vector2? iconSize = null, Color? iconColor = null)
        {
            // draw icon
            if (icon != null && iconSize.HasValue)
                batch.DrawSpriteWithin(icon, position.X, position.Y, iconSize.Value, iconColor);
            else
                iconSize = Vector2.Zero;

            // draw text
            Vector2 textSize = batch.DrawTextBlock(font, text, position + new Vector2(iconSize.Value.X, 0), absoluteWrapWidth - position.X, textColor);

            // get drawn size
            return new Vector2(
                x: iconSize.Value.X + textSize.X,
                y: Math.Max(iconSize.Value.Y, textSize.Y)
            );
        }

        /// <summary>Create a recipe item model.</summary>
        /// <param name="id">The item id.</param>
        /// <param name="ingredient">The recipe ingredient model for the item.</param>
        /// <returns>The equivalent item entry model, or <c>null</c> for a category with no matching items.</returns>
        private RecipeItemEntry TryCreateItemEntry(int id, RecipeIngredientModel ingredient)
        {
            // from category
            if (id < 0)
            {
                Item input = this.GameHelper.GetObjectsByCategory(id).FirstOrDefault();
                if (input == null)
                    return null;

                return this.CreateItemEntry(
                    name: input.getCategoryName(),
                    minCount: ingredient.Count,
                    maxCount: ingredient.Count
                );
            }

            // from item
            {
                Item input = this.GameHelper.GetObjectBySpriteIndex(id);

                if (input is SObject obj)
                {
                    if (ingredient.PreservedParentSheetIndex != null)
                        obj.preservedParentSheetIndex.Value = ingredient.PreservedParentSheetIndex.Value;
                    if (ingredient.PreserveType != null)
                        obj.preserve.Value = ingredient.PreserveType.Value;
                }

                return this.CreateItemEntry(
                    name: input.DisplayName,
                    item: input,
                    minCount: ingredient.Count,
                    maxCount: ingredient.Count
                );
            }
        }

        /// <summary>Create a recipe item model.</summary>
        /// <param name="name">The display name for the item.</param>
        /// <param name="item">The instance of the item.</param>
        /// <param name="minCount">The minimum number of items needed or created.</param>
        /// <param name="maxCount">The maximum number of items needed or created.</param>
        /// <param name="chance">The chance of creating an output item.</param>
        /// <param name="isOutput">Whether the item is output or input.</param>
        private RecipeItemEntry CreateItemEntry(string name, Item item = null, int minCount = 1, int maxCount = 1, decimal chance = 100, bool isOutput = false)
        {
            // get display text
            string text;
            {
                // name + count
                if (minCount != maxCount)
                    text = I18n.Item_RecipesForMachine_MultipleItems(name: name, count: I18n.Generic_Range(min: minCount, max: maxCount));
                else if (minCount > 1)
                    text = I18n.Item_RecipesForMachine_MultipleItems(name: name, count: minCount);
                else
                    text = name;

                // chance
                if (chance > 0 && chance < 100)
                    text += $" ({I18n.Generic_Percent(chance)})";

                // output suffix
                if (isOutput)
                    text += ":";
            }

            return new RecipeItemEntry(
                sprite: this.GameHelper.GetSprite(item),
                displayText: text,
                displayTextSize: Game1.smallFont.MeasureString(text)
            );
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
    }
}
