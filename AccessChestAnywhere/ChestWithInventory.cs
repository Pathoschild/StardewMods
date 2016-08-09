using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace AccessChestAnywhere
{
    public class ChestWithInventory : IClickableMenu
    {
        public bool disable;
        public List<Item> chestItems = new List<Item>();
        public readonly int capacity;
        private readonly List<Item> playerItems;
        private readonly Rectangle cornerTopLeft = new Rectangle(12, 16, 32, 32);
        private readonly Rectangle cornerTopRight = new Rectangle(212, 16, 32, 32);
        private readonly Rectangle cornerBottomLeft = new Rectangle(12, 208, 32, 32);
        private readonly Rectangle cornerBottomRight = new Rectangle(212, 208, 32, 32);
        private readonly Rectangle middleLeft = new Rectangle(12, 80, 32, 32);
        private readonly Rectangle middlemiddle = new Rectangle(132, 80, 32, 32);
        private readonly Rectangle middleRight = new Rectangle(212, 80, 32, 32);
        private readonly Rectangle edgeTop = new Rectangle(40, 16, 32, 32);
        private readonly Rectangle edgeLeft = new Rectangle(12, 36, 32, 32);
        private readonly Rectangle edgeRight = new Rectangle(212, 40, 32, 32);
        private readonly Rectangle edgeBottom = new Rectangle(36, 208, 32, 32);
        private readonly List<ClickableComponent> slots = new List<ClickableComponent>();
        private Item hoveredItem;

        public ChestWithInventory(int capacity = 36)
        {
            this.width = Game1.tileSize * 13;
            this.height = Game1.tileSize * 7 + Game1.tileSize / 2;
            this.xPositionOnScreen = Game1.viewport.Width / 2 - this.width / 2;
            this.yPositionOnScreen = Game1.viewport.Height / 2 - this.height / 2 + Game1.tileSize;
            this.playerItems = Game1.player.Items;
            this.capacity = capacity;
            this.initializeComponents();
        }

        public override void performHoverAction(int x, int y)
        {
            if (disable)
                return;

            // find hovered item
            Item item = null;
            try
            {
                for (int i = this.slots.Count - 1; i >= 0; i--)
                {
                    // chest
                    if (i < 36)
                    {
                        if (this.slots[i].containsPoint(x, y))
                        {
                            if (i < chestItems.Count && chestItems[i] != null)
                            {
                                item = this.chestItems[i];
                                break;
                            }
                        }
                    }

                    // player
                    else
                    {
                        if (this.slots[i].containsPoint(x, y))
                        {
                            if (this.playerItems[i - 36] != null)
                            {
                                item = this.playerItems[i - 36];
                                break;
                            }
                        }
                    }
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                item = null; // happens when hovering over an unavailable slot (e.g. player doesn't have bigger backpack)
            }

            // set
            this.hoveredItem = item;
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (this.disable)
                return;
            for (int i = this.slots.Count - 1; i >= 0; i--)
            {
                // chest to player
                if (i < 36)
                {
                    if (this.slots[i].containsPoint(x, y))
                    {
                        if (i < chestItems.Count && chestItems[i] != null)
                        {
                            this.chestItems[i] = this.tryToAddItem(this.chestItems[i], true);
                            if (this.chestItems[i] == null)
                                this.chestItems.RemoveAt(i);
                            if (playSound)
                                Game1.playSound("coin");
                        }
                    }
                }

                // player to chest
                else
                {
                    if (this.slots[i].containsPoint(x, y))
                    {
                        if (this.playerItems[i - 36] != null)
                        {
                            this.playerItems[i - 36] = this.tryToAddItem(this.playerItems[i - 36], false);
                            if (playSound)
                                Game1.playSound("coin");
                        }
                    }
                }
            }
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            if (this.disable)
                return;
            for (int i = this.slots.Count - 1; i >= 0; i--)
            {
                // chest to player
                if (i < 36)
                {
                    if (this.slots[i].containsPoint(x, y))
                    {
                        if (i < this.chestItems.Count && this.chestItems[i] != null)
                        {
                            Item itemToPlace = this.chestItems[i].getOne();
                            if (Game1.oldKBState.IsKeyDown(Keys.LeftShift) && this.chestItems[i].Stack > 1)
                            {
                                itemToPlace.Stack = this.chestItems[i].Stack / 2;
                                itemToPlace = this.tryToAddItem(itemToPlace, true);
                                if (itemToPlace == null)
                                    this.chestItems[i].Stack -= this.chestItems[i].Stack / 2;
                                if (this.chestItems[i].Stack <= 0)
                                    this.chestItems.RemoveAt(i);
                            }
                            else
                            {
                                itemToPlace = this.tryToAddItem(itemToPlace, true);
                                if (itemToPlace == null && this.chestItems[i].Stack == 1)
                                    this.chestItems.RemoveAt(i);
                                else
                                    this.chestItems[i].Stack--;
                            }
                            if (playSound)
                                Game1.playSound("coin");
                        }
                    }
                }

                // player to chest
                else
                {
                    if (this.slots[i].containsPoint(x, y))
                    {
                        if (playerItems[i - 36] != null)
                        {
                            Item itemToPlace = this.playerItems[i - 36].getOne();
                            if (Game1.oldKBState.IsKeyDown(Keys.LeftShift) && this.playerItems[i - 36].Stack > 1)
                            {
                                itemToPlace.Stack = this.playerItems[i - 36].Stack / 2;
                                itemToPlace = this.tryToAddItem(itemToPlace, false);
                                if (itemToPlace == null)
                                    this.playerItems[i - 36].Stack -= this.playerItems[i - 36].Stack / 2;
                            }
                            else
                            {
                                itemToPlace = this.tryToAddItem(itemToPlace, false);
                                if (itemToPlace == null && this.playerItems[i - 36].Stack == 1)
                                    this.playerItems[i - 36] = null;
                                else
                                    this.playerItems[i - 36].Stack--;
                            }

                            if (playSound)
                                Game1.playSound("coin");
                        }
                    }
                }
            }
        }

        private Item tryToAddItem(Item itemToPlace, bool toPlayer)
        {
            // to player
            if (toPlayer)
            {
                foreach (Item item in this.playerItems)
                {
                    if (item == null || !item.canStackWith(itemToPlace))
                        continue;

                    itemToPlace.Stack = item.addToStack(itemToPlace.Stack);
                    if (itemToPlace.Stack <= 0)
                        return null;
                }
                if (this.playerItems.Contains(null))
                {
                    for (int i = 0; i < this.playerItems.Count; i++)
                    {
                        if (this.playerItems[i] == null)
                        {
                            this.playerItems[i] = itemToPlace;
                            return null;
                        }
                    }
                }
                return itemToPlace;
            }

            // to chest
            foreach (Item item in this.chestItems)
            {
                if (item == null || !item.canStackWith(itemToPlace))
                    continue;

                itemToPlace.Stack = item.addToStack(itemToPlace.Stack);
                if (itemToPlace.Stack <= 0)
                    return null;
            }
            if (this.chestItems.Count < this.capacity)
            {
                this.chestItems.Add(itemToPlace);
                return null;
            }
            return itemToPlace;
        }

        private void initializeComponents()
        {
            // slots
            int wCount = 1;
            int count = 0;
            int y = this.yPositionOnScreen + Game1.tileSize / 2;
            while (wCount < 3)
            {
                int yCount = 0;
                while (yCount < 3)
                {
                    int xCount = 1;
                    int x = this.xPositionOnScreen + Game1.tileSize / 2;
                    while (xCount < 13)
                    {
                        this.slots.Add(
                            new ClickableComponent(new Rectangle(x, y, Game1.tileSize, Game1.tileSize), wCount == 1 ? $"Chest Slot {yCount * 12 + xCount}" : $"Player Slot {yCount * 12 + xCount}")
                        );
                        x += Game1.tileSize;
                        xCount++;
                        count++;
                    }
                    y += Game1.tileSize;
                    yCount++;
                }
                y = this.yPositionOnScreen + this.height / 2 + Game1.tileSize / 4;
                wCount++;
            }
        }

        public override void draw(SpriteBatch b)
        {
            // bg menu
            b.Draw(Game1.menuTexture, new Rectangle(this.xPositionOnScreen + Game1.tileSize / 16, this.yPositionOnScreen + Game1.tileSize / 16, this.width - Game1.tileSize / 8, this.height - Game1.tileSize / 8), new Rectangle(64, 128, 64, 64), Color.White);
            this.drawBorder(b);

            // slots
            this.drawChestSlots(b);
            this.drawPlayerSlots(b);

            // items
            this.drawChestItems(b);
            this.drawPlayerItems(b);

            // tooltips
            if (this.hoveredItem != null)
            {
                string hoverText = this.hoveredItem.getHoverBoxText(this.hoveredItem);
                string hoverTitle = null;
                if (hoverText == null)
                {
                    hoverText = this.hoveredItem.getDescription();
                    hoverTitle = this.hoveredItem.Name;
                }

                IClickableMenu.drawToolTip(b, hoverText, hoverTitle, this.hoveredItem);
            }

        }

        private void drawPlayerItems(SpriteBatch b)
        {
            int slotCount = 36;

            while (slotCount < 72)
            {
                if (slotCount - 36 >= this.playerItems.Count)
                    break;

                this.playerItems[slotCount - 36]?.drawInMenu(b, new Vector2(this.slots[slotCount].bounds.X, this.slots[slotCount].bounds.Y), this.slots[slotCount].containsPoint(Game1.getMouseX(), Game1.getMouseY()) ? 1f : 0.8f);
                slotCount++;
            }
        }

        private void drawChestItems(SpriteBatch b)
        {
            int slotCount = 0;
            while (slotCount < 72)
            {
                if (slotCount >= this.chestItems.Count)
                    break;

                this.chestItems[slotCount]?.drawInMenu(b, new Vector2(this.slots[slotCount].bounds.X, this.slots[slotCount].bounds.Y), this.slots[slotCount].containsPoint(Game1.getMouseX(), Game1.getMouseY()) ? 1f : 0.8f);
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
                    Rectangle sourceRectangle = yCount * 12 + xCount <= Game1.player.maxItems
                        ? new Rectangle(128, 128, 64, 64)
                        : new Rectangle(64, 896, 64, 64);
                    b.Draw(Game1.menuTexture, new Rectangle(x, y, Game1.tileSize, Game1.tileSize), sourceRectangle, Color.White);
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
            b.Draw(Game1.menuTexture, new Rectangle(this.xPositionOnScreen, this.yPositionOnScreen, ts2, ts2), this.cornerTopLeft, Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(this.xPositionOnScreen + ts2, this.yPositionOnScreen, this.width - ts, ts2), this.edgeTop, Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(this.xPositionOnScreen + this.width - ts2, this.yPositionOnScreen, ts2, ts2), this.cornerTopRight, Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(this.xPositionOnScreen, this.yPositionOnScreen + ts2, ts2, this.height - ts), this.edgeLeft, Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(this.xPositionOnScreen + this.width - ts2, this.yPositionOnScreen + ts2, ts2, this.height - ts), this.edgeRight, Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(this.xPositionOnScreen + ts2, this.yPositionOnScreen + this.height / 2 - ts / 4, this.width - ts, ts2), this.middlemiddle, Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(this.xPositionOnScreen, this.yPositionOnScreen + this.height / 2 - ts / 4, ts2, ts2), this.middleLeft, Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(this.xPositionOnScreen + this.width - ts2, this.yPositionOnScreen + this.height / 2 - ts / 4, ts2, ts2), this.middleRight, Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(this.xPositionOnScreen, this.yPositionOnScreen + this.height - ts2, ts2, ts2), this.cornerBottomLeft, Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(this.xPositionOnScreen + ts2, this.yPositionOnScreen + this.height - ts2, this.width - ts, ts2), this.edgeBottom, Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(this.xPositionOnScreen + this.width - ts2, this.yPositionOnScreen + this.height - ts2, ts2, ts2), this.cornerBottomRight, Color.White);
        }
    }
}
