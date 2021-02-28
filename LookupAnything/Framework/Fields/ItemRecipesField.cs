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
    /// <summary>
    /// A metadata field which shows a list of recipes that
    /// contain an ingredient, or create an output, or are for a given machine.
    /// </summary>
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

        /*********
        ** Public methods
        *********/
        /// <summary>Create an item recipe field that shows recipes with ingredients and the output.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="label">A short field label.</param>
        /// <param name="item">The recipe item.</param>
        /// <param name="recipes">The recipes to list.</param>
        public ItemRecipesField(GameHelper gameHelper, string label, Item item, RecipeModel[] recipes)
            : base(label, true)
        {
            // get recipe data
            RecipeEntry[] availableRecipes = recipes
                .SelectMany(recipe =>
                {
                    Item outputItem = recipe.CreateItem(item);
                    RecipeItemEntry outputEntry = this.CreateItemEntry(
                        gameHelper,
                        outputItem.DisplayName,
                        outputItem,
                        recipe.MinOutput,
                        recipe.MaxOutput,
                        recipe.OutputChance,
                        isOutput: true);

                    return this.GetCartesianInputs(recipe)
                        .Select(inputIds =>
                            {
                                RecipeItemEntry[] inputEntries = inputIds
                                    .Select((inputId, index) =>
                                        this.CreateItemEntry(gameHelper, inputId,
                                            recipe.Ingredients[index]))
                                    .Where(i => i.HasValue)
                                    .Select(i => i.Value)
                                    .OrderBy(i => i.DisplayText)
                                    .ToArray();

                                return new RecipeEntry
                                {
                                    Name = recipe.Key,
                                    IsKnown = !recipe.MustBeLearned || recipe.KnowsRecipe(Game1.player),
                                    Type = recipe.DisplayType,
                                    Output = outputEntry,
                                    Inputs = inputEntries,
                                };
                            }
                        );
                }).ToArray();

            // group to get unique recipes,
            // order the recipes by the recipe type,
            // then the output item name
            this.Recipes = availableRecipes
                .GroupBy(recipe => string.Join(", ", recipe.Inputs
                    .Select(i => i.DisplayText)
                    .OrderBy(i => i)
                    .Concat(new[] { recipe.Output.DisplayText })
                    .Concat(new[] { recipe.Name })
                ))
                .Select(i => i.First())
                .OrderBy(recipe => recipe.Type)
                .ThenBy(recipe => recipe.Output.DisplayText)
                .ToArray();

            // calculate column alignment
            if (!this.Recipes.Any())
                return;

            int recipeCount = this.Recipes.Length;
            int inputCount = this.Recipes.First().Inputs.Length;

            // align recipe items only if all recipes have the same number of inputs
            if (this.Recipes.Any(p => p.Inputs.Length != inputCount))
                return;

            this.AlignColumns = true;

            // include the recipe output as a column
            this.ColumnWidths = new float[inputCount + 1];

            for (int row = 0; row < recipeCount; row++)
            {
                RecipeEntry recipe = this.Recipes[row];

                // include the recipe output as a column by starting at -1
                for (int col = -1; col < inputCount; col++)
                {
                    if (col > -1 && col >= recipe.Inputs.Length)
                        continue;

                    RecipeItemEntry itemEntry = col == -1 ? recipe.Output : recipe.Inputs[col];
                    float width = itemEntry.DisplayTextSize.X;

                    // set the column width
                    if (this.ColumnWidths[col + 1] < width)
                        this.ColumnWidths[col + 1] = width;
                }
            }
        }

        /// <inheritdoc />
        public override Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
        {
            // group type margins
            const int topMarginGroupType = 6;
            const int leftMarginGroupType = 0;

            // recipe item margins
            const int topMarginRecipeFirstLine = 5;
            const int leftMarginRecipeFirstLine = 14;
            const int topMarginRecipeSubsequentLine = 2;
            float horizontalMarginInputDivider = font.MeasureString("+").X;

            // current drawing position
            var currentPosition = new Vector2(position.X, position.Y);
            float absoluteWrapWidth = position.X + wrapWidth;

            // icon size and line height
            float lineHeight = font.MeasureString("ABC").Y;
            var iconSize = new Vector2(lineHeight);
            float joinerWidth = horizontalMarginInputDivider * 3;

            // draw recipes
            string lastGroupType = null;
            foreach (RecipeEntry entry in this.Recipes)
            {
                // change the text the text colour depending on knowledge of the recipe
                Color iconColor = entry.IsKnown ? Color.White : Color.White * .5f;
                Color textColor = entry.IsKnown ? Color.Black : Color.Gray;

                // draw group type
                if (entry.Type != lastGroupType)
                {
                    // move to position for group type
                    currentPosition = new Vector2(
                        position.X + leftMarginGroupType,
                        currentPosition.Y + (lastGroupType == null ? topMarginGroupType : lineHeight));

                    // draw group type text
                    currentPosition = this.DrawCellContent(
                        spriteBatch,
                        font,
                        currentPosition,
                        absoluteWrapWidth,
                        $"{entry.Type}:",
                        Color.Black);

                    // set the last group type
                    lastGroupType = entry.Type;
                }

                // move to position for recipe output
                currentPosition = new Vector2(
                    position.X + leftMarginRecipeFirstLine,
                    currentPosition.Y + lineHeight + topMarginRecipeFirstLine);

                // draw output item icon and text (name + count + chance)
                var currentPositionFromOutput = this.DrawCellContent(
                    spriteBatch,
                    font,
                    currentPosition,
                    absoluteWrapWidth,
                    entry.Output.DisplayText,
                    textColor,
                    entry.Output.Sprite,
                    iconSize,
                    iconColor);

                // calculate the output width
                float outputWidth = this.AlignColumns
                    ? this.ColumnWidths[0] + iconSize.X
                    : currentPositionFromOutput.X - currentPosition.X;

                // move draw position for first input
                currentPosition = new Vector2(
                    (this.AlignColumns ? currentPosition.X + outputWidth : currentPositionFromOutput.X) +
                    horizontalMarginInputDivider,
                    currentPositionFromOutput.Y);

                // draw input items
                for (int i = 0, last = entry.Inputs.Length - 1; i <= last; i++)
                {
                    RecipeItemEntry input = entry.Inputs[i];

                    // move the draw position down to a new line if the next item would be drawn off the right edge
                    float itemWidth = this.AlignColumns
                        ? this.ColumnWidths[i + 1] + iconSize.X
                        : input.DisplayTextSize.X + iconSize.X;

                    if (currentPosition.X + itemWidth > absoluteWrapWidth)
                    {
                        currentPosition = new Vector2(
                            position.X + leftMarginRecipeFirstLine + outputWidth + horizontalMarginInputDivider,
                            currentPosition.Y + lineHeight + topMarginRecipeSubsequentLine);
                    }

                    // draw input item icon and text (name + count)
                    var currentPositionFromInput = this.DrawCellContent(
                        spriteBatch,
                        font,
                        currentPosition,
                        absoluteWrapWidth,
                        input.DisplayText,
                        textColor,
                        input.Sprite,
                        iconSize,
                        iconColor);

                    // move the draw position
                    currentPosition = new Vector2(
                        (this.AlignColumns ? currentPosition.X + itemWidth : currentPositionFromInput.X),
                        currentPositionFromInput.Y);

                    // draw input item joiner
                    if (i == last)
                        continue;

                    // move the draw position down to a new line if the joiner would be drawn off the right edge
                    if (currentPosition.X + joinerWidth > absoluteWrapWidth)
                    {
                        currentPosition = new Vector2(
                            position.X + leftMarginRecipeFirstLine + outputWidth + horizontalMarginInputDivider,
                            currentPosition.Y + lineHeight + topMarginRecipeSubsequentLine);
                    }
                    else
                    {
                        // add the space on the left of the joiner if it is not the first thing on a line
                        currentPosition = new Vector2(
                            currentPosition.X + horizontalMarginInputDivider,
                            currentPosition.Y);
                    }

                    // draw the input item joiner
                    currentPosition = this.DrawCellContent(
                        spriteBatch,
                        font,
                        currentPosition,
                        absoluteWrapWidth,
                        "+",
                        textColor);

                    // move the draw position to make space around the joiner
                    currentPosition = new Vector2(
                        currentPosition.X + horizontalMarginInputDivider,
                        currentPosition.Y);
                }
            }

            // vertical spacer at the bottom of the recipes
            currentPosition = new Vector2(
                currentPosition.X,
                currentPosition.Y + lineHeight + topMarginGroupType);

            // return the drawn dimensions
            return new Vector2(wrapWidth, currentPosition.Y - position.Y);
        }

        /*********
        ** Private methods
        *********/

        /// <summary>Draw text with an optional icon.</summary>
        /// <param name="batch">The sprite batch.</param>
        /// <param name="font">The sprite font.</param>
        /// <param name="position">The position at which to draw the text.</param>
        /// <param name="absoluteWrapWidth">The width at which to wrap the text.</param>
        /// <param name="text">The block of text to write.</param>
        /// <param name="textColor">The text color.</param>
        /// <param name="icon">The sprite to draw.</param>
        /// <param name="iconSize">The size to draw.</param>
        /// <param name="iconColor">The color to tint the sprite.</param>
        /// <returns>Returns the new drawing position.</returns>
        private Vector2 DrawCellContent(SpriteBatch batch, SpriteFont font, Vector2 position, float absoluteWrapWidth,
            string text, Color textColor, SpriteInfo icon = null, Vector2? iconSize = null, Color? iconColor = null)
        {
            if (icon != null && iconSize != null)
            {
                // draw icon
                batch.DrawSpriteWithin(
                    icon,
                    position.X,
                    position.Y,
                    iconSize.Value,
                    iconColor);

                // move draw position
                position = new Vector2(position.X + iconSize.Value.X, position.Y);
            }

            // draw text
            Vector2 textSize = batch.DrawTextBlock(
                font,
                text,
                position,
                absoluteWrapWidth - position.X,
                textColor);

            // move draw position
            position = new Vector2(position.X + textSize.X, position.Y);
            return position;
        }

        /// <summary>Create an item entry from an item id and ingredient model.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="id">The item id.</param>
        /// <param name="ingredient">The recipe ingredient model for the item.</param>
        /// <returns>An item entry model or null.</returns>
        private RecipeItemEntry? CreateItemEntry(GameHelper gameHelper, int id, RecipeIngredientModel ingredient)
        {
            // category
            if (id < 0)
            {
                Item sampleInput = gameHelper.GetObjectsByCategory(id).FirstOrDefault();
                if (sampleInput == null)
                    return null;

                return this.CreateItemEntry(gameHelper, sampleInput.getCategoryName(),
                    minCount: ingredient.Count, maxCount: ingredient.Count);
            }

            // item
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
                    return null;

                return this.CreateItemEntry(gameHelper, input.DisplayName, input,
                    ingredient.Count, ingredient.Count);
            }
        }

        /// <summary>Create an item entry model from the component items of information.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="name">The display name for the item.</param>
        /// <param name="item">The instance of the item.</param>
        /// <param name="minCount">The minimum number of items needed or created.</param>
        /// <param name="maxCount">The maximum number of items needed or created.</param>
        /// <param name="chance">The chance of creating an output item.</param>
        /// <param name="isOutput">Whether the item is output or input.</param>
        /// <returns>An item entry model.</returns>
        private RecipeItemEntry CreateItemEntry(GameHelper gameHelper, string name, Item item = null, int minCount = 1,
            int maxCount = 1, decimal chance = 100, bool isOutput = false)
        {
            string displayText = this.CreateItemText(name, minCount, maxCount, chance);
            if (isOutput)
            {
                displayText += ":";
            }

            return new RecipeItemEntry
            {
                Sprite = item != null ? gameHelper.GetSprite(item) : null,
                DisplayText = displayText,
                DisplayTextSize = Game1.smallFont.MeasureString(displayText)
            };
        }

        /// <summary>Get the display text for an input or output item.</summary>
        /// <param name="name">The item name.</param>
        /// <param name="minCount">The minimum number needed or produced.</param>
        /// <param name="maxCount">The maximum number needed or produced.</param>
        /// <param name="chance">The chance of the item being produced.</param>
        /// <returns>A string representing the item, min and max, and chance.</returns>
        private string CreateItemText(string name, int minCount, int maxCount, decimal chance)
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
    }
}
