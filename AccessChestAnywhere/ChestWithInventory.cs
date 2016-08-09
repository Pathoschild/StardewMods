// Decompiled with JetBrains decompiler
// Type: AccessChestAnywhere.ChestWithInventory
// Assembly: AccessChestAnywhere, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A5EF4C5A-AE47-40FE-981A-E2469D9B9502
// Assembly location: C:\Program Files (x86)\GalaxyClient\Games\Stardew Valley\Mods\AccessChestAnywhere\AccessChestAnywhere.dll

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using xTile.Dimensions;

namespace AccessChestAnywhere
{
    public class ChestWithInventory : IClickableMenu
    {
        public bool disable;
        public List<Item> chestItems;
        public int capacity;
        private List<Item> playerItems;
        private Rectangle cornerTopLeft;
        private Rectangle cornerTopRight;
        private Rectangle cornerBottomLeft;
        private Rectangle cornerBottomRight;
        private Rectangle middleLeft;
        private Rectangle middlemiddle;
        private Rectangle middleRight;
        private Rectangle edgeTop;
        private Rectangle edgeLeft;
        private Rectangle edgeRight;
        private Rectangle edgeBottom;
        private List<ClickableComponent> slots;

        public ChestWithInventory(int capacity = 36)
        {
            this.\u002Ector();
            this.width = (__Null)(Game1.tileSize * 13);
            this.height = (__Null)(Game1.tileSize * 7 + Game1.tileSize / 2);
            // ISSUE: explicit reference operation
            this.xPositionOnScreen = (__Null)(((Rectangle)@Game1.viewport).get_Width() / 2 - this.width / 2);
            // ISSUE: explicit reference operation
            this.yPositionOnScreen = (__Null)(((Rectangle)@Game1.viewport).get_Height() / 2 - this.height / 2 + Game1.tileSize);
            this.playerItems = ((Farmer)Game1.player).get_Items();
            this.chestItems = new List<Item>();
            this.capacity = capacity;
            this.initializeComponents();
        }

        public virtual void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (this.disable)
                return;
            for (int index = this.slots.Count - 1; index >= 0; --index)
            {
                if (index < 36)
                {
                    if (this.slots[index].containsPoint(x, y) && (index < this.chestItems.Count && this.chestItems[index] != null))
                    {
                        this.chestItems[index] = this.tryToAddItem(this.chestItems[index], true);
                        if (this.chestItems[index] == null)
                            this.chestItems.RemoveAt(index);
                        if (playSound)
                            Game1.playSound("coin");
                    }
                }
                else if (this.slots[index].containsPoint(x, y) && this.playerItems[index - 36] != null)
                {
                    this.playerItems[index - 36] = this.tryToAddItem(this.playerItems[index - 36], false);
                    if (playSound)
                        Game1.playSound("coin");
                }
            }
        }

