using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;

namespace AccessChestAnywhere
{
    public class ChestWithInventory : MenuWithInventory
    {
        public bool disable = false;

        public List<Item> chestItems;
        public int capacity;

        private List<Item> playerItems;

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

        private List<ClickableComponent> slots = new List<ClickableComponent>();
        

        public ChestWithInventory(int capacity = 36)
             : base()
        {
            this.width = Game1.tileSize * 13;
            this.height = Game1.tileSize * 7 + Game1.tileSize / 2;
            this.xPositionOnScreen = Game1.viewport.Width / 2 - (width / 2);
            this.yPositionOnScreen = Game1.viewport.Height / 2 - (height / 2) + Game1.tileSize;
            playerItems = Game1.player.Items;
            chestItems = new List<Item>();
            this.capacity = capacity;
            initializeComponents();
        }

        public override void performHoverAction(int x, int y)
        {
            if (disable) return;
            for (var i = slots.Count - 1; i >= 0; i--)
            {
                if (i < 36)
                {
                    if (slots[i].containsPoint(x, y))
                    {
                        if (i < chestItems.Count && chestItems[i] != null)
                        {
                            Log.AsyncG(chestItems[i].Name);
                            hoveredItem = chestItems[i];
                        }
                    }
                }//chest
                else
                {
                    if (slots[i].containsPoint(x, y))
                    {
                        if (playerItems[i - 36] != null)
                        {
                            Log.AsyncG(playerItems[i - 36].Name);
                            hoveredItem = playerItems[i - 36];
                        }
                    }
                }//player
            }

        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if(disable) return;
            for (var i = slots.Count - 1; i >= 0; i--)
            {
                if (i < 36)
                {
                    if (slots[i].containsPoint(x, y))
                    {
                        if (i < chestItems.Count && chestItems[i] != null)
                        {
                            chestItems[i] = tryToAddItem(chestItems[i], true);
                            if (chestItems[i] == null) chestItems.RemoveAt(i);
                            if (playSound)
                                Game1.playSound("coin");
                        }
                    }
                }//chest to player
                else
                {
                    if (slots[i].containsPoint(x, y))
                    {
                        if (playerItems[i - 36] != null)
                        {
                            playerItems[i - 36] = tryToAddItem(playerItems[i - 36], false);
                            if (playSound)
                                Game1.playSound("coin");
                        }
                    }
                }//player to chest
            }
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            if (disable) return;
            for (var i = slots.Count - 1; i >= 0; i--)
            {
                if (i < 36)
                {
                    if (slots[i].containsPoint(x, y))
                    {
                        if (i < chestItems.Count && chestItems[i] != null)
                        {
                            Item itemToPlace = chestItems[i].getOne();
                            if (Game1.oldKBState.IsKeyDown(Keys.LeftShift) && chestItems[i].Stack > 1)
                            {
                                itemToPlace.Stack = chestItems[i].Stack / 2;
                                itemToPlace = tryToAddItem(itemToPlace, true);
                                if (itemToPlace == null) chestItems[i].Stack -= chestItems[i].Stack / 2;
                                if (chestItems[i].Stack <= 0) chestItems.RemoveAt(i);
                            }
                            else
                            {
                                itemToPlace = tryToAddItem(itemToPlace, true);
                                if (itemToPlace == null && chestItems[i].Stack == 1) chestItems.RemoveAt(i);
                                else chestItems[i].Stack--;
                            }
                            if (playSound)
                                Game1.playSound("coin");
                        }
                    }
                }//chest to player
                else
                {
                    if (slots[i].containsPoint(x, y))
                    {
                        if (playerItems[i - 36] != null)
                        {
                            Item itemToPlace = playerItems[i - 36].getOne();
                            if (Game1.oldKBState.IsKeyDown(Keys.LeftShift) && playerItems[i - 36].Stack > 1)
                            {
                                itemToPlace.Stack = playerItems[i - 36].Stack / 2;
                                itemToPlace = tryToAddItem(itemToPlace, false);
                                if (itemToPlace == null) playerItems[i - 36].Stack -= playerItems[i - 36].Stack / 2;
                            }
                            else
                            {
                                itemToPlace = tryToAddItem(itemToPlace, false);
                                if (itemToPlace == null && playerItems[i - 36].Stack == 1) playerItems[i - 36] = null;
                                else playerItems[i - 36].Stack--;
                            }
                            if (playSound)
                                Game1.playSound("coin");
                        }
                    }
                }//player to chest
            }
        }

