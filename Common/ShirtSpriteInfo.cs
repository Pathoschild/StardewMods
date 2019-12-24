using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.LookupAnything;
using StardewValley;
using StardewValley.Objects;

namespace Pathoschild.Stardew.Common
{
    /// <summary>A single clothing sprite in a spritesheet.</summary>
    internal class ShirtSpriteInfo : SpriteInfo
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The area in the spritesheet containing the colored overlay sprite.</summary>
        public Rectangle OverlaySourceRectangle { get; }

        /// <summary>The overlay color.</summary>
        public Color OverlayColor { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="clothing">The clothing item.</param>
        /// <remarks>Derived from <see cref="Clothing.drawInMenu(SpriteBatch, Vector2, float, float, float, StackDrawType, Color, bool)"/>.</remarks>
        public ShirtSpriteInfo(Clothing clothing)
            : base(FarmerRenderer.shirtsTexture, new Rectangle(clothing.indexInTileSheetMale.Value * 8 % 128, clothing.indexInTileSheetMale.Value * 8 / 128 * 32, 8, 8))
        {
            this.OverlaySourceRectangle = new Rectangle(this.SourceRectangle.X + 128, this.SourceRectangle.Y, this.SourceRectangle.Width, this.SourceRectangle.Height);
            this.OverlayColor = clothing.clothesColor.Value;
        }

        /// <summary>Draw the sprite to the screen scaled and centered to fit the given dimensions.</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        /// <param name="x">The X-position at which to draw the sprite.</param>
        /// <param name="y">The X-position at which to draw the sprite.</param>
        /// <param name="size">The size to draw.</param>
        /// <param name="color">The color to tint the sprite.</param>
        public override void Draw(SpriteBatch spriteBatch, int x, int y, Vector2 size, Color? color = null)
        {
            base.Draw(spriteBatch, x, y, size, color);
            spriteBatch.DrawSpriteWithin(this.Spritesheet, this.OverlaySourceRectangle, x, y, size, Utility.MultiplyColor(this.OverlayColor, color ?? Color.White));
        }
    }
}
