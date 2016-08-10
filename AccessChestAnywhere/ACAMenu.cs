using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Objects;

namespace AccessChestAnywhere
{
    /// <summary>A UI which lets the player choose a chest and location, and transfer transfer items between a chest and their inventory.</summary>
    internal class ACAMenu : ChestWithInventory
    {
        /*********
        ** Properties
        *********/
        private int Count;

        /// <summary>Whether the chest dropdown is open.</summary>
        private bool ChestListOpen;

        /// <summary>Whether the location dropdown is open.</summary>
        private bool LocationListOpen;

        /// <summary>The chests by their location name.</summary>
        private readonly IDictionary<string, Vector2[]> ChestsByLocation;

        /// <summary>The player's current location.</summary>
        private GameLocation Location;

        /// <summary>The font with which to render text.</summary>
        private readonly SpriteFont Font = Game1.smallFont;

        /// <summary>The coordinates of the current chest.</summary>
        private Vector2 ChestCoordinate;

        /// <summary>The chest selector tab.</summary>
        private Tab ChestTab;

        /// <summary>The location selector tab.</summary>
        private Tab LocationTab;

        /// <summary>The chest selector dropdown.</summary>
        private DropList ChestSelector;

        /// <summary>The location selector dropdown.</summary>
        private DropList LocationSelector;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="chestsByLocation">The chests by their location name.</param>
        public ACAMenu(IDictionary<string, Vector2[]> chestsByLocation)
        {
            this.ChestsByLocation = chestsByLocation;
            this.Initialise();
            this.InitialiseTabs();
            this.InitialiseSelectors();
        }

        /// <summary>The method invoked when the player presses a key.</summary>
        /// <param name="key">The key that was pressed.</param>
        public override void receiveKeyPress(Keys key)
        {
            if (this.Count <= 10)
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

        /// <summary>The method invoked when the player scrolls the dropdown using the mouse wheel.</summary>
        /// <param name="direction">The scroll direction.</param>
        public override void receiveScrollWheelAction(int direction)
        {
            if (this.ChestListOpen)
                this.ChestSelector.ReceiveScrollWheelAction(direction);
            else if (this.LocationListOpen)
                this.LocationSelector.ReceiveScrollWheelAction(direction);
        }

        /// <summary>The method invoked when the player left-clicks on the control.</summary>
        /// <param name="x">The X-position of the cursor.</param>
        /// <param name="y">The Y-position of the cursor.</param>
        /// <param name="playSound">Whether to enable sound.</param>
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            if (this.ChestListOpen)
            {
                if (!this.ChestSelector.containsPoint(x, y))
                    return;
                this.ChestListOpen = false;
                this.IsDisabled = false;
                int selectedIndex = this.ChestSelector.Select(x, y);
                if (selectedIndex >= 0)
                {
                    this.ChestCoordinate = this.ChestsByLocation[this.Location.Name][selectedIndex];
                    this.ChestItems = ((Chest)this.Location.objects[this.ChestCoordinate]).items;
                    this.InitialiseTabs();
                }
            }
            else if (this.LocationListOpen)
            {
                if (!this.LocationSelector.containsPoint(x, y))
                    return;
                this.LocationListOpen = false;
                this.IsDisabled = false;
                int selectedIndex = this.LocationSelector.Select(x, y);
                if (selectedIndex >= 0)
                {
                    this.Location = Game1.getLocationFromName(this.ChestsByLocation.ElementAt(selectedIndex).Key);
                    this.Initialise();
                    this.InitialiseTabs();
                    this.InitialiseSelectors();
                }
            }
            else if (this.ChestTab.containsPoint(x, y))
            {
                this.ChestListOpen = true;
                this.IsDisabled = true;
            }
            else if (this.LocationTab.containsPoint(x, y))
            {
                this.LocationListOpen = true;
                this.IsDisabled = true;
            }
        }

        /// <summary>Render the UI.</summary>
        /// <param name="sprites">The sprites to render.</param>
        public override void draw(SpriteBatch sprites)
        {
            this.Count++;

            // chest with inventory menu
            base.draw(sprites);

            // tabs
            this.ChestTab.Draw(sprites);
            this.LocationTab.Draw(sprites);
            if (this.ChestListOpen)
                this.ChestSelector.Draw(sprites);
            if (this.LocationListOpen)
                this.LocationSelector.Draw(sprites);

            // mouse
            this.drawMouse(sprites);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Initialise the control.</summary>
        private void Initialise()
        {
            if (this.Location == null)
                this.Location = Game1.getLocationFromName(this.ChestsByLocation.ElementAt(0).Key);
            if (this.ChestCoordinate.Equals(Vector2.Zero))
                this.ChestCoordinate = this.ChestsByLocation[this.Location.Name][0];
            else if (!this.ChestsByLocation[this.Location.Name].Contains(this.ChestCoordinate))
                this.ChestCoordinate = this.ChestsByLocation[this.Location.Name][0];
            this.ChestItems = ((Chest)this.Location.objects[this.ChestCoordinate]).items;
        }

        /// <summary>Initialise the chest and location selectors.</summary>
        private void InitialiseSelectors()
        {
            // chest selector
            {
                string[] chestNames = this.ChestsByLocation[this.Location.name]
                    .Select(v => this.Location.objects[v].Name)
                    .ToArray();
                int x = this.xPositionOnScreen + Game1.tileSize / 4;
                int y = this.yPositionOnScreen;
                this.ChestSelector = new DropList(Array.IndexOf(this.ChestsByLocation[this.Location.name], this.ChestCoordinate), chestNames, x, y, true, this.Font);
            }

            // location selector
            {
                string[] locationNames = this.ChestsByLocation.Keys.ToArray();
                int x = this.xPositionOnScreen + this.width - Game1.tileSize / 4;
                int y = this.yPositionOnScreen;
                this.LocationSelector = new DropList(Array.IndexOf(locationNames, this.Location.Name), locationNames, x, y, false, this.Font);
            }
        }

        /// <summary>Initialise the chest and location tabs.</summary>
        private void InitialiseTabs()
        {
            // chest
            {
                int x = this.xPositionOnScreen + Game1.tileSize / 4;
                int y = this.yPositionOnScreen - Game1.tileSize - Game1.tileSize / 16;
                this.ChestTab = new Tab(this.Location.objects[this.ChestCoordinate].Name, x, y, true, this.Font);
            }

            // location
            {
                int x = this.xPositionOnScreen + this.width - Game1.tileSize / 4;
                int y = this.yPositionOnScreen - Game1.tileSize - Game1.tileSize / 16;
                this.LocationTab = new Tab(this.Location.Name, x, y, false, this.Font);
            }
        }
    }
}