        private Item tryToAddItem(Item itemToPlace, bool toPlayer)
        {
            if (toPlayer)
            {
                for (int i = 0; i < playerItems.Count; i++)
                {
                    if (playerItems[i] != null)
                        if (playerItems[i].canStackWith(itemToPlace))
                        {
                            itemToPlace.Stack = playerItems[i].addToStack(itemToPlace.Stack);
                            if (itemToPlace.Stack <= 0) return null;
                        }
                }
                if (playerItems.Contains(null))
                {
                    for (int i = 0; i < playerItems.Count; i++)
                    {
                        if (playerItems[i] == null)
                        {
                            playerItems[i] = itemToPlace;
                            return null;
                        }
                    }
                }
                return itemToPlace;
            }
            else
            {
                for (int i = 0; i < chestItems.Count; i++)
                {
                    if (chestItems[i] != null)
                        if (chestItems[i].canStackWith(itemToPlace))
                        {
                            itemToPlace.Stack = chestItems[i].addToStack(itemToPlace.Stack);
                            if (itemToPlace.Stack <= 0) return null;
                        }
                }
                if (chestItems.Count < capacity)
                {
                    chestItems.Add(itemToPlace);
                    return null;
                }
                return itemToPlace;
            }
        }

        private void initializeComponents()
        {
            //slots
            int wCount = 1;
            int count = 0;
            int y = yPositionOnScreen + Game1.tileSize / 2;
            while (wCount < 3)
            {
                int yCount = 0;
                while (yCount < 3)
                {
                    int xCount = 1;
                    int x = xPositionOnScreen + Game1.tileSize / 2;
                    while (xCount < 13)
                    {
                        slots.Add(new ClickableComponent(new Rectangle(x, y, Game1.tileSize, Game1.tileSize), wCount == 1 ? string.Format("Chest Slot {0}", yCount * 12 + xCount) : string.Format("Player Slot {0}", yCount * 12 + xCount)));
                        x += Game1.tileSize;
                        xCount++;
                        count++;
                    }
                    y += Game1.tileSize;
                    yCount++;
                }
                y = yPositionOnScreen + height / 2 + Game1.tileSize / 4;
                wCount++;
            }
        }

        public override void draw(SpriteBatch b)
        {
            //bg menu
            b.Draw(Game1.menuTexture, new Rectangle(xPositionOnScreen + Game1.tileSize / 16, yPositionOnScreen + Game1.tileSize / 16, width - Game1.tileSize / 8, height - Game1.tileSize / 8),
                new Rectangle?(new Rectangle(64, 128, 64, 64)), Color.White);
            drawBorder(b);
            //draw slots
            drawChestSlots(b);
            drawPlayerSlots(b);
            //draw items
            drawChestItems(b);
            drawPlayerItems(b);

        }

        private void drawPlayerItems(SpriteBatch b)
        {
            int slotCount = 36;
            while (slotCount < 72)
            {
                if (slotCount - 36 < playerItems.Count)
                {
                    playerItems[slotCount - 36]?.drawInMenu(b, new Vector2(slots[slotCount].bounds.X, slots[slotCount].bounds.Y), slots[slotCount].containsPoint(Game1.getMouseX(), Game1.getMouseY()) ? 1f : 0.8f);
                }
                else
                    break;
                slotCount++;
            }
        }

