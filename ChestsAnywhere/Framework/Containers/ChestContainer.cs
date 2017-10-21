using System.Collections.Generic;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using SFarmer = StardewValley.Farmer;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.Containers
{
    /// <summary>A storage container for an in-game chest.</summary>
    internal class ChestContainer : IContainer
    {
        /*********
        ** Properties
        *********/
        /// <summary>The in-game chest.</summary>
        private readonly Chest Chest;


        /*********
        ** Accessors
        *********/
        /// <summary>The underlying inventory.</summary>
        public List<Item> Inventory => this.Chest.items;

        /// <summary>The container's name.</summary>
        public string Name
        {
            get => this.Chest.Name;
            set => this.Chest.name = value;
        }

        /// <summary>Whether the player can configure the container.</summary>
        public bool IsEditable { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="chest">The in-game chest.</param>
        /// <param name="isEditable">Whether the player can configure the container.</param>
        public ChestContainer(Chest chest, bool isEditable = true)
        {
            this.Chest = chest;
            this.IsEditable = isEditable;
        }

        /// <summary>Get whether the in-game container is open.</summary>
        public virtual bool IsOpen()
        {
            return this.Chest.currentLidFrame == 135;
        }

        /// <summary>Get whether the container has its default name.</summary>
        public virtual bool HasDefaultName()
        {
            return this.Name == "Chest";
        }

        /// <summary>Get whether the inventory can accept the item type.</summary>
        /// <param name="item">The item.</param>
        public bool CanAcceptItem(Item item)
        {
            return InventoryMenu.highlightAllItems(item);
        }

        /// <summary>Add an item to the container from the player inventory.</summary>
        /// <param name="item">The item taken.</param>
        /// <param name="player">The player taking the item.</param>
        public void GrabItemFromInventory(Item item, SFarmer player)
        {
            this.Chest.grabItemFromInventory(item, player);
        }

        /// <summary>Add an item to the player inventory from the container.</summary>
        /// <param name="item">The item taken.</param>
        /// <param name="player">The player taking the item.</param>
        public void GrabItemFromContainer(Item item, SFarmer player)
        {
            this.Chest.grabItemFromChest(item, player);
        }

        /// <summary>Get whether the specified object is equal to the current object.</summary>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            return this.Chest.Equals((obj as ChestContainer)?.Chest);
        }

        /// <summary>Serves as the default hash function.</summary>
        public override int GetHashCode()
        {
            return this.Chest.GetHashCode();
        }
    }
}
