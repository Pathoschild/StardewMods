using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace AccessChestAnywhere
{
    class ACATab : ClickableComponent
    {

        private Rectangle tabTopLeft = new Rectangle(16, 384, 4, 4);
        private Rectangle tabTopRight = new Rectangle(28, 384, 4, 4);
        private Rectangle tabBottomLeft = new Rectangle(16, 396, 4, 4);
        private Rectangle tabBottomRight = new Rectangle(28, 396, 4, 4);
        private Rectangle tabEdgeTop = new Rectangle(21, 384, 4, 4);
        private Rectangle tabEdgeLeft = new Rectangle(16, 389, 4, 4);
        private Rectangle tabEdgeRight = new Rectangle(28, 389, 4, 4);
        private Rectangle tabEdgeBottom = new Rectangle(21, 396, 4, 4);
        private Rectangle tabBG = new Rectangle(21, 373, 4, 4);

        private SpriteFont font;
        private bool toRight;
        private int x;
        private int y;
        
        public ACATab(bool toRight, SpriteFont font) 
            : base(Rectangle.Empty, "")
        {
            this.font = font;
            this.toRight = toRight;
        }

        public void setTabPosIndex(int x, int y)
        {
            this.x = x;
            this.y = y;
            resetBounds();
        }

        public void setName(string name)
        {
            this.name = name;
            resetBounds();
        }

        private void resetBounds()
        {
            Vector2 length = font.MeasureString(name);
            bounds.Width = (int)length.X + Game1.tileSize / 2;
            bounds.Height = (int)length.Y + Game1.tileSize / 2;
            if (toRight) bounds.X = x;
            else bounds.X = x - bounds.Width;
            bounds.Y = y;
        }

        public void draw(SpriteBatch b)
        {
            drawTab(b);
        }

        private void drawTab(SpriteBatch b)
        {
            int x = bounds.X;
            int y = bounds.Y;
            int width = bounds.Width;
            int height = bounds.Height;
            int ts = Game1.tileSize;
            b.Draw(Game1.mouseCursors, new Rectangle(x, y, ts / 4, ts / 4), new Rectangle?(tabTopLeft), Color.White);
            b.Draw(Game1.mouseCursors, new Rectangle(x + ts / 4, y, width - ts / 2, ts / 4), new Rectangle?(tabEdgeTop),
                Color.White);
            b.Draw(Game1.mouseCursors, new Rectangle(x + width - ts / 4, y, ts / 4, ts / 4), new Rectangle?(tabTopRight),
                Color.White);
            b.Draw(Game1.mouseCursors, new Rectangle(x, y + ts / 4, ts / 4, height - ts / 2), new Rectangle?(tabEdgeLeft),
                Color.White);
            b.Draw(Game1.mouseCursors, new Rectangle(x + width - ts / 4, y + ts / 4, ts / 4, height - ts / 2),
                new Rectangle?(tabEdgeRight), Color.White);
            b.Draw(Game1.mouseCursors, new Rectangle(x, y + height - ts / 4, ts / 4, ts / 4),
                new Rectangle?(tabBottomLeft), Color.White);
            b.Draw(Game1.mouseCursors, new Rectangle(x + ts / 4, y + height - ts / 4, width - ts / 2, ts / 4),
                new Rectangle?(tabEdgeBottom), Color.White);
            b.Draw(Game1.mouseCursors, new Rectangle(x + width - ts / 4, y + height - ts / 4, ts / 4, ts / 4),
                new Rectangle?(tabBottomRight), Color.White);
            b.Draw(Game1.mouseCursors, new Rectangle(x + ts / 4, y + ts / 4, width - ts / 2, height - ts / 2),
                new Rectangle?(tabBG), Color.White);
            b.DrawString(font, name, new Vector2(x + ts / 4, y + ts / 4 + ts / 16), Color.Black);
        }
    }
}
