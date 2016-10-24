using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.ChestsAnywhere.Menus.Components
{
    /// <summary>A tab UI component which lets the player trigger a dropdown list.</summary>
    internal class Tab : ClickableComponent
    {
        /*********
        ** Properties
        *********/
        /// <summary>The font with which to render text.</summary>
        private readonly SpriteFont Font;

        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The displayed tab text.</param>
        /// <param name="x">The X-position at which to draw the tab.</param>
        /// <param name="y">The Y-position at which to draw the tab.</param>
        /// <param name="toRight">Whether the tab should be aligned right of the origin.</param>
        /// <param name="font">The font with which to render text.</param>
        public Tab(string name, int x, int y, bool toRight, SpriteFont font)
            : base(Rectangle.Empty, name)
        {
            // save values
            this.Font = font;

            // set bounds
            Vector2 size = Tab.GetTabSize(font, name);
            this.bounds.Width = (int)size.X;
            this.bounds.Height = (int)size.Y;
            this.bounds.X = x;
            if (!toRight)
                this.bounds.X -= this.bounds.Width;
            this.bounds.Y = y;
        }

        /// <summary>Render the tab UI.</summary>
        /// <param name="sprites">The sprites to render.</param>
        /// <param name="opacity">The opacity at which to draw.</param>
        public void Draw(SpriteBatch sprites, float opacity = 1)
        {
            // calculate
            int x = this.bounds.X;
            int y = this.bounds.Y;
            int width = this.bounds.Width;
            int height = this.bounds.Height;
            int zoom = Game1.pixelZoom;
            Color color = Color.White * opacity;
            int borderWidth = Sprites.Tab.Left.Width * zoom;
            int cornerWidth = Sprites.Tab.TopLeft.Width * zoom;

            // draw
            var sheet = Sprites.Tab.Sheet;
            sprites.Draw(sheet, Sprites.Tab.Background, x + borderWidth, y + borderWidth, width - borderWidth * 2, height - borderWidth * 2, color);
            sprites.Draw(sheet, Sprites.Tab.Top, x + cornerWidth, y, width - borderWidth * 2, borderWidth, color);
            sprites.Draw(sheet, Sprites.Tab.Left, x, y + cornerWidth, borderWidth, height - borderWidth * 2, color);
            sprites.Draw(sheet, Sprites.Tab.Right, x + width - borderWidth, y + cornerWidth, borderWidth, height - cornerWidth * 2, color);
            sprites.Draw(sheet, Sprites.Tab.Bottom, x + cornerWidth, y + height - borderWidth, width - borderWidth * 2, borderWidth, color);
            sprites.Draw(sheet, Sprites.Tab.TopLeft, x, y, cornerWidth, cornerWidth, color);
            sprites.Draw(sheet, Sprites.Tab.TopRight, x + width - cornerWidth, y, cornerWidth, cornerWidth, color);
            sprites.Draw(sheet, Sprites.Tab.BottomLeft, x, y + height - cornerWidth, cornerWidth, cornerWidth, color);
            sprites.Draw(sheet, Sprites.Tab.BottomRight, x + width - cornerWidth, y + height - cornerWidth, cornerWidth, cornerWidth, color);
            sprites.DrawString(this.Font, this.name, new Vector2(x + cornerWidth, y + cornerWidth), Color.Black * opacity);
        }

        /// <summary>Get the size of the tab rendered for a given label.</summary>
        /// <param name="font">The font with which to render text.</param>
        /// <param name="name">The displayed tab text.</param>
        public static Vector2 GetTabSize(SpriteFont font, string name)
        {
            return
                font.MeasureString(name) // get font size
                - new Vector2(0, 10) // adjust for font's broken measurement
                + new Vector2(Sprites.Tab.TopLeft.Width * 2 * Game1.pixelZoom); // add space for borders
        }
    }
}
