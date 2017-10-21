using System.Collections.Generic;
using StardewValley;
using SFarmer = StardewValley.Farmer;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.Containers
{
    /// <summary>Represents an in-game object which can store items.</summary>
    internal interface IContainer
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The underlying inventory.</summary>
        List<Item> Inventory { get; }

        /// <summary>The container's name.</summary>
        string Name { get; set; }

        /// <summary>Whether the player can configure the container.</summary>
        bool IsEditable { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get whether the in-game container is open.</summary>
        bool IsOpen();

        /// <summary>Get whether the container has its default name.</summary>
        bool HasDefaultName();

        /// <summary>Add an item to the container from the player inventory.</summary>
        /// <param name="item">The item taken.</param>
        /// <param name="player">The player taking the item.</param>
        void GrabItemFromInventory(Item item, SFarmer player);

        /// <summary>Add an item to the player inventory from the container.</summary>
        /// <param name="item">The item taken.</param>
        /// <param name="player">The player taking the item.</param>
        void GrabItemFromContainer(Item item, SFarmer player);

        /// <summary>Get whether the specified object is equal to the current object.</summary>
        /// <param name="obj">The object to compare with the current object. </param>
        bool Equals(object obj);

        /// <summary>Serves as the default hash function.</summary>
        int GetHashCode();
    }
}
