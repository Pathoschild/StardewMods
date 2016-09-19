using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.LookupAnything.Components;
using Pathoschild.LookupAnything.Framework.Models;
using StardewValley;

namespace Pathoschild.LookupAnything.Framework.Fields
{
    /// <summary>A metadata field which shows a list of recipes containing an ingredient.</summary>
    internal class RecipesForIngredientField : GenericField
    {
        /*********
        ** Properties
        *********/
        /// <summary>The recipe data to list (recipe => {player knows recipe, number required for recipe}).</summary>
        private readonly IDictionary<RecipeData, Tuple<bool, int>> Recipes;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="label">A short field label.</param>
        /// <param name="itemID">The ingredient's item ID.</param>
        /// <param name="recipes">The recipe to list.</param>
        public RecipesForIngredientField(string label, int itemID, RecipeData[] recipes)
            : base(label, null, hasValue: true)
        {
            this.Recipes = recipes
                .ToDictionary(
                    recipe => recipe,
                    recipe => Tuple.Create(!recipe.MustBeLearned || recipe.KnowsRecipe(Game1.player), recipe.Ingredients[itemID])
                );
        }

        /// <summary>Draw the value (or return <c>null</c> to render the <see cref="GenericField.Value"/> using the default format).</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        /// <param name="font">The recommended font.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="wrapWidth">The maximum width before which content should be wrapped.</param>
        /// <returns>Returns the drawn dimensions, or <c>null</c> to draw the <see cref="GenericField.Value"/> using the default format.</returns>
        public override Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
        {
            float height = 0;

            // get icon size
            float textHeight = font.MeasureString("ABC").Y;
            Vector2 iconSize = new Vector2(textHeight);

            // draw recipes
            foreach (var entry in this.Recipes)
            {
                // get data
                RecipeData recipe = entry.Key;
                bool isKnown = entry.Value.Item1;
                int numberRequired = entry.Value.Item2;
                Item item = recipe.CreateItem();

                // draw icon
                {
                    // get sprite data
                    Tuple<Texture2D, Rectangle> spriteData = GameHelper.GetSprite(item);
                    if (spriteData != null)
                    {
                        Texture2D spriteSheet = spriteData.Item1;
                        Rectangle spriteRectangle = spriteData.Item2;

                        // calculate dimensions
                        float largestDimension = Math.Max(spriteRectangle.Width, spriteRectangle.Height);
                        float scale = iconSize.X / largestDimension;
                        float leftOffset = Math.Max((iconSize.X - (spriteRectangle.Width * scale)) / 2, 0);
                        float topOffset = Math.Max((iconSize.Y - (spriteRectangle.Height * scale)) / 2, 0);

                        // draw
                        Color iconTint = isKnown ? Color.White : Color.White * .5f;
                        spriteBatch.DrawBlock(spriteSheet, spriteRectangle, position.X + leftOffset, position.Y + height + topOffset, iconTint, scale);
                    }
                }

                // draw text
                Color color = isKnown ? Color.Black : Color.Gray;
                Vector2 textSize = spriteBatch.DrawStringBlock(font, $"{recipe.Name} (needs {numberRequired})", position + new Vector2(iconSize.X + 3, height + 5), wrapWidth - iconSize.X, color);

                height += Math.Max(iconSize.Y, textSize.Y) + 5;
            }

            return new Vector2(wrapWidth, height);
        }
    }
}