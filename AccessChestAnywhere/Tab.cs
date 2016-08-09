// Decompiled with JetBrains decompiler
// Type: AccessChestAnywhere.Tab
// Assembly: AccessChestAnywhere, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A5EF4C5A-AE47-40FE-981A-E2469D9B9502
// Assembly location: C:\Program Files (x86)\GalaxyClient\Games\Stardew Valley\Mods\AccessChestAnywhere\AccessChestAnywhere.dll

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace AccessChestAnywhere
{
    internal class Tab : ClickableComponent
    {
        private Rectangle tabTopLeft;
        private Rectangle tabTopRight;
        private Rectangle tabBottomLeft;
        private Rectangle tabBottomRight;
        private Rectangle tabEdgeTop;
        private Rectangle tabEdgeLeft;
        private Rectangle tabEdgeRight;
        private Rectangle tabEdgeBottom;
        private Rectangle tabBG;
        private SpriteFont font;

        public Tab(string name, int x, int y, bool toRight, SpriteFont font)
        {
            this.\u002Ector(new Rectangle(), name);
            this.font = font;
            Vector2 vector2 = font.MeasureString(name);
            // ISSUE: explicit reference operation
            // ISSUE: cast to a reference type
            // ISSUE: explicit reference operation
            (^(Rectangle&) @this.bounds).Width = (int)vector2.X + Game1.tileSize / 2;
            // ISSUE: explicit reference operation
            // ISSUE: cast to a reference type
            // ISSUE: explicit reference operation
            (^(Rectangle&) @this.bounds).Height = (int)vector2.Y + Game1.tileSize / 2;
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
        }

        public void draw(SpriteBatch b)
        {
            this.drawTab(b);
        }

        private void drawTab(SpriteBatch b)
        {
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
            int num1 = (^(Rectangle&) @this.bounds).Width;
            // ISSUE: explicit reference operation
            // ISSUE: cast to a reference type
            // ISSUE: explicit reference operation
            int num2 = (^(Rectangle&) @this.bounds).Height;
            int num3 = (int)Game1.tileSize;
            b.Draw((Texture2D)Game1.mouseCursors, new Rectangle(x, y, num3 / 4, num3 / 4), new Rectangle?(this.tabTopLeft), Color.White);
            b.Draw((Texture2D)Game1.mouseCursors, new Rectangle(x + num3 / 4, y, num1 - num3 / 2, num3 / 4), new Rectangle?(this.tabEdgeTop), Color.White);
            b.Draw((Texture2D)Game1.mouseCursors, new Rectangle(x + num1 - num3 / 4, y, num3 / 4, num3 / 4), new Rectangle?(this.tabTopRight), Color.White);
            b.Draw((Texture2D)Game1.mouseCursors, new Rectangle(x, y + num3 / 4, num3 / 4, num2 - num3 / 2), new Rectangle?(this.tabEdgeLeft), Color.White);
            b.Draw((Texture2D)Game1.mouseCursors, new Rectangle(x + num1 - num3 / 4, y + num3 / 4, num3 / 4, num2 - num3 / 2), new Rectangle?(this.tabEdgeRight), Color.White);
            b.Draw((Texture2D)Game1.mouseCursors, new Rectangle(x, y + num2 - num3 / 4, num3 / 4, num3 / 4), new Rectangle?(this.tabBottomLeft), Color.White);
            b.Draw((Texture2D)Game1.mouseCursors, new Rectangle(x + num3 / 4, y + num2 - num3 / 4, num1 - num3 / 2, num3 / 4), new Rectangle?(this.tabEdgeBottom), Color.White);
            b.Draw((Texture2D)Game1.mouseCursors, new Rectangle(x + num1 - num3 / 4, y + num2 - num3 / 4, num3 / 4, num3 / 4), new Rectangle?(this.tabBottomRight), Color.White);
            b.Draw((Texture2D)Game1.mouseCursors, new Rectangle(x + num3 / 4, y + num3 / 4, num1 - num3 / 2, num2 - num3 / 2), new Rectangle?(this.tabBG), Color.White);
            b.DrawString(this.font, (string)this.name, new Vector2((float)(x + num3 / 4), (float)(y + num3 / 4 + num3 / 16)), Color.Black);
        }
    }
}
