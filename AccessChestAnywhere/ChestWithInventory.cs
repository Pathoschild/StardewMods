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

        public ChestWithInventory(int capacity = 36)
        {
            this.width = Game1.tileSize * 13;
            this.height = Game1.tileSize * 7 + Game1.tileSize / 2;
            this.xPositionOnScreen = @Game1.viewport.Width / 2 - this.width / 2;
            this.yPositionOnScreen = @Game1.viewport.Height / 2 - this.height / 2 + Game1.tileSize;
            this.playerItems = Game1.player.Items;
            this.capacity = capacity;
            this.initializeComponents();
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (this.disable)
                return;
            for (int i = this.slots.Count - 1; i >= 0; --i)
            {
                if (i < 36)
                {
                    if (this.slots[i].containsPoint(x, y) && i < this.chestItems.Count && this.chestItems[i] != null)
                    {
                        this.chestItems[i] = this.tryToAddItem(this.chestItems[i], true);
                        if (this.chestItems[i] == null)
                            this.chestItems.RemoveAt(i);
                        if (playSound)
                            Game1.playSound("coin");
                    }
                }
                else if (this.slots[i].containsPoint(x, y) && this.playerItems[i - 36] != null)
                {
                    this.playerItems[i - 36] = this.tryToAddItem(this.playerItems[i - 36], false);
                    if (playSound)
                        Game1.playSound("coin");
                }
            }
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            if (this.disable)
                return;
            for (int index = this.slots.Count - 1; index >= 0; --index)
            {
                if (index < 36)
                {
                    if (!this.slots[index].containsPoint(x, y) || (index >= this.chestItems.Count || this.chestItems[index] == null))
                        continue;

                    Item one = this.chestItems[index].getOne();
                    if (Game1.oldKBState.IsKeyDown(Keys.LeftShift) && this.chestItems[index].Stack > 1)
                    {
                        one.Stack = this.chestItems[index].Stack / 2;
                        if (this.tryToAddItem(one, true) == null)
                        {
                            Item obj = this.chestItems[index];
                            int num = obj.Stack - this.chestItems[index].Stack / 2;
                            obj.Stack = num;
                        }
                        if (this.chestItems[index].Stack <= 0)
                            this.chestItems.RemoveAt(index);
                    }
                    else if (this.tryToAddItem(one, true) == null && this.chestItems[index].Stack == 1)
                        this.chestItems.RemoveAt(index);
                    else
                    {
                        Item obj = this.chestItems[index];
                        int num = obj.Stack - 1;
                        obj.Stack = num;
                    }
                    if (playSound)
                        Game1.playSound("coin");
                }
                else if (this.slots[index].containsPoint(x, y) && this.playerItems[index - 36] != null)
                {
                    Item one = this.playerItems[index - 36].getOne();
                    if (Game1.oldKBState.IsKeyDown(Keys.LeftShift) && this.playerItems[index - 36].Stack > 1)
                    {
                        one.Stack = this.playerItems[index - 36].Stack / 2;
                        if (this.tryToAddItem(one, false) == null)
                        {
                            Item obj = this.playerItems[index - 36];
                            int num = obj.Stack - this.playerItems[index - 36].Stack / 2;
                            obj.Stack = num;
                        }
                    }
                    else if (this.tryToAddItem(one, false) == null && this.playerItems[index - 36].Stack == 1)
                        this.playerItems[index - 36] = null;
                    else
                    {
                        Item obj = this.playerItems[index - 36];
                        int num = obj.Stack - 1;
                        obj.Stack = num;
                    }
                    if (playSound)
                        Game1.playSound("coin");
                }
            }
        }

        private Item tryToAddItem(Item itemToPlace, bool toPlayer)
        {
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
                    for (int i = 0; i < this.playerItems.Count; ++i)
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
            foreach (Item item in this.chestItems)
            {
                if (item == null || !item.canStackWith(itemToPlace))
                    continue;

                itemToPlace.Stack = item.addToStack(itemToPlace.Stack);
                if (itemToPlace.Stack <= 0)
                    return null;
            }
            if (this.chestItems.Count >= this.capacity)
                return itemToPlace;
            this.chestItems.Add(itemToPlace);
            return null;
        }

        private void initializeComponents()
        {
            int num1 = 1;
            int num2 = 0;
            int y = this.yPositionOnScreen + Game1.tileSize / 2;
            for (; num1 < 3; ++num1)
            {
                for (int index = 0; index < 3; ++index)
                {
                    int num3 = 1;
                    int x = this.xPositionOnScreen + Game1.tileSize / 2;
                    while (num3 < 13)
                    {
                        this.slots.Add(
                            new ClickableComponent(new Rectangle(x, y, Game1.tileSize, Game1.tileSize), num1 == 1 ? $"Chest Slot {index * 12 + num3}" : $"Player Slot {index * 12 + num3}")
                        );
                        x += Game1.tileSize;
                        ++num3;
                        ++num2;
                    }
                    y += Game1.tileSize;
                }
                y = this.yPositionOnScreen + this.height / 2 + Game1.tileSize / 4;
            }
        }

        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.menuTexture, new Rectangle(this.xPositionOnScreen + Game1.tileSize / 16, this.yPositionOnScreen + Game1.tileSize / 16, this.width - Game1.tileSize / 8, this.height - Game1.tileSize / 8), new Rectangle(64, 128, 64, 64), Color.White);
            this.drawBorder(b);
            this.drawChestSlots(b);
            this.drawPlayerSlots(b);
            this.drawChestItems(b);
            this.drawPlayerItems(b);
        }

        private void drawPlayerItems(SpriteBatch b)
        {
            for (int index = 36; index < 72 && index - 36 < this.playerItems.Count; ++index)
            {
                Item obj = this.playerItems[index - 36];
                if (obj != null)
                {
                    SpriteBatch spriteBatch = b;
                    Vector2 vector2 = new Vector2(this.slots[index].bounds.X, this.slots[index].bounds.Y);
                    double num = this.slots[index].containsPoint(Game1.getMouseX(), Game1.getMouseY()) ? 1.0 : 0.800000011920929;
                    obj.drawInMenu(spriteBatch, vector2, (float)num);
                }
            }
        }

        private void drawChestItems(SpriteBatch b)
        {
            for (int index = 0; index < 72 && index < this.chestItems.Count; ++index)
            {
                Item obj = this.chestItems[index];
                if (obj != null)
                {
                    SpriteBatch spriteBatch = b;
                    Vector2 vector2 = new Vector2(this.slots[index].bounds.X, this.slots[index].bounds.Y);
                    double num = this.slots[index].containsPoint(Game1.getMouseX(), Game1.getMouseY()) ? 1.0 : 0.800000011920929;
                    obj.drawInMenu(spriteBatch, vector2, (float)num);
                }
            }
        }

        private void drawPlayerSlots(SpriteBatch b)
        {
            int num1 = 0;
            int y = this.yPositionOnScreen + this.height / 2 + Game1.tileSize / 4;
            for (; num1 < 3; ++num1)
            {
                int num2 = 1;
                int x = this.xPositionOnScreen + Game1.tileSize / 2;
                for (; num2 < 13; ++num2)
                {
                    if (num1 * 12 + num2 <= Game1.player.maxItems)
                        b.Draw(Game1.menuTexture, new Rectangle(x, y, Game1.tileSize, Game1.tileSize), new Rectangle(128, 128, 64, 64), Color.White);
                    else
                        b.Draw(Game1.menuTexture, new Rectangle(x, y, Game1.tileSize, Game1.tileSize), new Rectangle(64, 896, 64, 64), Color.White);
                    x += Game1.tileSize;
                }
                y += Game1.tileSize;
            }
        }

        private void drawChestSlots(SpriteBatch b)
        {
            int num1 = 0;
            int y = this.yPositionOnScreen + Game1.tileSize / 2;
            for (; num1 < 3; ++num1)
            {
                int num2 = 0;
                int x = this.xPositionOnScreen + Game1.tileSize / 2;
                for (; num2 < 12; ++num2)
                {
                    b.Draw(Game1.menuTexture, new Rectangle(x, y, Game1.tileSize, Game1.tileSize), new Rectangle(128, 128, 64, 64), Color.White);
                    x += Game1.tileSize;
                }
                y += Game1.tileSize;
            }
        }

        private void drawBorder(SpriteBatch b)
        {
            int num1 = Game1.tileSize;
            int num2 = Game1.tileSize / 2;
            int num3 = Game1.tileSize / 16;
            b.Draw(Game1.menuTexture, new Rectangle(this.xPositionOnScreen, this.yPositionOnScreen, num2, num2), this.cornerTopLeft, Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(this.xPositionOnScreen + num2, this.yPositionOnScreen, this.width - num1, num2), this.edgeTop, Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(this.xPositionOnScreen + this.width - num2, this.yPositionOnScreen, num2, num2), this.cornerTopRight, Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(this.xPositionOnScreen, this.yPositionOnScreen + num2, num2, this.height - num1), this.edgeLeft, Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(this.xPositionOnScreen + this.width - num2, this.yPositionOnScreen + num2, num2, this.height - num1), this.edgeRight, Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(this.xPositionOnScreen + num2, this.yPositionOnScreen + this.height / 2 - num1 / 4, this.width - num1, num2), this.middlemiddle, Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(this.xPositionOnScreen, this.yPositionOnScreen + this.height / 2 - num1 / 4, num2, num2), this.middleLeft, Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(this.xPositionOnScreen + this.width - num2, this.yPositionOnScreen + this.height / 2 - num1 / 4, num2, num2), this.middleRight, Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(this.xPositionOnScreen, this.yPositionOnScreen + this.height - num2, num2, num2), this.cornerBottomLeft, Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(this.xPositionOnScreen + num2, this.yPositionOnScreen + this.height - num2, this.width - num1, num2), this.edgeBottom, Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(this.xPositionOnScreen + this.width - num2, this.yPositionOnScreen + this.height - num2, num2, num2), this.cornerBottomRight, Color.White);
        }
    }
}
