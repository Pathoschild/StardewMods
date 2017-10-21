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

        /// <summary>The items in the storage container.</summary>
        public List<Item> Items => this.Chest.items;

        /// <summary>The callback to invoke when an item is selected in the player inventory.</summary>
        public ItemGrabMenu.behaviorOnItemSelect GrabItemFromInventory => this.Chest.grabItemFromInventory;

        /// <summary>The callback to invoke when an item is selected in the storage container.</summary>
        public ItemGrabMenu.behaviorOnItemSelect GrabItemFromContainer => this.Chest.grabItemFromChest;

        /// <summary>Whether the player can configure the container.</summary>
        public virtual bool IsEditable { get; } = true;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="chest">The in-game chest.</param>
        public ChestContainer(Chest chest)
        {
            this.Chest = chest;
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
