using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace AccessChestAnywhere
{
    public class ACA : MenuWithInventory
    {
        private int count = 0;

        ACAConfig config = AccessChestAnywhere.config;
        private AccessChestAnywhere aca;

        private SpriteFont font = Game1.smallFont;

        private ACABlueprintMenu menuBP;
        private ACATab chestTab;
        private static Vector2 chestVector;
        private ACATab locationTab;
        private static string locationName;

        public ACA(AccessChestAnywhere a)
        {
            aca = a;
            menuBP = new ACABlueprintMenu();
            initTabs();
            setTabPositions();
            checkTabs();
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            menuBP.reBound();
            setTabPositions();
        }

        public override void receiveKeyPress(Keys key)
        {
            if (key.ToString().Equals(config.hotkeys["keyboard"]) && count > 10) exitThisMenu();
        }

        public override void receiveGamePadButton(Buttons b)
        {
            if (b.ToString().Equals(config.hotkeys["gamepad"]) && count > 10) exitThisMenu();
        }

        public override void performHoverAction(int x, int y)
        {

        }

        public override void draw(SpriteBatch b)
        {
            count++;
            menuBP.draw(b);
            chestTab.draw(b);
            locationTab.draw(b);
            drawMouse(b);
        }

        private void initTabs()
        {
            chestTab = new ACATab(true, font);
            locationTab = new ACATab(false, font);
        }

        private void setTabPositions()
        {
            chestTab.setTabPosIndex(menuBP.bounds.X+Game1.tileSize/4,menuBP.bounds.Y - Game1.tileSize - Game1.tileSize/16);
            locationTab.setTabPosIndex(menuBP.bounds.X + menuBP.bounds.Width - Game1.tileSize / 4, menuBP.bounds.Y - Game1.tileSize - Game1.tileSize / 16);
        }
        
        private void checkTabs()
        {
            if (locationName == null || !aca.chests.ContainsKey(locationName))
            {
                locationName = aca.chests.ElementAt(0).Key;
            }
            locationTab.setName(locationName);
            if (chestVector.Equals(Vector2.Zero)) chestVector = aca.chests[locationName][0].TileLocation;
            else
            {
                if (getChestAt(chestVector)==null) chestVector = aca.chests[locationName][0].TileLocation;
            }
            chestTab.setName(getChestAt(chestVector).name);
        }

        private Chest getChestAt(Vector2 v)
        {
            foreach (Chest chest in aca.chests[locationName])
            {
                if (chestVector.Equals(chest.tileLocation))
                {
                    return chest;
                }
            }
            return null;
        }
    }
}