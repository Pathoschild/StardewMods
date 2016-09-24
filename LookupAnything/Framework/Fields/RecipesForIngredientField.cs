using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        /// <summary>Metadata needed to draw a recipe.</summary>
        private struct Entry
        {
            /// <summary>The recipe name.</summary>
            public string Name;

            /// <summary>The recipe type.</summary>
            public string Type;

            /// <summary>Whether the player knows the recipe.</summary>
            public bool IsKnown;

            /// <summary>The number of the item required for the recipe.</summary>
            public int NumberRequired;

            /// <summary>The resulting item.</summary>
            public Item Item;
        }

        /// <summary>The recipe data to list (type => recipe => {player knows recipe, number required for recipe}).</summary>
        private readonly Entry[] Recipes;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="label">A short field label.</param>
        /// <param name="item">The ingredient item.</param>
        /// <param name="recipes">The recipe to list.</param>
        public RecipesForIngredientField(string label, Item item, RecipeModel[] recipes)
            : base(label, null, hasValue: true)
        {
            this.Recipes =
                (
                    from recipe in recipes
                    orderby recipe.Type.ToString(), recipe.Name
                    select new Entry
                    {
                        Name = recipe.Name.Replace("$ingredient", item.Name),
                        Type = Regex.Replace(recipe.Type.ToString(), @"(\B[A-Z])", " $1"), // e.g. "OilMaker" => "Oil Maker"
                        IsKnown = !recipe.MustBeLearned || recipe.KnowsRecipe(Game1.player),
                        NumberRequired = recipe.Ingredients.ContainsKey(item.parentSheetIndex) ? recipe.Ingredients[item.parentSheetIndex] : recipe.Ingredients[item.category],
                        Item = recipe.CreateItem()
                    }
                )
                .ToArray();
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
            float height = 0;

            // get icon size
            float textHeight = font.MeasureString("ABC").Y;
            Vector2 iconSize = new Vector2(textHeight);

            // draw recipes
            string lastType = null;
            foreach (Entry entry in this.Recipes)
            {
                // draw type
                if (entry.Type != lastType)
                {
                    height += spriteBatch.DrawTextBlock(font, $"{entry.Type}:", position + new Vector2(0, height), wrapWidth).Y;
                    lastType = entry.Type;
                }

                // draw icon
                Color iconColor = entry.IsKnown ? Color.White : Color.White * .5f;
                spriteBatch.DrawIcon(entry.Item, position.X + leftIndent, position.Y + height, iconSize, iconColor);

                // draw text
                Color color = entry.IsKnown ? Color.Black : Color.Gray;
                Vector2 textSize = spriteBatch.DrawTextBlock(font, $"{entry.Name} (needs {entry.NumberRequired})", position + new Vector2(leftIndent + iconSize.X + 3, height + 5), wrapWidth - iconSize.X, color);

                height += Math.Max(iconSize.Y, textSize.Y) + 5;
            }

            return new Vector2(wrapWidth, height);
        }
    }
}