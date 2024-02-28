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

        /// <summary>Whether to show the chest color picker.</summary>
        private readonly bool ShowColorPicker;


        /*********
        ** Accessors
        *********/
        /// <inheritdoc />
        public IList<Item?> Inventory => this.Chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID);

        /// <inheritdoc />
        public ContainerData Data { get; }

        /// <inheritdoc />
        public bool CanConfigureAutomate => this.Chest.SpecialChestType != Chest.SpecialChestTypes.JunimoChest && this.Chest.SpecialChestType != Chest.SpecialChestTypes.MiniShippingBin;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="chest">The in-game chest.</param>
        /// <param name="context">The <see cref="ItemGrabMenu.context"/> value which indicates what opened the menu.</param>
        /// <param name="showColorPicker">Whether to show the chest color picker.</param>
        public ChestContainer(Chest chest, object context, bool showColorPicker)
        {
            this.Chest = chest;
            this.Context = context;
            this.ShowColorPicker = showColorPicker;
            this.Data = new ContainerData(chest.modData);
        }

        /// <inheritdoc />
        public bool CanAcceptItem(Item item)
        {
            return InventoryMenu.highlightAllItems(item);
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

            Game1.activeClickableMenu = menu;
            return menu;
        }

        /// <inheritdoc />
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
                itemGrabMenu.behaviorFunction = this.GrabItemFromPlayer;
            }
        }
    }
}
