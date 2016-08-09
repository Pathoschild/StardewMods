// Decompiled with JetBrains decompiler
// Type: AccessChestAnywhere.ACAMenu
// Assembly: AccessChestAnywhere, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A5EF4C5A-AE47-40FE-981A-E2469D9B9502
// Assembly location: C:\Program Files (x86)\GalaxyClient\Games\Stardew Valley\Mods\AccessChestAnywhere\AccessChestAnywhere.dll

using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Objects;

namespace AccessChestAnywhere
{
    internal class ACAMenu : ChestWithInventory
    {
        private int count = 0;
        private bool cClicked = false;
        private bool lClicked = false;
        private SpriteFont font = (SpriteFont)Game1.smallFont;
        private Dictionary<string, List<Vector2>> chestList;
        private static GameLocation location;
        private static Vector2 chestVector2;
        private Tab chestTab;
        private Tab locationsTab;
        private DropList chestDropList;
        private DropList locationDropList;

        public ACAMenu(Dictionary<string, List<Vector2>> chestList)
          : base(36)
        {
            this.chestList = chestList;
            this.init();
            this.initTabs();
            this.initDropLists();
        }

        public virtual void receiveKeyPress(Keys key)
        {
            if (this.count <= 10)
                return;
            switch (key)
            {
                case Keys.Escape:
                case Keys.B:
                case Keys.E:
                    this.exitThisMenuNoSound();
                    break;
            }
        }

        public virtual void receiveScrollWheelAction(int direction)
        {
            if (this.cClicked)
            {
                this.chestDropList.receiveScrollWheelAction(direction);
            }
            else
            {
                if (!this.lClicked)
                    return;
                this.locationDropList.receiveScrollWheelAction(direction);
            }
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            if (this.cClicked)
            {
                if (!this.chestDropList.containsPoint(x, y))
                    return;
                this.cClicked = false;
                this.disable = false;
                int index = this.chestDropList.select(x, y);
                if (index >= 0)
                {
                    ACAMenu.chestVector2 = this.chestList[ACAMenu.location.get_Name()][index];
                    this.chestItems = (List<Item>)((Chest)((Dictionary<Vector2, Object>)ACAMenu.location.objects)[ACAMenu.chestVector2]).items;
                    this.initTabs();
                }
            }
            else if (this.lClicked)
            {
                if (!this.locationDropList.containsPoint(x, y))
                    return;
                this.lClicked = false;
                this.disable = false;
                int index = this.locationDropList.select(x, y);
                if (index >= 0)
                {
                    ACAMenu.location = Game1.getLocationFromName(this.chestList.ElementAt<KeyValuePair<string, List<Vector2>>>(index).Key);
                    this.init();
                    this.initTabs();
                    this.initDropLists();
                }
            }
            else if (this.chestTab.containsPoint(x, y))
            {
                this.cClicked = true;
                this.disable = true;
            }
            else if (this.locationsTab.containsPoint(x, y))
            {
                this.lClicked = true;
                this.disable = true;
            }
        }

        private void initDropLists()
        {
            int y = (int)this.yPositionOnScreen;
            List<string> list1 = new List<string>();
            foreach (Vector2 index in this.chestList[(string)ACAMenu.location.name])
                list1.Add(((Item)((Dictionary<Vector2, Object>)ACAMenu.location.objects)[index]).get_Name());
            int x1 = this.xPositionOnScreen + Game1.tileSize / 4;
            this.chestDropList = new DropList(this.chestList[(string)ACAMenu.location.name].IndexOf(ACAMenu.chestVector2), list1, x1, y, true, this.font);
            List<string> list2 = this.chestList.Keys.ToList<string>();
            int x2 = this.xPositionOnScreen + this.width - Game1.tileSize / 4;
            this.locationDropList = new DropList(list2.IndexOf(ACAMenu.location.get_Name()), list2, x2, y, false, this.font);
        }

        private void initTabs()
        {
            int x1 = this.xPositionOnScreen + Game1.tileSize / 4;
            int y1 = this.yPositionOnScreen - Game1.tileSize - Game1.tileSize / 16;
            this.chestTab = new Tab(((Item)((Dictionary<Vector2, Object>)ACAMenu.location.objects)[ACAMenu.chestVector2]).get_Name(), x1, y1, true, this.font);
            int x2 = this.xPositionOnScreen + this.width - Game1.tileSize / 4;
            int y2 = this.yPositionOnScreen - Game1.tileSize - Game1.tileSize / 16;
            this.locationsTab = new Tab(ACAMenu.location.get_Name(), x2, y2, false, this.font);
        }

        private void init()
        {
            if (ACAMenu.location == null)
                ACAMenu.location = Game1.getLocationFromName(this.chestList.ElementAt<KeyValuePair<string, List<Vector2>>>(0).Key);
            if (ACAMenu.chestVector2.Equals(Vector2.Zero))
                ACAMenu.chestVector2 = this.chestList[ACAMenu.location.get_Name()][0];
            else if (!this.chestList[ACAMenu.location.get_Name()].Contains(ACAMenu.chestVector2))
                ACAMenu.chestVector2 = this.chestList[ACAMenu.location.get_Name()][0];
            this.chestItems = (List<Item>)((Chest)((Dictionary<Vector2, Object>)ACAMenu.location.objects)[ACAMenu.chestVector2]).items;
        }

        public override void draw(SpriteBatch b)
        {
            this.count = this.count + 1;
            base.draw(b);
            this.chestTab.draw(b);
            this.locationsTab.draw(b);
            if (this.cClicked)
                this.chestDropList.draw(b);
            if (this.lClicked)
                this.locationDropList.draw(b);
            this.drawMouse(b);
        }
    }
}
