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
        private int count;
        private bool cClicked;
        private bool lClicked;
        private readonly SpriteFont font = Game1.smallFont;
        private readonly Dictionary<string, List<Vector2>> chestList;
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

        public override void receiveKeyPress(Keys key)
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

        public override void receiveScrollWheelAction(int direction)
        {
            if (this.cClicked)
                this.chestDropList.receiveScrollWheelAction(direction);
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
                    ACAMenu.chestVector2 = this.chestList[ACAMenu.location.Name][index];
                    this.chestItems = ((Chest)ACAMenu.location.objects[ACAMenu.chestVector2]).items;
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
                    ACAMenu.location = Game1.getLocationFromName(this.chestList.ElementAt(index).Key);
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
            int y = this.yPositionOnScreen;
            List<string> list1 = new List<string>();
            foreach (Vector2 index in this.chestList[ACAMenu.location.name])
                list1.Add(ACAMenu.location.objects[index].Name);
            int x1 = this.xPositionOnScreen + Game1.tileSize / 4;
            this.chestDropList = new DropList(this.chestList[ACAMenu.location.name].IndexOf(ACAMenu.chestVector2), list1, x1, y, true, this.font);
            List<string> list2 = this.chestList.Keys.ToList();
            int x2 = this.xPositionOnScreen + this.width - Game1.tileSize / 4;
            this.locationDropList = new DropList(list2.IndexOf(ACAMenu.location.Name), list2, x2, y, false, this.font);
        }

        private void initTabs()
        {
            int x1 = this.xPositionOnScreen + Game1.tileSize / 4;
            int y1 = this.yPositionOnScreen - Game1.tileSize - Game1.tileSize / 16;
            this.chestTab = new Tab(ACAMenu.location.objects[ACAMenu.chestVector2].Name, x1, y1, true, this.font);
            int x2 = this.xPositionOnScreen + this.width - Game1.tileSize / 4;
            int y2 = this.yPositionOnScreen - Game1.tileSize - Game1.tileSize / 16;
            this.locationsTab = new Tab(ACAMenu.location.Name, x2, y2, false, this.font);
        }

        private void init()
        {
            if (ACAMenu.location == null)
                ACAMenu.location = Game1.getLocationFromName(this.chestList.ElementAt(0).Key);
            if (ACAMenu.chestVector2.Equals(Vector2.Zero))
                ACAMenu.chestVector2 = this.chestList[ACAMenu.location.Name][0];
            else if (!this.chestList[ACAMenu.location.Name].Contains(ACAMenu.chestVector2))
                ACAMenu.chestVector2 = this.chestList[ACAMenu.location.Name][0];
            this.chestItems = ((Chest)ACAMenu.location.objects[ACAMenu.chestVector2]).items;
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
