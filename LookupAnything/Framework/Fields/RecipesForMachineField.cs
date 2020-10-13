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
    /// <summary>A metadata field which shows a list of recipes for a given machine.</summary>
    internal class RecipesForMachineField : GenericField
    {
        /*********
        ** Fields
        *********/
        /// <summary>Metadata for a drawn input or output item.</summary>
        private struct EntryItem
        {
            /// <summary>The sprite to display.</summary>
            public SpriteInfo Sprite;

            /// <summary>The display text for the item name and count.</summary>
            public string DisplayText;

            /// <summary>The pixel size of the display text.</summary>
            public Vector2 DisplayTextSize;
        }

        /// <summary>Metadata needed to draw a recipe.</summary>
        private struct Entry
        {
            /// <summary>Whether the player knows the recipe.</summary>
            public bool IsKnown;

            /// <summary>The input items.</summary>
            public EntryItem[] Inputs;

            /// <summary>The output items.</summary>
            public EntryItem Output;
        }

        /// <summary>The recipe data to list (type => recipe => {player knows recipe, number required for recipe}).</summary>
        private readonly Entry[] Recipes;

        /// <summary>Whether to align input/output columns.</summary>
        private readonly bool AlignColumns;

        /// <summary>The input column widths, if <see cref="AlignColumns"/> is true.</summary>
        private readonly int[] InputColumnWidths;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="label">A short field label.</param>
        /// <param name="recipes">The recipe to list.</param>
        public RecipesForMachineField(GameHelper gameHelper, string label, RecipeModel[] recipes)
            : base(label, hasValue: true)
        {
            // get recipe data
            this.Recipes = this
                .GetRecipeEntries(gameHelper, recipes)
                .OrderBy(entry => string.Join(", ", entry.Inputs.SelectMany(input => input.DisplayText)))
                .ThenBy(entry => entry.Output.DisplayText)
                .ToArray();

            // calculate column alignment
            if (this.Recipes.Any())
            {
                var firstRecipe = this.Recipes.First();
                int inputCount = firstRecipe.Inputs.Length;
                if (this.Recipes.All(p => p.Inputs.Length == inputCount))
                {
                    this.AlignColumns = true;
                    this.InputColumnWidths = firstRecipe.Inputs.Select(p => (int)p.DisplayTextSize.X).ToArray();

                    foreach (var recipe in this.Recipes.Skip(1))
                    {
                        for (int i = 0; i < recipe.Inputs.Length; i++)
                        {
                            int inputWidth = (int)recipe.Inputs[i].DisplayTextSize.X;
                            if (inputWidth > this.InputColumnWidths[i])
                                this.InputColumnWidths[i] = inputWidth;
                        }
                    }
                }
            }
        }

        /// <summary>Draw the value (or return <c>null</c> to render the <see cref="GenericField.Value"/> using the default format).</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        /// <param name="font">The recommended font.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="wrapWidth">The maximum width before which content should be wrapped.</param>
        /// <returns>Returns the drawn dimensions, or <c>null</c> to draw the <see cref="GenericField.Value"/> using the default format.</returns>
        public override Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
        {
            const float leftIndent = 16;

            // get icon size
            float textHeight = font.MeasureString("ABC").Y;
            float spaceWidth = font.MeasureString("W").X;
            Vector2 iconSize = new Vector2(textHeight);

            // draw recipes
            float topOffset = 0;
            foreach (Entry entry in this.Recipes)
            {
                float leftOffset = leftIndent;
                Color iconColor = entry.IsKnown ? Color.White : Color.White * .5f;
                Color textColor = entry.IsKnown ? Color.Black : Color.Gray;
                float lineHeight = iconSize.Y + 5;

                // draw inputs
                for (int i = 0, last = entry.Inputs.Length - 1; i <= last; i++)
                {
                    EntryItem input = entry.Inputs[i];

                    // icon
                    if (input.Sprite != null)
                        spriteBatch.DrawSpriteWithin(input.Sprite, position.X + leftOffset, position.Y + topOffset, iconSize, iconColor);
                    leftOffset += iconSize.X;

                    // name + count
                    Vector2 textSize = spriteBatch.DrawTextBlock(font, input.DisplayText, position + new Vector2(leftOffset, topOffset + 5), wrapWidth - leftOffset, textColor);
                    leftOffset += this.AlignColumns ? this.InputColumnWidths[i] : textSize.X;
                    lineHeight = Math.Max(lineHeight, textSize.Y);

                    // joiner
                    if (i != last)
                    {
                        Vector2 joinerSize = spriteBatch.DrawTextBlock(font, "+", position + new Vector2(leftOffset + spaceWidth, topOffset + 5), wrapWidth - leftOffset, textColor);
                        leftOffset += joinerSize.X + (2 * spaceWidth);
                        lineHeight = Math.Max(lineHeight, joinerSize.Y);
                    }
                }

                // draw output
                {
                    // joiner
                    Vector2 joinerSize = spriteBatch.DrawTextBlock(font, ">", position + new Vector2(leftOffset + spaceWidth, topOffset + 5), wrapWidth - leftOffset, textColor);
                    leftOffset += joinerSize.X + (2 * spaceWidth);
                    lineHeight = Math.Max(lineHeight, joinerSize.Y);

                    // icon
                    if (entry.Output.Sprite != null)
                        spriteBatch.DrawSpriteWithin(entry.Output.Sprite, position.X + leftOffset, position.Y + topOffset, iconSize, iconColor);
                    leftOffset += iconSize.X;

                    // name + count + chance
                    Vector2 textSize = spriteBatch.DrawTextBlock(font, entry.Output.DisplayText, position + new Vector2(leftOffset, topOffset + 5), wrapWidth - leftOffset, textColor);
                    lineHeight = Math.Max(lineHeight, textSize.Y);
                }

                topOffset += lineHeight;
            }

            return new Vector2(wrapWidth, topOffset);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the recipe entries.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="recipes">The recipe to list.</param>
        private IEnumerable<Entry> GetRecipeEntries(GameHelper gameHelper, IEnumerable<RecipeModel> recipes)
        {
            foreach (RecipeModel recipe in recipes)
            {
                // get inputs
                List<EntryItem> inputs = new List<EntryItem>();
                foreach (var ingredient in recipe.Ingredients)
                {
                    int count = ingredient.Count;

                    // category
                    if (ingredient.ID < 0)
                    {
                        Item sampleInput = gameHelper.GetObjectsByCategory(ingredient.ID).FirstOrDefault();
                        if (sampleInput == null)
                            continue;

                        string displayText = this.GetItemDisplayText(name: sampleInput.getCategoryName(), minCount: count, maxCount: count, chance: 100);
                        inputs.Add(new EntryItem
                        {
                            Sprite = null,
                            DisplayText = displayText,
                            DisplayTextSize = Game1.smallFont.MeasureString(displayText)
                        });
                    }

                    // item
                    else
                    {
                        Item input = gameHelper.GetObjectBySpriteIndex(ingredient.ID);
                        if (input is SObject obj)
                        {
                            if (ingredient.PreservedParentSheetIndex != null)
                                obj.preservedParentSheetIndex.Value = ingredient.PreservedParentSheetIndex.Value;
                            if (ingredient.PreserveType != null)
                                obj.preserve.Value = ingredient.PreserveType.Value;
                        }

                        string displayText = this.GetItemDisplayText(name: input.DisplayName, minCount: count, maxCount: count, chance: 100);
                        inputs.Add(new EntryItem
                        {
                            Sprite = gameHelper.GetSprite(input),
                            DisplayText = displayText,
                            DisplayTextSize = Game1.smallFont.MeasureString(displayText)
                        });
                    }
                }

                // get output
                EntryItem output;
                {
                    Item outputItem = recipe.CreateItem(null);
                    string displayText = this.GetItemDisplayText(name: outputItem.DisplayName, minCount: recipe.MinOutput, maxCount: recipe.MaxOutput, chance: recipe.OutputChance);
                    output = new EntryItem
                    {
                        Sprite = gameHelper.GetSprite(outputItem),
                        DisplayText = displayText,
                        DisplayTextSize = Game1.smallFont.MeasureString(displayText)
                    };
                }

                yield return new Entry
                {
                    IsKnown = !recipe.MustBeLearned || recipe.KnowsRecipe(Game1.player),
                    Inputs = inputs.ToArray(),
                    Output = output
                };
            }
        }

        /// <summary>Get the display text for an input or output item.</summary>
        /// <param name="name">The item name.</param>
        /// <param name="minCount">The minimum number needed or produced.</param>
        /// <param name="maxCount">The maximum number needed or produced.</param>
        /// <param name="chance">The chance of the item being produced.</param>
        private string GetItemDisplayText(string name, int minCount, int maxCount, decimal chance)
        {
            // get name + count
            string text;
            if (minCount != maxCount)
                text = I18n.Item_RecipesForMachine_MultipleItems(name: name, count: I18n.Generic_Range(min: minCount, max: maxCount));
            else if (minCount > 1)
                text = I18n.Item_RecipesForMachine_MultipleItems(name: name, count: minCount);
            else
                text = name;

            // add chance
            if (chance > 0 && chance < 100)
                text += " (" + I18n.Generic_Percent(chance) + ")";

            return text;
        }
    }
}
