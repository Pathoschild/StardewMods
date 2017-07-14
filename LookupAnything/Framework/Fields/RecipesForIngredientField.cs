using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using Pathoschild.Stardew.LookupAnything.Framework.Models;
using StardewModdingAPI;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields
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

        /// <summary>Provides translations stored in the mod folder.</summary>
        private readonly ITranslationHelper Translations;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="label">A short field label.</param>
        /// <param name="ingredient">The ingredient item.</param>
        /// <param name="recipes">The recipe to list.</param>
        /// <param name="translations">Provides translations stored in the mod folder.</param>
        public RecipesForIngredientField(string label, Item ingredient, RecipeModel[] recipes, ITranslationHelper translations)
            : base(label, hasValue: true)
        {
            this.Translations = translations;
            this.Recipes =
                (
                    from recipe in recipes
                    let output = recipe.CreateItem(ingredient)
                    let name = output.DisplayName
                    orderby recipe.DisplayType, name
                    select new Entry
                    {
                        Name = name,
                        Type = recipe.DisplayType,
                        IsKnown = !recipe.MustBeLearned || recipe.KnowsRecipe(Game1.player),
                        NumberRequired = recipe.Ingredients.ContainsKey(ingredient.parentSheetIndex) ? recipe.Ingredients[ingredient.parentSheetIndex] : recipe.Ingredients[ingredient.category],
                        Item = output
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
                Vector2 textSize = spriteBatch.DrawTextBlock(font, this.Translations.Get(L10n.Item.RecipesEntry, new { name = entry.Name, count = entry.NumberRequired }), position + new Vector2(leftIndent + iconSize.X + 3, height + 5), wrapWidth - iconSize.X, color);

                height += Math.Max(iconSize.Y, textSize.Y) + 5;
            }

            return new Vector2(wrapWidth, height);
        }
    }
}
