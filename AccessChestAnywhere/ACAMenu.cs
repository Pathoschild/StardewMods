using System;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;

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

        /// <summary>The known chests.</summary>
        private readonly ManagedChest[] Chests;

        /// <summary>The known chest locations.</summary>
        private readonly GameLocation[] Locations;

        /// <summary>The selected location.</summary>
        private GameLocation SelectedLocation => this.SelectedChest.Location;

        /// <summary>The font with which to render text.</summary>
        private readonly SpriteFont Font = Game1.smallFont;

        /// <summary>The chest selector tab.</summary>
        private Tab ChestTab;

        /// <summary>The location selector tab.</summary>
        private Tab LocationTab;

        /// <summary>The chest selector dropdown.</summary>
        private DropList<ManagedChest> ChestSelector;

        /// <summary>The location selector dropdown.</summary>
        private DropList<GameLocation> LocationSelector;

        /// <summary>The key which toggles the chest UI.</summary>
        private Keys ToggleKey;


        /*********
        ** Accessors
        *********/
        /// <summary>An event raised when the player selects a chest.</summary>
        public event Action<ManagedChest> OnChestSelected;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="chests">The known chests.</param>
        /// <param name="selectedChest">The selected chest.</param>
        /// <param name="toggleKey">The key which toggles the chest UI.</param>
        public ACAMenu(ManagedChest[] chests, ManagedChest selectedChest, Keys toggleKey)
        {
            this.Chests = chests;
            this.SelectedChest = selectedChest;
            this.ToggleKey = toggleKey;
            this.Locations = this.Chests.Select(p => p.Location).Distinct().ToArray();
            this.InitialiseTabs();
            this.InitialiseSelectors();
        }

        /// <summary>The method invoked when the player presses a key.</summary>
        /// <param name="key">The key that was pressed.</param>
        public override void receiveKeyPress(Keys key)
        {
            if (this.Count <= 10)
                return;

            if (key == this.ToggleKey || key == Keys.Escape)
                this.exitThisMenuNoSound();
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

                ManagedChest chest = this.ChestSelector.Select(x, y);
                if (chest != null)
                {
                    this.SelectedChest = chest;
                    this.OnChestSelected?.Invoke(this.SelectedChest);
                    this.InitialiseTabs();
                }
            }
            else if (this.LocationListOpen)
            {
                if (!this.LocationSelector.containsPoint(x, y))
                    return;
                this.LocationListOpen = false;
                this.IsDisabled = false;
                GameLocation location = this.LocationSelector.Select(x, y);
                if (location != null && location != this.SelectedLocation)
                {
                    this.SelectedChest = this.Chests.First(p => p.Location == location);
                    this.OnChestSelected?.Invoke(this.SelectedChest);
                    this.InitialiseTabs();
                    this.InitialiseSelectors();
                }
            }
            else if (this.ChestTab.containsPoint(x, y))
            {
                this.ChestListOpen = true;
                this.IsDisabled = true;
            }
            else if (this.LocationTab?.containsPoint(x, y) == true)
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
            if (this.Locations.Length > 1)
                this.LocationTab.Draw(sprites);

            // tab dropdowns
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
        /// <summary>Initialise the chest and location selectors.</summary>
        private void InitialiseSelectors()
        {
            // chest selector
            {
                ManagedChest[] chests = this.Chests.Where(chest => chest.Location == this.SelectedLocation).ToArray();
                int x = this.xPositionOnScreen + Game1.tileSize / 4;
                int y = this.yPositionOnScreen;
                this.ChestSelector = new DropList<ManagedChest>(this.SelectedChest, chests, chest => chest.Name, x, y, true, this.Font);
            }

            // location selector
            {
                GameLocation[] locations = this.Chests
                    .Select(p => p.Location)
                    .Distinct()
                    .ToArray();
                int x = this.xPositionOnScreen + this.width - Game1.tileSize / 4;
                int y = this.yPositionOnScreen;
                this.LocationSelector = new DropList<GameLocation>(this.SelectedLocation, locations, location => location.Name, x, y, false, this.Font);
            }
        }

        /// <summary>Initialise the chest and location tabs.</summary>
        private void InitialiseTabs()
        {
            // chest
            {
                int x = this.xPositionOnScreen + Game1.tileSize / 4;
                int y = this.yPositionOnScreen - Game1.tileSize - Game1.tileSize / 16;
                this.ChestTab = new Tab(this.SelectedChest.Name, x, y, true, this.Font);
            }

            // location
            if (this.Locations.Length > 1)
            {
                int x = this.xPositionOnScreen + this.width - Game1.tileSize / 4;
                int y = this.yPositionOnScreen - Game1.tileSize - Game1.tileSize / 16;
                this.LocationTab = new Tab(this.SelectedLocation.Name, x, y, false, this.Font);
            }
        }
    }
}
