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
        }

        /// <summary>Metadata needed to draw a recipe.</summary>
        private struct RecipeEntry
        {
            /// <summary>The recipe name or key.</summary>
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

        /// <summary>The indent from the left when drawing the lookup UI.</summary>
        private readonly int LeftIndent = 16;

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
            List<RecipeEntry> availableRecipes = recipes
                .SelectMany(recipe =>
                {
                    Item outputItem = recipe.CreateItem(item);
                    ItemEntry outputEntry = this.ItemEntryFromItem(gameHelper, outputItem.DisplayName, outputItem,
                        recipe.MinOutput, recipe.MaxOutput, recipe.OutputChance);

                    return this.GetCartesianInputs(recipe)
                        .Select(inputIds =>
                            {
                                var inputEntries = inputIds
                                    .Select((inputId, index) =>
                                        this.ItemEntryFromIdAndIngredient(gameHelper, inputId,
                                            recipe.Ingredients[index])
                                    ).Where(i => i.HasValue)
                                    .Select(i => i.Value);

                                return new RecipeEntry
                                {
                                    Name = recipe.Key,
                                    IsKnown = !recipe.MustBeLearned || recipe.KnowsRecipe(Game1.player),
                                    Type = recipe.DisplayType,
                                    Output = outputEntry,
                                    Inputs = inputEntries.ToArray(),
                                };
                            }
                        );
                }).ToList();

            // group to get unique recipes,
            // order the recipes by the recipe type,
            // then the output item name
            this.Recipes = availableRecipes
                .GroupBy(recipe => string.Join(", ", recipe.Inputs
                    .Select(i => i.DisplayText)
                    .OrderBy(i => i)
                    .Concat(new[] {recipe.Output.DisplayText})
                    .Concat(new[] {recipe.Name})
                ))
                .Select(i => i.First())
                .OrderBy(recipe => recipe.Type)
                .ThenBy(recipe => recipe.Output.DisplayText)
                .ToArray();
        }

        /// <inheritdoc />
        public override Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
        {
            // The margin between lines of text
            const int verticalMargin = 5;

            var originalPosition = position;
            var currentPosition = new Vector2(position.X, position.Y);

            // get icon size
            float textHeight = font.MeasureString("ABC").Y;
            float spaceWidth = font.MeasureString("I").X;
            Vector2 iconSize = new Vector2(textHeight);

            // draw recipes
            string lastType = null;
            foreach (RecipeEntry entry in this.Recipes)
            {
                Color iconColor = entry.IsKnown ? Color.White : Color.White * .5f;
                Color textColor = entry.IsKnown ? Color.Black : Color.Gray;
                float lineHeight = iconSize.Y + verticalMargin;

                // draw type for group
                if (entry.Type != lastType)
                {
                    currentPosition = new Vector2(originalPosition.X, currentPosition.Y + verticalMargin);
                    Vector2 textSize = spriteBatch.DrawTextBlock(
                        font,
                        $"{entry.Type}:",
                        currentPosition,
                        wrapWidth,
                        Color.Black);
                    currentPosition = new Vector2(currentPosition.X, currentPosition.Y + textSize.Y);

                    lastType = entry.Type;
                }

                // draw output item
                {
                    currentPosition = new Vector2(originalPosition.X + this.LeftIndent, currentPosition.Y);

                    // icon
                    if (entry.Output.Sprite != null)
                    {
                        spriteBatch.DrawSpriteWithin(
                            entry.Output.Sprite,
                            currentPosition.X,
                            currentPosition.Y,
                            iconSize,
                            iconColor);
                        currentPosition = new Vector2(currentPosition.X + iconSize.X, currentPosition.Y);
                    }

                    // name + count + chance
                    Vector2 textSize = spriteBatch.DrawTextBlock(
                        font,
                        entry.Output.DisplayText,
                        currentPosition,
                        wrapWidth,
                        textColor);
                    currentPosition = new Vector2(currentPosition.X + textSize.X, currentPosition.Y);

                    // joiner
                    Vector2 joinerSize = spriteBatch.DrawTextBlock(
                        font,
                        ":",
                        currentPosition,
                        wrapWidth,
                        textColor);
                    currentPosition = new Vector2(currentPosition.X + joinerSize.X + spaceWidth, currentPosition.Y);

                    // choose highest line height
                    lineHeight = new[] {lineHeight, iconSize.Y, textSize.Y, joinerSize.Y}.Max();
                }

                // draw input items
                for (int i = 0, last = entry.Inputs.Length - 1; i <= last; i++)
                {
                    ItemEntry input = entry.Inputs[i];

                    currentPosition = this.WrapLineIfNeeded(spriteBatch, font, input.DisplayText,
                        originalPosition, currentPosition, lineHeight, wrapWidth, iconSize);

                    // icon
                    if (input.Sprite != null)
                    {
                        spriteBatch.DrawSpriteWithin(
                            input.Sprite,
                            currentPosition.X,
                            currentPosition.Y,
                            iconSize,
                            iconColor);
                        currentPosition = new Vector2(currentPosition.X + iconSize.X, currentPosition.Y);
                    }

                    // display text
                    Vector2 textSize = spriteBatch.DrawTextBlock(
                        font,
                        input.DisplayText,
                        currentPosition,
                        wrapWidth,
                        textColor);
                    currentPosition = new Vector2(currentPosition.X + textSize.X, currentPosition.Y);

                    // joiner
                    if (i == last)
                        continue;

                    currentPosition = new Vector2(currentPosition.X + spaceWidth, currentPosition.Y);

                    currentPosition = this.WrapLineIfNeeded(spriteBatch, font, "+",
                        originalPosition, currentPosition, lineHeight, wrapWidth);

                    Vector2 joinerSize = spriteBatch.DrawTextBlock(
                        font,
                        "+",
                        currentPosition,
                        wrapWidth,
                        textColor);
                    currentPosition = new Vector2(currentPosition.X + joinerSize.X + (2 * spaceWidth),
                        currentPosition.Y);

                    // choose highest line height
                    lineHeight = new[] {lineHeight, iconSize.Y, textSize.Y, joinerSize.Y}.Max();
                }

                currentPosition = new Vector2(currentPosition.X, currentPosition.Y + lineHeight);
            }

            return new Vector2(wrapWidth, currentPosition.Y - originalPosition.Y);
        }

        /*********
        ** Private methods
        *********/
        private Vector2 WrapLineIfNeeded(SpriteBatch batch, SpriteFont font, string text, Vector2 originalPosition,
            Vector2 position, float lineHeight, float wrapWidth, Vector2? iconSize = null)
        {
            var estimatedText = font.MeasureString(text);
            float iconWidth = iconSize?.X ?? 0;
            float iconHeight = iconSize?.Y ?? 0;
            float indentFromOriginal = position.X - originalPosition.X;

            if ((indentFromOriginal + iconWidth + estimatedText.X) > wrapWidth)
            {
                // if the icon width plus text width will go beyond the wrap width
                // move down to the next line, with 2x indents
                lineHeight = new[] {lineHeight, iconHeight, estimatedText.Y}.Max();
                position = new Vector2(originalPosition.X + (this.LeftIndent * 3), position.Y + lineHeight);
            }

            return position;
        }



        /// <summary>Create an item entry from an item id and ingredient model.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="id">The item id.</param>
        /// <param name="ingredient">The recipe ingredient model for the item.</param>
        /// <returns>An item entry model or null.</returns>
        private ItemEntry? ItemEntryFromIdAndIngredient(GameHelper gameHelper, int id, RecipeIngredientModel ingredient)
        {
            // category
            if (id < 0)
            {
                Item sampleInput = gameHelper.GetObjectsByCategory(id).FirstOrDefault();
                if (sampleInput == null)
                    return null;

                return this.ItemEntryFromItem(gameHelper, sampleInput.getCategoryName(),
                    min: ingredient.Count, max: ingredient.Count);
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

                return this.ItemEntryFromItem(gameHelper, input.DisplayName, input,
                    ingredient.Count, ingredient.Count);
            }
        }

        /// <summary>Create an item entry model from the component items of information.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="name">The display name for the item.</param>
        /// <param name="item">The instance of the item.</param>
        /// <param name="min">The minimum number of items needed or created.</param>
        /// <param name="max">The maximum number of items needed or created.</param>
        /// <param name="chance">The chance of creating an output item.</param>
        /// <returns>An item entry model.</returns>
        private ItemEntry ItemEntryFromItem(GameHelper gameHelper, string name, Item item = null, int min = 1,
            int max = 1, decimal chance = 100)
        {
            string displayText = this.GetRecipeItemDisplayText(name, min, max, chance);
            return new ItemEntry
            {
                Sprite = item != null ? gameHelper.GetSprite(item) : null,
                DisplayText = displayText
            };
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
    }
}
