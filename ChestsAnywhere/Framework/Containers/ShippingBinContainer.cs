using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.Menus;
using SFarmer = StardewValley.Farmer;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.Containers
{
    /// <summary>A storage container for the shipping bin.</summary>
    internal class ShippingBinContainer : IContainer
    {
        /*********
        ** Fields
        *********/
        /// <summary>A key added to the mod data keys to distinguish different containers in the same mod data.</summary>
        internal static readonly string ModDataDiscriminator = "shipping-bin";

        /// <summary>The location containing the shipping bin.</summary>
        private readonly GameLocation Location;

        /// <summary>The farm instance. This is not necessarily the location which contains the shipping bin.</summary>
        private readonly Farm Farm;

        /// <summary>The underlying shipping bin.</summary>
        private readonly IInventory ShippingBin;

        /// <summary>The callback to invoke when an item is selected in the player inventory.</summary>
        private ItemGrabMenu.behaviorOnItemSelect GrabItemFromInventory => this.GrabItemFromInventoryImpl;

        /// <summary>The callback to invoke when an item is selected in the storage container.</summary>
        private ItemGrabMenu.behaviorOnItemSelect GrabItemFromContainer => this.GrabItemFromContainerImpl;


        /*********
        ** Accessors
        *********/
        /// <inheritdoc />
        public IList<Item?> Inventory => this.ShippingBin;

        /// <inheritdoc />
        public ContainerData Data { get; }

        /// <inheritdoc />
        public bool CanConfigureAutomate { get; } = false; // Automate can't read the shipping bin settings

        /// <summary>The type of shipping bin menu to create.</summary>
        public ShippingBinMode Mode { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="location">The location whose shipping bin to manage.</param>
        /// <param name="mode">The type of shipping bin menu to create.</param>
        public ShippingBinContainer(GameLocation location, ShippingBinMode mode)
        {
            this.Location = location;
            this.Farm = location as Farm ?? Game1.getFarm();
            this.ShippingBin = this.Farm.getShippingBin(Game1.player);
            this.Data = new ContainerData(location.modData, discriminator: ShippingBinContainer.ModDataDiscriminator);
            this.Mode = mode;
        }

        /// <inheritdoc />
        public bool CanAcceptItem(Item item)
        {
            return Utility.highlightShippableObjects(item);
        }

        /// <inheritdoc />
        public bool IsSameAs(IContainer? container)
        {
            return
                container is not null
                && this.IsSameAs(container.Inventory);
        }

        /// <inheritdoc />
        public bool IsSameAs(IList<Item?>? inventory)
        {
            return
                inventory is not null
                && object.ReferenceEquals(this.Inventory, inventory);
        }

        /// <inheritdoc />
        public IClickableMenu OpenMenu()
        {
            // build menu
            ItemGrabMenu menu;
            switch (this.Mode)
            {
                case ShippingBinMode.Normal:
                    menu = new ItemGrabMenu(
                        inventory: this.Inventory,
                        reverseGrab: false,
                        showReceivingMenu: true,
                        highlightFunction: this.CanAcceptItem,
                        behaviorOnItemSelectFunction: this.GrabItemFromInventory,
                        message: null,
                        behaviorOnItemGrab: this.GrabItemFromContainer,
                        canBeExitedWithKey: true,
                        showOrganizeButton: true,
                        context: this.Location
                    );
                    break;

                case ShippingBinMode.MobileStore:
                    {
                        menu = new ItemGrabMenu(
                            inventory: this.Inventory,
                            reverseGrab: false,
                            showReceivingMenu: true,
                            highlightFunction: this.CanAcceptItem,
                            behaviorOnItemSelectFunction: null,
                            message: null,
                            behaviorOnItemGrab: this.GrabItemFromContainer,
                            canBeExitedWithKey: true,
                            showOrganizeButton: true,
                            context: this.Location
                        );
                        menu.initializeShippingBin();
                        break;
                    }

                case ShippingBinMode.MobileTake:
                    menu = new ItemGrabMenu(
                        inventory: this.Inventory,
                        reverseGrab: false,
                        showReceivingMenu: true,
                        highlightFunction: this.CanAcceptItem,
                        behaviorOnItemSelectFunction: null,
                        message: null,
                        behaviorOnItemGrab: this.GrabItemFromContainer,
                        canBeExitedWithKey: true,
                        showOrganizeButton: true,
                        context: this.Location
                    );
                    break;

                default:
                    throw new NotSupportedException($"Unknown shipping bin mode '{this.Mode}'.");
            }

            // use shipping sound
            menu.inventory.moveItemSound = "Ship";

            return Game1.activeClickableMenu = menu;
        }

        /// <inheritdoc />
        public void SaveData()
        {
            this.Data.ToModData(this.Location.modData, discriminator: ShippingBinContainer.ModDataDiscriminator);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Add an item to the container from the player inventory.</summary>
        /// <param name="item">The item taken.</param>
        /// <param name="player">The player taking the item.</param>
        /// <remarks>This implementation replicates <see cref="StardewValley.Objects.Chest.grabItemFromInventory"/> without the slot limit (instead of using <c>Farm::shipItem</c> which does some weird things that don't work well with a full chest UI).</remarks>
        private void GrabItemFromInventoryImpl(Item item, SFarmer player)
        {
            // normalize
            if (item.Stack == 0)
                item.Stack = 1;

            // add to shipping bin
            this.ClearNulls();
            foreach (Item? slot in this.Inventory)
            {
                if (!slot!.canStackWith(item))
                    continue;

                item.Stack = slot.addToStack(item);
                if (item.Stack <= 0)
                    break;
            }
            if (item.Stack > 0)
                this.ShippingBin.Add(item);

            // remove from player inventory
            player.removeItemFromInventory(item);

            // reopen menu
            IClickableMenu menu = Game1.activeClickableMenu;
            int snappedComponentID = menu.currentlySnappedComponent?.myID ?? -1;
            menu = this.OpenMenu();
            if (snappedComponentID != -1)
            {
                menu.currentlySnappedComponent = menu.getComponentWithID(snappedComponentID);
                menu.snapCursorToCurrentSnappedComponent();
            }
        }

        /// <summary>Add an item to the player inventory from the container.</summary>
        /// <param name="item">The item taken.</param>
        /// <param name="player">The player taking the item.</param>
        private void GrabItemFromContainerImpl(Item item, SFarmer player)
        {
            if (!player.couldInventoryAcceptThisItem(item))
                return;
            if (Constants.TargetPlatform == GamePlatform.Android)
                player.addItemToInventory(item);
            this.ShippingBin.Remove(item);
            this.ClearNulls();
            if (item == this.Farm.lastItemShipped)
                this.Farm.lastItemShipped = this.ShippingBin.LastOrDefault();
        }

        /// <summary>Remove empty slots from the shipping bin's inventory list.</summary>
        private void ClearNulls()
        {
            for (int i = this.ShippingBin.Count - 1; i >= 0; i--)
            {
                if (this.ShippingBin[i] == null)
                    this.ShippingBin.RemoveAt(i);
            }
        }
    }
}
