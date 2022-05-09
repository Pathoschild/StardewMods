using System.Collections.Generic;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.Containers
{
    /// <summary>A storage container for an in-game chest.</summary>
    internal class ChestContainer : IContainer
    {
        /*********
        ** Fields
        *********/
        /// <summary>The in-game chest.</summary>
        internal readonly Chest Chest;

        /// <summary>The <see cref="ItemGrabMenu.context"/> value which indicates what opened the menu.</summary>
        private readonly object Context;

        /// <summary>Simplifies access to private code.</summary>
        private readonly IReflectionHelper Reflection;

        /// <summary>Whether to show the chest color picker.</summary>
        private readonly bool ShowColorPicker;


        /*********
        ** Accessors
        *********/
        /// <summary>The underlying inventory.</summary>
        public IList<Item?> Inventory => this.Chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID);

        /// <summary>The persisted data for this container.</summary>
        public ContainerData Data { get; }

        /// <summary>Whether Automate options can be configured for this chest.</summary>
        public bool CanConfigureAutomate => this.Chest.SpecialChestType != Chest.SpecialChestTypes.JunimoChest && this.Chest.SpecialChestType != Chest.SpecialChestTypes.MiniShippingBin;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="chest">The in-game chest.</param>
        /// <param name="context">The <see cref="ItemGrabMenu.context"/> value which indicates what opened the menu.</param>
        /// <param name="showColorPicker">Whether to show the chest color picker.</param>
        /// <param name="reflection">Simplifies access to private code.</param>
        public ChestContainer(Chest chest, object context, bool showColorPicker, IReflectionHelper reflection)
        {
            this.Chest = chest;
            this.Context = context;
            this.ShowColorPicker = showColorPicker;
            this.Reflection = reflection;
            this.Data = new ContainerData(chest.modData);
        }

        /// <summary>Get whether the inventory can accept the item type.</summary>
        /// <param name="item">The item.</param>
        public bool CanAcceptItem(Item item)
        {
            return InventoryMenu.highlightAllItems(item);
        }

        /// <summary>Get whether another instance wraps the same underlying container.</summary>
        /// <param name="container">The other container.</param>
        public bool IsSameAs(IContainer? container)
        {
            return
                container is not null
                && this.IsSameAs(container.Inventory);
        }

        /// <summary>Get whether another instance wraps the same underlying container.</summary>
        /// <param name="inventory">The other container's inventory.</param>
        public bool IsSameAs(IList<Item?>? inventory)
        {
            return
                inventory is not null
                && object.ReferenceEquals(this.Inventory, inventory);
        }

        /// <summary>Open a menu to transfer items between the player's inventory and this chest.</summary>
        /// <remarks>Derived from <see cref="StardewValley.Objects.Chest.updateWhenCurrentLocation"/>.</remarks>
        public IClickableMenu OpenMenu()
        {
            ItemGrabMenu menu = Constants.TargetPlatform switch
            {
                GamePlatform.Android => new ItemGrabMenu(
                    inventory: this.Inventory,
                    reverseGrab: true,
                    showReceivingMenu: true,
                    highlightFunction: this.CanAcceptItem,
                    behaviorOnItemSelectFunction: null,
                    message: null,
                    behaviorOnItemGrab: null,
                    canBeExitedWithKey: true,
                    showOrganizeButton: true,
                    source: ItemGrabMenu.source_chest,
                    sourceItem: this.Chest,
                    context: this.Context
                ),

                _ => new ItemGrabMenu(
                    inventory: this.Inventory,
                    reverseGrab: false,
                    showReceivingMenu: true,
                    highlightFunction: this.CanAcceptItem,
                    behaviorOnItemSelectFunction: this.GrabItemFromPlayer,
                    message: null,
                    behaviorOnItemGrab: this.GrabItemFromContainer,
                    canBeExitedWithKey: true,
                    showOrganizeButton: true,
                    source: ItemGrabMenu.source_chest,
                    sourceItem: this.Chest,
                    context: this.Context
                )
            };

            if (!this.ShowColorPicker) // disable color picker for some special cases like the shipping bin, which can't be recolored
            {
                menu.chestColorPicker = null;
                menu.colorPickerToggleButton = null;
            }

            return menu;
        }

        /// <summary>Persist the container data.</summary>
        public void SaveData()
        {
            this.Data.ToModData(this.Chest.modData);
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Transfer an item from a player to the container.</summary>
        /// <param name="item">The item to transfer.</param>
        /// <param name="player">The player transferring the item.</param>
        private void GrabItemFromPlayer(Item item, Farmer player)
        {
            this.Chest.grabItemFromInventory(item, player);
            this.OnChanged();
        }

        /// <summary>Transfer an item from the container to a player.</summary>
        /// <param name="item">The item to transfer.</param>
        /// <param name="player">The player transferring the item.</param>
        private void GrabItemFromContainer(Item item, Farmer player)
        {
            this.Chest.grabItemFromChest(item, player);
            this.OnChanged();
        }

        /// <summary>Update when an item is added/removed to the container.</summary>
        protected virtual void OnChanged()
        {
            if (Game1.activeClickableMenu is ItemGrabMenu itemGrabMenu)
            {
                itemGrabMenu.behaviorOnItemGrab = this.GrabItemFromContainer;
                this.Reflection.GetField<ItemGrabMenu.behaviorOnItemSelect>(itemGrabMenu, "behaviorFunction").SetValue(this.GrabItemFromPlayer);
            }
        }
    }
}
