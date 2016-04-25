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
using Object = StardewValley.Object;

namespace AccessChestAnywhere
{
    class ACAMenu : ChestWithInventory
    {
        private int count = 0;

        private Dictionary<string, List<Vector2>> chestList;
        private static GameLocation location;
        private static Vector2 chestVector2;


        private Tab chestTab;
        private Tab locationsTab;

        private bool cClicked = false;
        private DropList chestDropList;
        private bool lClicked = false;
        private DropList locationDropList;

        private SpriteFont font = Game1.smallFont;

        public ACAMenu(Dictionary<string, List<Vector2>> chestList)
            : base()
        {
            this.chestList = chestList;
            init();
            initTabs();
            initDropLists();
        }

        public override void receiveKeyPress(Keys key)
        {
            if(count>10)
            switch (key)
            {
                case Keys.E:
                case Keys.Escape:
                case Keys.B:
                    exitThisMenuNoSound();
                    break;
            }
        }

        public override void receiveScrollWheelAction(int direction)
        {
            if(cClicked) chestDropList.receiveScrollWheelAction(direction);
            else if (lClicked) locationDropList.receiveScrollWheelAction(direction);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            if (cClicked)
            {
                if (chestDropList.containsPoint(x, y))
                {
                    cClicked = false;
                    disable = false;
                    int index = chestDropList.select(x, y);
                    if (index >= 0)
                    {
                        chestVector2 = chestList[location.Name][index];
                        chestItems = ((Chest)location.objects[chestVector2]).items;
                        initTabs();
                    }
                }
            }
            else if (lClicked)
            {
                if (locationDropList.containsPoint(x, y))
                {
                    lClicked = false;
                    disable = false;
                    int index = locationDropList.select(x, y);
                    if (index >= 0)
                    {
                        location = Game1.getLocationFromName(chestList.ElementAt(index).Key);
                        init();
                        initTabs();
                        initDropLists();
                    }
                }
            }
            else
            {
                if (chestTab.containsPoint(x, y))
                {
                    cClicked = true;
                    disable = true;
                }
                else if (locationsTab.containsPoint(x,y))
                {
                    lClicked = true;
                    disable = true;
                }
            }
        }

        private void initDropLists()
        {
            int x, y;
            y = this.yPositionOnScreen;

            List<string> cList = new List<string>();
            foreach (Vector2 v in chestList[location.name])
            {
                cList.Add(location.objects[v].Name);
            }
            x = this.xPositionOnScreen + Game1.tileSize/4;
            chestDropList = new DropList(chestList[location.name].IndexOf(chestVector2),cList,x,y,true,font);

            List<string> lList = chestList.Keys.ToList();
            x = this.xPositionOnScreen + this.width - Game1.tileSize/4;
            locationDropList = new DropList(lList.IndexOf(location.Name),lList,x,y,false,font);
        }

        private void initTabs()
        {
            int x = this.xPositionOnScreen + Game1.tileSize / 4;
            int y = this.yPositionOnScreen - Game1.tileSize - Game1.tileSize / 16;
            chestTab = new Tab(location.objects[chestVector2].Name, x, y, true, font);

            x = this.xPositionOnScreen + this.width - Game1.tileSize / 4;
            y = this.yPositionOnScreen - Game1.tileSize - Game1.tileSize / 16;
            locationsTab = new Tab(location.Name, x, y, false, font);
        }

        private void init()
        {
            if (location == null)
                location = Game1.getLocationFromName(chestList.ElementAt(0).Key);
            if (chestVector2.Equals(Vector2.Zero))
            {
                chestVector2 = chestList[location.Name][0];
            }
            else
            {
                if (!chestList[location.Name].Contains(chestVector2))
                {
                    chestVector2 = chestList[location.Name][0];
                }
            }
            chestItems = ((Chest) location.objects[chestVector2]).items;
        }

        public override void draw(SpriteBatch b)
        {
            count++;
            //chest with inventory menu
            base.draw(b);
            //draw tabs
            chestTab.draw(b);
            locationsTab.draw(b);
            //draw lists
            if(cClicked) chestDropList.draw(b);
            if(lClicked) locationDropList.draw(b);
            //draw mouse
            this.drawMouse(b);
        }
    }
}
