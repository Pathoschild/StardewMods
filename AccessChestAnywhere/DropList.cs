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
    class DropList : ClickableComponent
    {
        private int index;
        private List<string> list;
        private SpriteFont font;
        private bool toRight;

        private Rectangle bgSel = new Rectangle(258,258,4,4);
        private Rectangle bg = new Rectangle(269,258,4,4);
        private Rectangle bgHover = new Rectangle(161,340,4,4);

        private List<ClickableComponent> listComponents = new List<ClickableComponent>();

        public DropList(int index, List<string> list, int x, int y, bool toRight, SpriteFont font)
            : base(new Rectangle(), index.ToString())
        {
            this.index = index;
            this.list = list;
            this.font = font;
            this.toRight = toRight;

            this.bounds.Width = Game1.tileSize*7;
            this.bounds.Height = (int) font.MeasureString("abcdefghijklmnopqrstuvwxyz").Y*10 + Game1.tileSize/16*9;
            if (toRight) this.bounds.X = x;
            else this.bounds.X = x - this.bounds.Width;
            this.bounds.Y = y;

            initComponents();
            if (index > 9)
            {
                int i = 9;
                int ind = index;
                while (i>=0)
                {
                    listComponents[i].name = ind.ToString();
                    ind--;
                    i--;
                }
            }
        }

        public void receiveScrollWheelAction(int direction)
        {
            if (direction > 0)
            {
                if (listComponents.Count == 10)
                {
                    int ind = int.Parse(listComponents[9].name);
                    if(ind>9)
                        foreach (ClickableComponent component in listComponents)
                        {
                            component.name = (int.Parse(component.name)-1).ToString();
                        }
                }
            }
            else
            {
                if (listComponents.Count == 10)
                {
                    int ind = int.Parse(listComponents[9].name);
                    if ((ind+1)<list.Count)
                        foreach (ClickableComponent component in listComponents)
                        {
                            component.name = (int.Parse(component.name) + 1).ToString();
                        }
                }
            }
        }

        public int select(int x, int y)
        {
            foreach (ClickableComponent component in listComponents)
            {
                if (component.containsPoint(x, y))
                {
                    index = int.Parse(component.name);
                    return index;
                }
            }
            return -1;
        }

        private void initComponents()
        {
            int count = 0;
            int x = this.bounds.X;
            int y = this.bounds.Y;
            int width = this.bounds.Width;
            int height = (int) font.MeasureString("abcdefghijklmnopqrstuvwxyz").Y;
            while (count<list.Count&&count<10)
            {
                Rectangle rect = new Rectangle(x,y,width,height);
                listComponents.Add(new ClickableComponent(rect,count.ToString()));
                y += (int) font.MeasureString("abcdefghijklmnopqrstuvwxyz").Y + Game1.tileSize/16;
                count++;
            }
        }

        public void draw(SpriteBatch b)
        {
            foreach (ClickableComponent component in listComponents)
            {
                if (component.containsPoint(Game1.getMouseX(), Game1.getMouseY())) b.Draw(Game1.mouseCursors,component.bounds,new Rectangle?(bgHover),Color.White );
                else if (component.name.Equals(index.ToString())) b.Draw(Game1.mouseCursors, component.bounds, new Rectangle?(bgSel), Color.White);
                else b.Draw(Game1.mouseCursors, component.bounds, new Rectangle?(bg), Color.White);

                if(toRight)
                    b.DrawString(font,list[int.Parse(component.name)],new Vector2(component.bounds.X,component.bounds.Y+Game1.tileSize/16),Color.Black);
                else
                    b.DrawString(font, list[int.Parse(component.name)], new Vector2(component.bounds.X+component.bounds.Width-font.MeasureString(list[int.Parse(component.name)]).X, component.bounds.Y + Game1.tileSize / 16), Color.Black);
            }
        }
    }
}
