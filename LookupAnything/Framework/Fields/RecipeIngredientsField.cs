using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.LookupAnything.Components;
using StardewValley;

namespace Pathoschild.LookupAnything.Framework.Fields
{
    /// <summary>A metadata field which shows recipe ingredients (if the player learned them).</summary>
    internal class RecipeIngredientsField : GenericField
    {
        /*********
        ** Properties
        *********/
        /// <summary>The crafting recipe.</summary>
        private readonly CraftingRecipe Recipe;

        /// <summary>Whether the player knows the recipe.</summary>
        private readonly bool KnowsRecipe;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="label">A short field label.</param>
        /// <param name="recipe">The crafting recipe.</param>
        public RecipeIngredientsField(string label, CraftingRecipe recipe)
            : base(label, null, hasValue: true)
        {
            this.Recipe = recipe;
            this.KnowsRecipe = Game1.player.knowsRecipe(recipe.name);
        }

        /// <summary>Draw the value (or return <c>null</c> to render the <see cref="GenericField.Value"/> using the default format).</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        /// <param name="font">The recommended font.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="wrapWidth">The maximum width before which content should be wrapped.</param>
        /// <returns>Returns the drawn dimensions, or <c>null</c> to draw the <see cref="GenericField.Value"/> using the default format.</returns>
        public override Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
        {
            // unknown recipe
            if (!this.KnowsRecipe)
                return spriteBatch.DrawStringBlock(font, "You haven't learned this recipe.", new Vector2(position.X, position.Y), wrapWidth, Color.Gray);

            // get basic info
            CraftingRecipe recipe = this.Recipe;
            bool isCookingRecipe = recipe.isCookingRecipe;
            float height = 0;

            // recipe summary
            {
                string text = recipe.timesCrafted == 0
                    ? (isCookingRecipe ? "Never cooked." : "Never crafted.")
                    : $"{(isCookingRecipe ? "Cooked" : "Crafted")} {GameHelper.Pluralise(recipe.timesCrafted, "once", $"{recipe.timesCrafted} times")}.";
                height += spriteBatch.DrawStringBlock(font, text, position, wrapWidth).Y;
            }

            // ingredients
            {
                Dictionary<int, int> ingredients = GameHelper.GetPrivateField<Dictionary<int, int>>(recipe, "recipeList");
                foreach (var ingredient in ingredients)
                {
                    // get item
                    int itemID = ingredient.Key;
                    int stack = ingredient.Value;
                    Object item = GameHelper.GetObjectBySpriteIndex(itemID, stack);

                    // draw item
                    float textHeight = font.MeasureString(item.Name).Y;
                    spriteBatch.DrawBlock(Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, itemID, 16, 16), position.X, position.Y + height, scale: textHeight / 16);
                    height += spriteBatch.DrawStringBlock(font, $"{item.name} ({stack})", new Vector2(position.X + textHeight + 3, position.Y + height), wrapWidth).Y;
                }
            }

            return new Vector2(wrapWidth, height);
        }
    }
}