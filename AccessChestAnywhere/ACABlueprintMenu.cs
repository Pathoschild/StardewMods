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
    class ACABlueprintMenu : ClickableComponent
    {

        private Rectangle cornerTopLeft = new Rectangle(12, 16, 32, 32);
        private Rectangle cornerTopRight = new Rectangle(212, 16, 32, 32);
        private Rectangle cornerBottomLeft = new Rectangle(12, 208, 32, 32);
        private Rectangle cornerBottomRight = new Rectangle(212, 208, 32, 32);
        private Rectangle middleLeft = new Rectangle(12, 80, 32, 32);
        private Rectangle middlemiddle = new Rectangle(132, 80, 32, 32);
        private Rectangle middleRight = new Rectangle(212, 80, 32, 32);
        private Rectangle edgeTop = new Rectangle(40, 16, 32, 32);
        private Rectangle edgeLeft = new Rectangle(12, 36, 32, 32);
        private Rectangle edgeRight = new Rectangle(212, 40, 32, 32);
        private Rectangle edgeBottom = new Rectangle(36, 208, 32, 32);

        public ClickableComponent chestBox;
        public ClickableComponent inventoryBox;
        public List<ClickableComponent> cSlots;
        public List<ClickableComponent> iSlots;

        public ACABlueprintMenu()
            : base(new Rectangle(), "")
        {
            reBound();
        }

        public void reBound()
        {
            bounds.Width = Game1.tileSize * 13;
            bounds.Height = Game1.tileSize * 7 + Game1.tileSize / 2;
            bounds.X = Game1.viewport.Width / 2 - (bounds.Width / 2);
            bounds.Y = Game1.viewport.Height / 2 - (bounds.Height / 2) + Game1.tileSize;
            initializeComponents();
        }

        public int getChestIndex(int x, int y)
        {
            foreach (ClickableComponent slot in cSlots)
            {
                if (slot.containsPoint(x, y))
                    return cSlots.IndexOf(slot);
            }
            return -1;
        }

        public int getInventoryIndex(int x, int y)
        {
            foreach (ClickableComponent slot in iSlots)
            {
                if (slot.containsPoint(x, y))
                    return iSlots.IndexOf(slot);
            }
            return -1;
        }

        private void initializeComponents()
        {
            chestBox = new ClickableComponent(new Rectangle(bounds.X + Game1.tileSize / 2, bounds.Y + Game1.tileSize / 2, Game1.tileSize * 12, Game1.tileSize * 3), "Chest Box");
            inventoryBox = new ClickableComponent(new Rectangle(bounds.X + Game1.tileSize / 2, bounds.Y + Game1.tileSize * 4, Game1.tileSize * 12, Game1.tileSize * 3), "Inventory Box");
            cSlots = new List<ClickableComponent>();
            iSlots = new List<ClickableComponent>();
            int slot = 1;
            int ycount = 0;
            int y = chestBox.bounds.Y;
            while (ycount < 3)
            {
                int xcount = 0;
                int x = chestBox.bounds.X;
                while (xcount < 12)
                {
                    cSlots.Add(new ClickableComponent(new Rectangle(x, y, Game1.tileSize, Game1.tileSize), $"Chest Slot {slot}"));
                    slot++;
                    xcount++;
                    x += Game1.tileSize;
                }
                ycount++;
                y += Game1.tileSize;
            }
            slot = 1;
            ycount = 0;
            y = inventoryBox.bounds.Y;
            while (ycount < 3)
            {
                int xcount = 0;
                int x = inventoryBox.bounds.X;
                while (xcount < 12)
                {
                    iSlots.Add(new ClickableComponent(new Rectangle(x, y, Game1.tileSize, Game1.tileSize), $"Inventory Slot {slot}"));
                    slot++;
                    xcount++;
                    x += Game1.tileSize;
                }
                ycount++;
                y += Game1.tileSize;
            }
        }

        public void draw(SpriteBatch b)
        {
            b.Draw(Game1.menuTexture, new Rectangle(bounds.X + Game1.tileSize / 16, bounds.Y + Game1.tileSize / 16, bounds.Width - Game1.tileSize / 8, bounds.Height - Game1.tileSize / 8),
                new Rectangle?(new Rectangle(64, 128, 64, 64)), Color.White);
            drawBorder(b);
            drawSlots(b);
        }

        private void drawSlots(SpriteBatch b)
        {
            foreach (ClickableComponent slot in cSlots)
            {
                b.Draw(Game1.menuTexture,slot.bounds, new Rectangle?(new Rectangle(128, 128, 64, 64)), Color.White);
            }
            int count = 0;
            foreach (ClickableComponent slot in iSlots)
            {
                if(count<Game1.player.maxItems)
                    b.Draw(Game1.menuTexture, slot.bounds, new Rectangle?(new Rectangle(128, 128, 64, 64)), Color.White);
                else
                    b.Draw(Game1.menuTexture, slot.bounds, new Rectangle?(new Rectangle(64, 896, 64, 64)), Color.White);
                count++;
            }
        }

        private void drawBorder(SpriteBatch b)
        {
            int ts = Game1.tileSize;
            int ts2 = Game1.tileSize / 2;
            int ts16 = Game1.tileSize / 16;
            b.Draw(Game1.menuTexture, new Rectangle(bounds.X, bounds.Y, ts2, ts2),
                new Rectangle?(cornerTopLeft), Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(bounds.X + ts2, bounds.Y, bounds.Width - ts, ts2),
                new Rectangle?(edgeTop),
                Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(bounds.X + bounds.Width - ts2, bounds.Y, ts2, ts2),
                new Rectangle?(cornerTopRight),
                Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(bounds.X, bounds.Y + ts2, ts2, bounds.Height - ts),
                new Rectangle?(edgeLeft),
                Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(bounds.X + bounds.Width - ts2, bounds.Y + ts2, ts2, bounds.Height - ts),
                new Rectangle?(edgeRight),
                Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(bounds.X + ts2, bounds.Y + bounds.Height / 2 - ts / 4, bounds.Width - ts, ts2),
                new Rectangle?(middlemiddle),
                Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(bounds.X, bounds.Y + bounds.Height / 2 - ts / 4, ts2, ts2),
                new Rectangle?(middleLeft), Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(bounds.X + bounds.Width - ts2, bounds.Y + bounds.Height / 2 - ts / 4, ts2, ts2),
                new Rectangle?(middleRight),
                Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(bounds.X, bounds.Y + bounds.Height - ts2, ts2, ts2),
                new Rectangle?(cornerBottomLeft),
                Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(bounds.X + ts2, bounds.Y + bounds.Height - ts2, bounds.Width - ts,
                    ts2),
                new Rectangle?(edgeBottom), Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(bounds.X + bounds.Width - ts2, bounds.Y + bounds.Height - ts2,
                    ts2, ts2),
                new Rectangle?(cornerBottomRight), Color.White);
        }
    }
}
