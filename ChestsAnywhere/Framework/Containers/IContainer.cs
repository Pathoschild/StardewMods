using System.Collections.Generic;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.Containers
{
    /// <summary>An in-game container which can store items.</summary>
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

        /// <summary>Whether to enable chest-specific UI.</summary>
        bool IsChest { get; }

        /// <summary>The container's original name.</summary>
        string DefaultName { get; }

        /// <summary>The callback to invoke when an item is selected in the player inventory.</summary>
        ItemGrabMenu.behaviorOnItemSelect GrabItemFromInventory { get; }

        /// <summary>The callback to invoke when an item is selected in the storage container.</summary>
        ItemGrabMenu.behaviorOnItemSelect GrabItemFromContainer { get; } // must be a delegate for compatibility with other mods which get `delegate.Target as Chest`


        /*********
        ** Public methods
        *********/
        /// <summary>Get whether the in-game container is open.</summary>
        bool IsOpen();

        /// <summary>Get whether the container has its default name.</summary>
        bool HasDefaultName();

        /// <summary>Get whether the inventory can accept the item type.</summary>
        /// <param name="item">The item.</param>
        bool CanAcceptItem(Item item);

        /// <summary>Get whether another instance wraps the same underlying container.</summary>
        /// <param name="container">The other container.</param>
        bool IsSameAs(IContainer container);

        /// <summary>Get whether another instance wraps the same underlying container.</summary>
        /// <param name="inventory">The other container's inventory.</param>
        bool IsSameAs(List<Item> inventory);
    }
}
