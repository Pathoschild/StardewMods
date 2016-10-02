using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace ChestsAnywhere.Components
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
            Vector2 fontSize = font.MeasureString(name);
            this.bounds.Width = (int)fontSize.X + Game1.tileSize / 2;
            this.bounds.Height = (int)fontSize.Y + Game1.tileSize / 2;
            this.bounds.X = toRight
                ? x
                : x - this.bounds.Width;
            this.bounds.Y = y;
        }

        /// <summary>Render the tab UI.</summary>
        /// <param name="sprites">The sprites to render.</param>
        /// <param name="opacity">The opacity at which to draw.</param>
        public void Draw(SpriteBatch sprites, float opacity = 1)
        {
            // calculate sprite dimensions
            int tileSize = Game1.tileSize;
            int edgeSize = tileSize / 4; // the pixel width of an edge
            int padding = tileSize / 2; // the pixel width of the edge padding

            // get bounds
            int x = this.bounds.X;
            int y = this.bounds.Y;
            int width = this.bounds.Width;
            int height = this.bounds.Height;

            // draw sprites
            Color color = Color.White * opacity;
            sprites.Draw(Sprites.Tab.Sheet, Sprites.Tab.TopLeft, x, y, edgeSize, edgeSize, color);
            sprites.Draw(Sprites.Tab.Sheet, Sprites.Tab.Top, x + edgeSize, y, width - padding, edgeSize, color);
            sprites.Draw(Sprites.Tab.Sheet, Sprites.Tab.TopRight, x + width - edgeSize, y, edgeSize, edgeSize, color);
            sprites.Draw(Sprites.Tab.Sheet, Sprites.Tab.Left, x, y + edgeSize, edgeSize, height - padding, color);
            sprites.Draw(Sprites.Tab.Sheet, Sprites.Tab.Right, x + width - edgeSize, y + edgeSize, edgeSize, height - padding, color);
            sprites.Draw(Sprites.Tab.Sheet, Sprites.Tab.BottomLeft, x, y + height - edgeSize, edgeSize, edgeSize, color);
            sprites.Draw(Sprites.Tab.Sheet, Sprites.Tab.Bottom, x + edgeSize, y + height - edgeSize, width - padding, edgeSize, color);
            sprites.Draw(Sprites.Tab.Sheet, Sprites.Tab.BottomRight, x + width - edgeSize, y + height - edgeSize, edgeSize, edgeSize, color);
            sprites.Draw(Sprites.Tab.Sheet, Sprites.Tab.Background, x + edgeSize, y + edgeSize, width - padding, height - padding, color);
            sprites.DrawString(this.Font, this.name, new Vector2(x + edgeSize, y + edgeSize + tileSize / 16), Color.Black * opacity);
        }
    }
}
