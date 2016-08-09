using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace AccessChestAnywhere
{
    internal class Tab : ClickableComponent
    {
        private readonly Rectangle tabTopLeft = new Rectangle(16, 384, 4, 4);
        private readonly Rectangle tabTopRight = new Rectangle(28, 384, 4, 4);
        private readonly Rectangle tabBottomLeft = new Rectangle(16, 396, 4, 4);
        private readonly Rectangle tabBottomRight = new Rectangle(28, 396, 4, 4);
        private readonly Rectangle tabEdgeTop = new Rectangle(21, 384, 4, 4);
        private readonly Rectangle tabEdgeLeft = new Rectangle(16, 389, 4, 4);
        private readonly Rectangle tabEdgeRight = new Rectangle(28, 389, 4, 4);
        private readonly Rectangle tabEdgeBottom = new Rectangle(21, 396, 4, 4);
        private readonly Rectangle tabBG = new Rectangle(21, 373, 4, 4);
        private readonly SpriteFont font;

        public Tab(string name, int x, int y, bool toRight, SpriteFont font)
            : base(new Rectangle(), name)
        {
            this.font = font;
            Vector2 vector2 = font.MeasureString(name);
            this.bounds.Width = (int)vector2.X + Game1.tileSize / 2;
            this.bounds.Height = (int)vector2.Y + Game1.tileSize / 2;
            this.bounds.X = toRight
                ? x
                : x - this.bounds.Width;
            this.bounds.Y = y;
        }

        public void draw(SpriteBatch b)
        {
            this.drawTab(b);
        }

        private void drawTab(SpriteBatch b)
        {
            int x = this.bounds.X;
            int y = this.bounds.Y;
            int num1 = this.bounds.Width;
            int num2 = this.bounds.Height;
            int num3 = Game1.tileSize;
            b.Draw(Game1.mouseCursors, new Rectangle(x, y, num3 / 4, num3 / 4), this.tabTopLeft, Color.White);
            b.Draw(Game1.mouseCursors, new Rectangle(x + num3 / 4, y, num1 - num3 / 2, num3 / 4), this.tabEdgeTop, Color.White);
            b.Draw(Game1.mouseCursors, new Rectangle(x + num1 - num3 / 4, y, num3 / 4, num3 / 4), this.tabTopRight, Color.White);
            b.Draw(Game1.mouseCursors, new Rectangle(x, y + num3 / 4, num3 / 4, num2 - num3 / 2), this.tabEdgeLeft, Color.White);
            b.Draw(Game1.mouseCursors, new Rectangle(x + num1 - num3 / 4, y + num3 / 4, num3 / 4, num2 - num3 / 2), this.tabEdgeRight, Color.White);
            b.Draw(Game1.mouseCursors, new Rectangle(x, y + num2 - num3 / 4, num3 / 4, num3 / 4), this.tabBottomLeft, Color.White);
            b.Draw(Game1.mouseCursors, new Rectangle(x + num3 / 4, y + num2 - num3 / 4, num1 - num3 / 2, num3 / 4), this.tabEdgeBottom, Color.White);
            b.Draw(Game1.mouseCursors, new Rectangle(x + num1 - num3 / 4, y + num2 - num3 / 4, num3 / 4, num3 / 4), this.tabBottomRight, Color.White);
            b.Draw(Game1.mouseCursors, new Rectangle(x + num3 / 4, y + num3 / 4, num1 - num3 / 2, num2 - num3 / 2), this.tabBG, Color.White);
            b.DrawString(this.font, this.name, new Vector2(x + num3 / 4, y + num3 / 4 + num3 / 16), Color.Black);
        }
    }
}
