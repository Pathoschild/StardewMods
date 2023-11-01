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
        /// <param name="reflection">Simplifies access to private code.</param>
        public ChestContainer(Chest chest, object context, bool showColorPicker, IReflectionHelper reflection)
        {
            this.Chest = chest;
            this.Context = context;
            this.ShowColorPicker = showColorPicker;
            this.Reflection = reflection;
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
            this.Chest.ShowMenu();

            if (!this.ShowColorPicker && Game1.activeClickableMenu is ItemGrabMenu menu) // disable color picker for some special cases like the shipping bin, which can't be recolored
            {
                menu.chestColorPicker = null;
                menu.colorPickerToggleButton = null;
            }

            return Game1.activeClickableMenu;
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
                this.Reflection.GetField<ItemGrabMenu.behaviorOnItemSelect>(itemGrabMenu, "behaviorFunction").SetValue(this.GrabItemFromPlayer);
            }
        }
    }
}
