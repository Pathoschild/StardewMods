using System.Collections.Generic;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.Containers
{
    /// <summary>A storage container for an in-game chest.</summary>
    internal class ChestContainer : IContainer
    {
        /*********
        ** Properties
        *********/
        /// <summary>The container's default internal name.</summary>
        private readonly string DefaultName = "Chest";

        /// <summary>The in-game chest.</summary>
        private readonly Chest Chest;

        /// <summary>The <see cref="ItemGrabMenu.context"/> value which indicates what opened the menu.</summary>
        private readonly object Context;

        /// <summary>The callback to invoke when an item is selected in the player inventory.</summary>
        private ItemGrabMenu.behaviorOnItemSelect GrabItemFromInventory => this.Chest.grabItemFromInventory;

        /// <summary>The callback to invoke when an item is selected in the storage container.</summary>
        private ItemGrabMenu.behaviorOnItemSelect GrabItemFromContainer => this.Chest.grabItemFromChest;


        /*********
        ** Accessors
        *********/
        /// <summary>The underlying inventory.</summary>
        public IList<Item> Inventory => this.Chest.items;

        /// <summary>The persisted data for this container.</summary>
        public ContainerData Data { get; }

        /// <summary>Whether the player can customise the container data.</summary>
        public bool IsDataEditable { get; } = true;

        /// <summary>Whether Automate options can be configured for this chest.</summary>
        public bool CanConfigureAutomate { get; } = true;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="chest">The in-game chest.</param>
        /// <param name="context">The <see cref="ItemGrabMenu.context"/> value which indicates what opened the menu.</param>
        public ChestContainer(Chest chest, object context)
        {
            this.Chest = chest;
            this.Context = context;
            this.Data = ContainerData.ParseName(chest.Name, this.DefaultName);
        }

        /// <summary>Get whether the inventory can accept the item type.</summary>
        /// <param name="item">The item.</param>
        public bool CanAcceptItem(Item item)
        {
            return InventoryMenu.highlightAllItems(item);
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
                source: ItemGrabMenu.source_chest,
                context: this.Context
            );
        }

        /// <summary>Persist the container data.</summary>
        public void SaveData()
        {
            this.Chest.name = this.Data.HasData()
                ? this.Data.ToName()
                : this.DefaultName;
        }
    }
}
