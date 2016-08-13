using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace AccessChestAnywhere.Components
{
    /// <summary>A tab UI component which lets the player trigger a dropdown list.</summary>
    internal class Tab : ClickableComponent
    {
        /*********
        ** Properties
        *********/
        /// <summary>The font with which to render text.</summary>
        private readonly SpriteFont Font;

        private readonly Rectangle TabTopLeft = new Rectangle(16, 384, 4, 4);
        private readonly Rectangle TabTopRight = new Rectangle(28, 384, 4, 4);
        private readonly Rectangle TabBottomLeft = new Rectangle(16, 396, 4, 4);
        private readonly Rectangle TabBottomRight = new Rectangle(28, 396, 4, 4);
        private readonly Rectangle TabEdgeTop = new Rectangle(21, 384, 4, 4);
        private readonly Rectangle TabEdgeLeft = new Rectangle(16, 389, 4, 4);
        private readonly Rectangle TabEdgeRight = new Rectangle(28, 389, 4, 4);
        private readonly Rectangle TabEdgeBottom = new Rectangle(21, 396, 4, 4);
        private readonly Rectangle TabBackground = new Rectangle(21, 373, 4, 4);


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The displayed tab text.</param>
        /// <param name="x">The X-position from which to render the list.</param>
        /// <param name="y">The Y-position from which to render the list.</param>
        /// <param name="toRight">Whether the tab should be aligned right of the origin.</param>
        /// <param name="font">The font with which to render text.</param>
        public Tab(string name, int x, int y, bool toRight, SpriteFont font)
            : base(new Rectangle(), name)
        {
            this.Font = font;
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
        public void Draw(SpriteBatch sprites)
        {
            int x = this.bounds.X;
            int y = this.bounds.Y;
            int width = this.bounds.Width;
            int height = this.bounds.Height;
            int tileSize = Game1.tileSize;

            sprites.Draw(Game1.mouseCursors, new Rectangle(x, y, tileSize / 4, tileSize / 4), this.TabTopLeft, Color.White);
            sprites.Draw(Game1.mouseCursors, new Rectangle(x + tileSize / 4, y, width - tileSize / 2, tileSize / 4), this.TabEdgeTop, Color.White);
            sprites.Draw(Game1.mouseCursors, new Rectangle(x + width - tileSize / 4, y, tileSize / 4, tileSize / 4), this.TabTopRight, Color.White);
            sprites.Draw(Game1.mouseCursors, new Rectangle(x, y + tileSize / 4, tileSize / 4, height - tileSize / 2), this.TabEdgeLeft, Color.White);
            sprites.Draw(Game1.mouseCursors, new Rectangle(x + width - tileSize / 4, y + tileSize / 4, tileSize / 4, height - tileSize / 2), this.TabEdgeRight, Color.White);
            sprites.Draw(Game1.mouseCursors, new Rectangle(x, y + height - tileSize / 4, tileSize / 4, tileSize / 4), this.TabBottomLeft, Color.White);
            sprites.Draw(Game1.mouseCursors, new Rectangle(x + tileSize / 4, y + height - tileSize / 4, width - tileSize / 2, tileSize / 4), this.TabEdgeBottom, Color.White);
            sprites.Draw(Game1.mouseCursors, new Rectangle(x + width - tileSize / 4, y + height - tileSize / 4, tileSize / 4, tileSize / 4), this.TabBottomRight, Color.White);
            sprites.Draw(Game1.mouseCursors, new Rectangle(x + tileSize / 4, y + tileSize / 4, width - tileSize / 2, height - tileSize / 2), this.TabBackground, Color.White);
            sprites.DrawString(this.Font, this.name, new Vector2(x + tileSize / 4, y + tileSize / 4 + tileSize / 16), Color.Black);
        }
    }
}
