// Decompiled with JetBrains decompiler
// Type: AccessChestAnywhere.DropList
// Assembly: AccessChestAnywhere, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A5EF4C5A-AE47-40FE-981A-E2469D9B9502
// Assembly location: C:\Program Files (x86)\GalaxyClient\Games\Stardew Valley\Mods\AccessChestAnywhere\AccessChestAnywhere.dll

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
        private List<string> list;
        private SpriteFont font;
        private bool toRight;
        private Rectangle bgSel;
        private Rectangle bg;
        private Rectangle bgHover;
        private List<ClickableComponent> listComponents;

        public DropList(int index, List<string> list, int x, int y, bool toRight, SpriteFont font)
        {
            this.\u002Ector(new Rectangle(), index.ToString());
            this.index = index;
            this.list = list;
            this.font = font;
            this.toRight = toRight;
            // ISSUE: explicit reference operation
            // ISSUE: cast to a reference type
            // ISSUE: explicit reference operation
            (^(Rectangle&) @this.bounds).Width = Game1.tileSize * 7;
            // ISSUE: explicit reference operation
            // ISSUE: cast to a reference type
            // ISSUE: explicit reference operation
            (^(Rectangle&) @this.bounds).Height = (int)font.MeasureString("abcdefghijklmnopqrstuvwxyz").Y * 10 + Game1.tileSize / 16 * 9;
            if (toRight)
            {
                // ISSUE: explicit reference operation
                // ISSUE: cast to a reference type
                // ISSUE: explicit reference operation
                (^(Rectangle&) @this.bounds).X = x;
            }
            else
            {
                // ISSUE: explicit reference operation
                // ISSUE: cast to a reference type
                // ISSUE: explicit reference operation
                // ISSUE: explicit reference operation
                // ISSUE: cast to a reference type
                // ISSUE: explicit reference operation
                (^(Rectangle&) @this.bounds).X = x - (^(Rectangle&) @this.bounds).Width;
            }
          // ISSUE: explicit reference operation
          // ISSUE: cast to a reference type
          // ISSUE: explicit reference operation
          (^(Rectangle&) @this.bounds).Y = y;
            this.initComponents();
            if (index <= 9)
                return;
            int index1 = 9;
            int num = index;
            for (; index1 >= 0; --index1)
            {
                this.listComponents[index1].name = (__Null)num.ToString();
                --num;
            }
        }

        public void receiveScrollWheelAction(int direction)
        {
            if (direction > 0)
            {
                if (this.listComponents.Count != 10 || int.Parse((string)this.listComponents[9].name) <= 9)
                    return;
                using (List<ClickableComponent>.Enumerator enumerator = this.listComponents.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        ClickableComponent current = enumerator.Current;
                        current.name = (__Null)(int.Parse((string)current.name) - 1).ToString();
                    }
                }
            }
            else if (this.listComponents.Count == 10 && int.Parse((string)this.listComponents[9].name) + 1 < this.list.Count)
            {
                using (List<ClickableComponent>.Enumerator enumerator = this.listComponents.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        ClickableComponent current = enumerator.Current;
                        current.name = (__Null)(int.Parse((string)current.name) + 1).ToString();
                    }
                }
            }
        }

        public int select(int x, int y)
        {
            using (List<ClickableComponent>.Enumerator enumerator = this.listComponents.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    ClickableComponent current = enumerator.Current;
                    if (current.containsPoint(x, y))
                    {
                        this.index = int.Parse((string)current.name);
                        return this.index;
                    }
                }
            }
            return -1;
        }

        private void initComponents()
        {
            int num = 0;
            // ISSUE: explicit reference operation
            // ISSUE: cast to a reference type
            // ISSUE: explicit reference operation
            int x = (^(Rectangle&) @this.bounds).X;
            // ISSUE: explicit reference operation
            // ISSUE: cast to a reference type
            // ISSUE: explicit reference operation
            int y = (^(Rectangle&) @this.bounds).Y;
            // ISSUE: explicit reference operation
            // ISSUE: cast to a reference type
            // ISSUE: explicit reference operation
            int width = (^(Rectangle&) @this.bounds).Width;
            int height = (int)this.font.MeasureString("abcdefghijklmnopqrstuvwxyz").Y;
            for (; num < this.list.Count && num < 10; ++num)
            {
                this.listComponents.Add(new ClickableComponent(new Rectangle(x, y, width, height), num.ToString()));
                y += (int)this.font.MeasureString("abcdefghijklmnopqrstuvwxyz").Y + Game1.tileSize / 16;
            }
        }

        public void draw(SpriteBatch b)
        {
            using (List<ClickableComponent>.Enumerator enumerator = this.listComponents.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    ClickableComponent current = enumerator.Current;
                    if (current.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
                        b.Draw((Texture2D)Game1.mouseCursors, (Rectangle)current.bounds, new Rectangle?(this.bgHover), Color.White);
                    else if (((string)current.name).Equals(this.index.ToString()))
                        b.Draw((Texture2D)Game1.mouseCursors, (Rectangle)current.bounds, new Rectangle?(this.bgSel), Color.White);
                    else
                        b.Draw((Texture2D)Game1.mouseCursors, (Rectangle)current.bounds, new Rectangle?(this.bg), Color.White);
                    if (this.toRight)
                    {
                        // ISSUE: explicit reference operation
                        // ISSUE: cast to a reference type
                        // ISSUE: explicit reference operation
                        // ISSUE: explicit reference operation
                        // ISSUE: cast to a reference type
                        // ISSUE: explicit reference operation
                        b.DrawString(this.font, this.list[int.Parse((string)current.name)], new Vector2((float)(^(Rectangle&) @current.bounds).X, (float)((^(Rectangle&) @current.bounds).Y + Game1.tileSize / 16)), Color.Black);
                    }
                    else
                    {
                        // ISSUE: explicit reference operation
                        // ISSUE: cast to a reference type
                        // ISSUE: explicit reference operation
                        // ISSUE: explicit reference operation
                        // ISSUE: cast to a reference type
                        // ISSUE: explicit reference operation
                        // ISSUE: explicit reference operation
                        // ISSUE: cast to a reference type
                        // ISSUE: explicit reference operation
                        b.DrawString(this.font, this.list[int.Parse((string)current.name)], new Vector2((float)((^(Rectangle&) @current.bounds).X + (^(Rectangle&) @current.bounds).Width) -this.font.MeasureString(this.list[int.Parse((string)current.name)]).X, (float)((^(Rectangle&) @current.bounds).Y + Game1.tileSize / 16)), Color.Black);
                    }
                }
            }
        }
    }
}
