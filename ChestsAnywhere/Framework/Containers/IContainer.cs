using System.Collections.Generic;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.Containers
{
    /// <summary>Represents an in-game object which can store items.</summary>
    internal interface IContainer
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The in-game container instance.</summary>
        object Instance { get; }

        /// <summary>The container's name.</summary>
        string Name { get; set; }

        /// <summary>The items in the storage container.</summary>
        List<Item> Items { get; }

        /// <summary>The callback to invoke when an item is selected in the player inventory.</summary>
        ItemGrabMenu.behaviorOnItemSelect GrabItemFromInventory { get; }

        /// <summary>The callback to invoke when an item is selected in the storage container.</summary>
        ItemGrabMenu.behaviorOnItemSelect GrabItemFromContainer { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get whether the in-game container is open.</summary>
        bool IsOpen();

        /// <summary>Get whether the container has its default name.</summary>
        bool HasDefaultName();

        /// <summary>Get whether the specified object is equal to the current object.</summary>
        /// <param name="obj">The object to compare with the current object. </param>
        bool Equals(object obj);

        /// <summary>Serves as the default hash function.</summary>
        int GetHashCode();
    }
}
