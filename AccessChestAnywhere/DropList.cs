using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace AccessChestAnywhere
{
    internal class DropList : ClickableComponent
    {
        private int index;
        private readonly List<string> list;
        private readonly SpriteFont font;
        private readonly bool toRight;
        private readonly Rectangle bgSel = new Rectangle(258, 258, 4, 4);
        private readonly Rectangle bg = new Rectangle(269, 258, 4, 4);
        private readonly Rectangle bgHover = new Rectangle(161, 340, 4, 4);
        private readonly List<ClickableComponent> listComponents = new List<ClickableComponent>();

        public DropList(int index, List<string> list, int x, int y, bool toRight, SpriteFont font)
            : base(new Rectangle(), index.ToString())
        {
            this.index = index;
            this.list = list;
            this.font = font;
            this.toRight = toRight;
            this.bounds.Width = Game1.tileSize * 7;
            this.bounds.Height = (int)font.MeasureString("abcdefghijklmnopqrstuvwxyz").Y * 10 + Game1.tileSize / 16 * 9;
            this.bounds.X = toRight
                ? x
                : x - this.bounds.Width;
            this.bounds.Y = y;

            this.initComponents();
            if (index <= 9)
                return;
            int index1 = 9;
            int num = index;
            for (; index1 >= 0; --index1)
            {
                this.listComponents[index1].name = num.ToString();
                --num;
            }
        }

        public void receiveScrollWheelAction(int direction)
        {
            if (direction > 0)
            {
                if (this.listComponents.Count != 10 || int.Parse(this.listComponents[9].name) <= 9)
                    return;
                foreach (var current in this.listComponents)
                    current.name = (int.Parse(current.name) - 1).ToString();
            }
            else if (this.listComponents.Count == 10 && int.Parse(this.listComponents[9].name) + 1 < this.list.Count)
            {
                foreach (var current in this.listComponents)
                    current.name = (int.Parse(current.name) + 1).ToString();
            }
        }

        public int select(int x, int y)
        {
            foreach (var current in this.listComponents)
            {
                if (current.containsPoint(x, y))
                {
                    this.index = int.Parse(current.name);
                    return this.index;
                }
            }
            return -1;
        }

        private void initComponents()
        {
            int num = 0;
            int x = this.bounds.X;
            int y = this.bounds.Y;
            int width = this.bounds.Width;
            int height = (int)this.font.MeasureString("abcdefghijklmnopqrstuvwxyz").Y;
            for (; num < this.list.Count && num < 10; ++num)
            {
                this.listComponents.Add(new ClickableComponent(new Rectangle(x, y, width, height), num.ToString()));
                y += (int)this.font.MeasureString("abcdefghijklmnopqrstuvwxyz").Y + Game1.tileSize / 16;
            }
        }

        public void draw(SpriteBatch b)
        {
            foreach (var current in this.listComponents)
            {
                if (current.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
                    b.Draw(Game1.mouseCursors, current.bounds, this.bgHover, Color.White);
                else if (current.name.Equals(this.index.ToString()))
                    b.Draw(Game1.mouseCursors, current.bounds, this.bgSel, Color.White);
                else
                    b.Draw(Game1.mouseCursors, current.bounds, this.bg, Color.White);

                if (this.toRight)
                    b.DrawString(this.font, this.list[int.Parse(current.name)], new Vector2(current.bounds.X, current.bounds.Y + Game1.tileSize / 16), Color.Black);
                else
                    b.DrawString(this.font, this.list[int.Parse(current.name)], new Vector2(current.bounds.X + current.bounds.Width - this.font.MeasureString(this.list[int.Parse(current.name)]).X, current.bounds.Y + Game1.tileSize / 16), Color.Black);
            }
        }
    }
}