        public virtual void receiveRightClick(int x, int y, bool playSound = true)
        {
            if (this.disable)
                return;
            for (int index = this.slots.Count - 1; index >= 0; --index)
            {
                if (index < 36)
                {
                    if (this.slots[index].containsPoint(x, y) && (index < this.chestItems.Count && this.chestItems[index] != null))
                    {
                        Item one = this.chestItems[index].getOne();
                        if (Game1.oldKBState.IsKeyDown(Keys.LeftShift) && this.chestItems[index].get_Stack() > 1)
                        {
                            one.set_Stack(this.chestItems[index].get_Stack() / 2);
                            if (this.tryToAddItem(one, true) == null)
                            {
                                Item obj = this.chestItems[index];
                                int num = obj.get_Stack() - this.chestItems[index].get_Stack() / 2;
                                obj.set_Stack(num);
                            }
                            if (this.chestItems[index].get_Stack() <= 0)
                                this.chestItems.RemoveAt(index);
                        }
                        else if (this.tryToAddItem(one, true) == null && this.chestItems[index].get_Stack() == 1)
                        {
                            this.chestItems.RemoveAt(index);
                        }
                        else
                        {
                            Item obj = this.chestItems[index];
                            int num = obj.get_Stack() - 1;
                            obj.set_Stack(num);
                        }
                        if (playSound)
                            Game1.playSound("coin");
                    }
                }
                else if (this.slots[index].containsPoint(x, y) && this.playerItems[index - 36] != null)
                {
                    Item one = this.playerItems[index - 36].getOne();
                    if (Game1.oldKBState.IsKeyDown(Keys.LeftShift) && this.playerItems[index - 36].get_Stack() > 1)
                    {
                        one.set_Stack(this.playerItems[index - 36].get_Stack() / 2);
                        if (this.tryToAddItem(one, false) == null)
                        {
                            Item obj = this.playerItems[index - 36];
                            int num = obj.get_Stack() - this.playerItems[index - 36].get_Stack() / 2;
                            obj.set_Stack(num);
                        }
                    }
                    else if (this.tryToAddItem(one, false) == null && this.playerItems[index - 36].get_Stack() == 1)
                    {
                        this.playerItems[index - 36] = (Item)null;
                    }
                    else
                    {
                        Item obj = this.playerItems[index - 36];
                        int num = obj.get_Stack() - 1;
                        obj.set_Stack(num);
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
                for (int index = 0; index < this.playerItems.Count; ++index)
                {
                    if (this.playerItems[index] != null && this.playerItems[index].canStackWith(itemToPlace))
                    {
                        itemToPlace.set_Stack(this.playerItems[index].addToStack(itemToPlace.get_Stack()));
                        if (itemToPlace.get_Stack() <= 0)
                            return (Item)null;
                    }
                }
                if (this.playerItems.Contains((Item)null))
                {
                    for (int index = 0; index < this.playerItems.Count; ++index)
                    {
                        if (this.playerItems[index] == null)
                        {
                            this.playerItems[index] = itemToPlace;
                            return (Item)null;
                        }
                    }
                }
                return itemToPlace;
            }
            for (int index = 0; index < this.chestItems.Count; ++index)
            {
                if (this.chestItems[index] != null && this.chestItems[index].canStackWith(itemToPlace))
                {
                    itemToPlace.set_Stack(this.chestItems[index].addToStack(itemToPlace.get_Stack()));
                    if (itemToPlace.get_Stack() <= 0)
                        return (Item)null;
                }
            }
            if (this.chestItems.Count >= this.capacity)
                return itemToPlace;
            this.chestItems.Add(itemToPlace);
            return (Item)null;
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
                        this.slots.Add(new ClickableComponent(new Rectangle(x, y, (int)Game1.tileSize, (int)Game1.tileSize), num1 == 1 ? string.Format("Chest Slot {0}", (object)(index * 12 + num3)) : string.Format("Player Slot {0}", (object)(index * 12 + num3))));
                        x += (int)Game1.tileSize;
                        ++num3;
                        ++num2;
                    }
                    y += (int)Game1.tileSize;
                }
                y = this.yPositionOnScreen + this.height / 2 + Game1.tileSize / 4;
            }
        }

        public virtual void draw(SpriteBatch b)
        {
            b.Draw((Texture2D)Game1.menuTexture, new Rectangle(this.xPositionOnScreen + Game1.tileSize / 16, this.yPositionOnScreen + Game1.tileSize / 16, this.width - Game1.tileSize / 8, this.height - Game1.tileSize / 8), new Rectangle?(new Rectangle(64, 128, 64, 64)), Color.White);
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
                    // ISSUE: explicit reference operation
                    // ISSUE: cast to a reference type
                    // ISSUE: explicit reference operation
                    // ISSUE: explicit reference operation
                    // ISSUE: cast to a reference type
                    // ISSUE: explicit reference operation
                    Vector2 vector2 = new Vector2((float)(^ (Rectangle &) @this.slots[index].bounds).X, (float)(^ (Rectangle &) @this.slots[index].bounds).Y);
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
            // ISSUE: explicit reference operation
            // ISSUE: cast to a reference type
            // ISSUE: explicit reference operation
            // ISSUE: explicit reference operation
            // ISSUE: cast to a reference type
            // ISSUE: explicit reference operation
            Vector2 vector2 = new Vector2((float)(^ (Rectangle &) @this.slots[index].bounds).X, (float)(^ (Rectangle &) @this.slots[index].bounds).Y);
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
            if (num1 * 12 + num2 <= ((Farmer)Game1.player).maxItems)
                b.Draw((Texture2D)Game1.menuTexture, new Rectangle(x, y, (int)Game1.tileSize, (int)Game1.tileSize), new Rectangle?(new Rectangle(128, 128, 64, 64)), Color.White);
            else
                b.Draw((Texture2D)Game1.menuTexture, new Rectangle(x, y, (int)Game1.tileSize, (int)Game1.tileSize), new Rectangle?(new Rectangle(64, 896, 64, 64)), Color.White);
            x += (int)Game1.tileSize;
        }
        y += (int)Game1.tileSize;
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
            b.Draw((Texture2D)Game1.menuTexture, new Rectangle(x, y, (int)Game1.tileSize, (int)Game1.tileSize), new Rectangle?(new Rectangle(128, 128, 64, 64)), Color.White);
            x += (int)Game1.tileSize;
        }
        y += (int)Game1.tileSize;
    }
}

