using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.LookupAnything;

namespace Pathoschild.Stardew.Common
{
    /// <summary>A single sprite in a spritesheet.</summary>
    internal class SpriteInfo
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The spritesheet texture.</summary>
        public Texture2D Spritesheet { get; }

        /// <summary>The area in the spritesheet containing the primary sprite. This is drawn to represent the item, and used when looking up the item in the world.</summary>
        public Rectangle SourceRectangle { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="spritesheet">The spritesheet texture.</param>
        /// <param name="sourceRectangle">The area in the spritesheet containing the sprite.</param>
        public SpriteInfo(Texture2D spritesheet, Rectangle sourceRectangle)
        {
            this.Spritesheet = spritesheet;
            this.SourceRectangle = sourceRectangle;
        }

        /// <summary>Draw the sprite to the screen scaled and centered to fit the given dimensions.</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        /// <param name="x">The X-position at which to draw the sprite.</param>
        /// <param name="y">The X-position at which to draw the sprite.</param>
        /// <param name="size">The size to draw.</param>
        /// <param name="color">The color to tint the sprite.</param>
        public virtual void Draw(SpriteBatch spriteBatch, int x, int y, Vector2 size, Color? color = null)
        {
            spriteBatch.DrawSpriteWithin(this.Spritesheet, this.SourceRectangle, x, y, size, color ?? Color.White);
        }
    }
}