        private void drawChestItems(SpriteBatch b)
        {
            int slotCount = 0;
            while (slotCount < 72)
            {
                if (slotCount < chestItems.Count)
                {
                    chestItems[slotCount]?.drawInMenu(b, new Vector2(slots[slotCount].bounds.X, slots[slotCount].bounds.Y), slots[slotCount].containsPoint(Game1.getMouseX(), Game1.getMouseY()) ? 1f : 0.8f);
                }
                else
                    break;
                slotCount++;
            }
        }

        private void drawPlayerSlots(SpriteBatch b)
        {
            int yCount = 0;
            int y = yPositionOnScreen + height / 2 + Game1.tileSize / 4;
            while (yCount < 3)
            {
                int xCount = 1;
                int x = xPositionOnScreen + Game1.tileSize / 2;
                while (xCount < 13)
                {
                    if (yCount * 12 + xCount <= Game1.player.maxItems)
                        b.Draw(Game1.menuTexture, new Rectangle(x, y, Game1.tileSize, Game1.tileSize),
                            new Rectangle?(new Rectangle(128, 128, 64, 64)), Color.White);
                    else
                        b.Draw(Game1.menuTexture, new Rectangle(x, y, Game1.tileSize, Game1.tileSize),
                            new Rectangle?(new Rectangle(64, 896, 64, 64)), Color.White);
                    x += Game1.tileSize;
                    xCount++;
                }
                y += Game1.tileSize;
                yCount++;
            }
        }

        private void drawChestSlots(SpriteBatch b)
        {
            int yCount = 0;
            int y = yPositionOnScreen + Game1.tileSize / 2;
            while (yCount < 3)
            {
                int xCount = 0;
                int x = xPositionOnScreen + Game1.tileSize / 2;
                while (xCount < 12)
                {
                    b.Draw(Game1.menuTexture, new Rectangle(x, y, Game1.tileSize, Game1.tileSize),
                        new Rectangle?(new Rectangle(128, 128, 64, 64)), Color.White);
                    x += Game1.tileSize;
                    xCount++;
                }
                y += Game1.tileSize;
                yCount++;
            }
        }

        private void drawBorder(SpriteBatch b)
        {
            int ts = Game1.tileSize;
            int ts2 = Game1.tileSize / 2;
            int ts16 = Game1.tileSize / 16;
            b.Draw(Game1.menuTexture, new Rectangle(xPositionOnScreen, yPositionOnScreen, ts2, ts2),
                new Rectangle?(cornerTopLeft), Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(xPositionOnScreen + ts2, yPositionOnScreen, width - ts, ts2),
                new Rectangle?(edgeTop),
                Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(xPositionOnScreen + width - ts2, yPositionOnScreen, ts2, ts2),
                new Rectangle?(cornerTopRight),
                Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(xPositionOnScreen, yPositionOnScreen + ts2, ts2, height - ts),
                new Rectangle?(edgeLeft),
                Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(xPositionOnScreen + width - ts2, yPositionOnScreen + ts2, ts2, height - ts),
                new Rectangle?(edgeRight),
                Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(xPositionOnScreen + ts2, yPositionOnScreen + height / 2 - ts / 4, width - ts, ts2),
                new Rectangle?(middlemiddle),
                Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(xPositionOnScreen, yPositionOnScreen + height / 2 - ts / 4, ts2, ts2),
                new Rectangle?(middleLeft), Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(xPositionOnScreen + width - ts2, yPositionOnScreen + height / 2 - ts / 4, ts2, ts2),
                new Rectangle?(middleRight),
                Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(xPositionOnScreen, yPositionOnScreen + height - ts2, ts2, ts2),
                new Rectangle?(cornerBottomLeft),
                Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(xPositionOnScreen + ts2, yPositionOnScreen + height - ts2, width - ts,
                    ts2),
                new Rectangle?(edgeBottom), Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(xPositionOnScreen + width - ts2, yPositionOnScreen + height - ts2,
                    ts2, ts2),
                new Rectangle?(cornerBottomRight), Color.White);
        }
    }
}