private void drawBorder(SpriteBatch b)
{
    int num1 = (int)Game1.tileSize;
    int num2 = Game1.tileSize / 2;
    int num3 = Game1.tileSize / 16;
    b.Draw((Texture2D)Game1.menuTexture, new Rectangle((int)this.xPositionOnScreen, (int)this.yPositionOnScreen, num2, num2), new Rectangle?(this.cornerTopLeft), Color.White);
    b.Draw((Texture2D)Game1.menuTexture, new Rectangle(this.xPositionOnScreen + num2, (int)this.yPositionOnScreen, this.width - num1, num2), new Rectangle?(this.edgeTop), Color.White);
    b.Draw((Texture2D)Game1.menuTexture, new Rectangle(this.xPositionOnScreen + this.width - num2, (int)this.yPositionOnScreen, num2, num2), new Rectangle?(this.cornerTopRight), Color.White);
    b.Draw((Texture2D)Game1.menuTexture, new Rectangle((int)this.xPositionOnScreen, this.yPositionOnScreen + num2, num2, this.height - num1), new Rectangle?(this.edgeLeft), Color.White);
    b.Draw((Texture2D)Game1.menuTexture, new Rectangle(this.xPositionOnScreen + this.width - num2, this.yPositionOnScreen + num2, num2, this.height - num1), new Rectangle?(this.edgeRight), Color.White);
    b.Draw((Texture2D)Game1.menuTexture, new Rectangle(this.xPositionOnScreen + num2, this.yPositionOnScreen + this.height / 2 - num1 / 4, this.width - num1, num2), new Rectangle?(this.middlemiddle), Color.White);
    b.Draw((Texture2D)Game1.menuTexture, new Rectangle((int)this.xPositionOnScreen, this.yPositionOnScreen + this.height / 2 - num1 / 4, num2, num2), new Rectangle?(this.middleLeft), Color.White);
    b.Draw((Texture2D)Game1.menuTexture, new Rectangle(this.xPositionOnScreen + this.width - num2, this.yPositionOnScreen + this.height / 2 - num1 / 4, num2, num2), new Rectangle?(this.middleRight), Color.White);
    b.Draw((Texture2D)Game1.menuTexture, new Rectangle((int)this.xPositionOnScreen, this.yPositionOnScreen + this.height - num2, num2, num2), new Rectangle?(this.cornerBottomLeft), Color.White);
    b.Draw((Texture2D)Game1.menuTexture, new Rectangle(this.xPositionOnScreen + num2, this.yPositionOnScreen + this.height - num2, this.width - num1, num2), new Rectangle?(this.edgeBottom), Color.White);
    b.Draw((Texture2D)Game1.menuTexture, new Rectangle(this.xPositionOnScreen + this.width - num2, this.yPositionOnScreen + this.height - num2, num2, num2), new Rectangle?(this.cornerBottomRight), Color.White);
}
  }
}
