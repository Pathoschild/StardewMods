using System.Collections.Generic;
using System.Linq;
using Netcode;
using StardewValley;
using StardewValley.Menus;
using SFarmer = StardewValley.Farmer;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.Containers
{
    /// <summary>A storage container for the shipping bin.</summary>
    internal class ShippingBinContainer : IContainer
    {
        /*********
        ** Properties
        *********/
        /// <summary>The farm containing the shipping bin.</summary>
        private readonly Farm Farm;

        /// <summary>The underlying shipping bin.</summary>
        private readonly NetCollection<Item> ShippingBin;

        /// <summary>The callback to invoke when an item is selected in the player inventory.</summary>
        private ItemGrabMenu.behaviorOnItemSelect GrabItemFromInventory => this.GrabItemFromInventoryImpl;

        /// <summary>The callback to invoke when an item is selected in the storage container.</summary>
        private ItemGrabMenu.behaviorOnItemSelect GrabItemFromContainer => this.GrabItemFromContainerImpl;


        /*********
        ** Accessors
        *********/
        /// <summary>The underlying inventory.</summary>
        public IList<Item> Inventory => this.ShippingBin;

        /// <summary>The container's name.</summary>
        public string Name { get; set; }

        /// <summary>Whether the player can configure the container.</summary>
        public bool IsEditable { get; } = false;

        /// <summary>Whether to enable chest-specific UI.</summary>
        public bool IsChest { get; } = false;

        /// <summary>The container's original name.</summary>
        public string DefaultName => null;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="farm">The farm whose shipping bin to manage.</param>
        public ShippingBinContainer(Farm farm)
        {
            this.Farm = farm;
            this.ShippingBin = farm.shippingBin;
        }

        /// <summary>Get whether the container has its default name.</summary>
        public bool HasDefaultName()
        {
            return true; // name isn't editable
        }

        /// <summary>Get whether the inventory can accept the item type.</summary>
        /// <param name="item">The item.</param>
        public bool CanAcceptItem(Item item)
        {
            return Utility.highlightShippableObjects(item);
        }

        /// <summary>Get whether another instance wraps the same underlying container.</summary>
        /// <param name="container">The other container.</param>
        public bool IsSameAs(IContainer container)
        {
            return container != null && this.IsSameAs(container.Inventory);
        }

        /// <summary>Get whether another instance wraps the same underlying container.</summary>
        /// <param name="inventory">The other container's inventory.</param>
        public bool IsSameAs(IList<Item> inventory)
        {
            return object.ReferenceEquals(this.Inventory, inventory);
        }

        /// <summary>Open a menu to transfer items between the player's inventory and this chest.</summary>
        /// <remarks>Derived from <see cref="StardewValley.Objects.Chest.updateWhenCurrentLocation"/>.</remarks>
        public ItemGrabMenu OpenMenu()
        {
            return new ItemGrabMenu(
                inventory: this.Inventory,
                reverseGrab: false,
                showReceivingMenu: true,
                highlightFunction: this.CanAcceptItem,
                behaviorOnItemSelectFunction: this.GrabItemFromInventory,
                message: null,
                behaviorOnItemGrab: this.GrabItemFromContainer,
                canBeExitedWithKey: true,
                showOrganizeButton: true,
                context: this.Farm
            );
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
            // normalise
            if (item.Stack == 0)
                item.Stack = 1;

            // add to shipping bin
            this.ShippingBin.Filter(p => p != null);
            foreach (Item slot in this.Inventory)
            {
                if (!slot.canStackWith(item))
                    continue;

                item.Stack = slot.addToStack(item.Stack);
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
            Game1.activeClickableMenu = menu = this.OpenMenu();
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

            this.ShippingBin.Remove(item);
            this.ShippingBin.Filter(p => p != null);
            if (item == this.Farm.lastItemShipped)
                this.Farm.lastItemShipped = this.Farm.shippingBin.LastOrDefault();
        }
    }
}
