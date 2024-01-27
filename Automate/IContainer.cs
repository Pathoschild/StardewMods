using System;
using System.Collections.Generic;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.Mods;

namespace Pathoschild.Stardew.Automate
{
    /// <summary>Provides and stores items for machines.</summary>
    public interface IContainer : IAutomatable, IEnumerable<ITrackedStack>
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The container name (if any).</summary>
        string Name { get; }

        /// <summary>The raw mod data for the container.</summary>
        ModDataDictionary ModData { get; }

        /// <summary>Whether this is a Junimo chest, which shares a global inventory with all other Junimo chests.</summary>
        bool IsJunimoChest { get; }

        /// <summary>An object instance which uniquely identifies the underlying inventory.</summary>
        /// <remarks>Normal chests should have a unique instance, while chests with shared inventories (like Junimo Chests) should share this instance too. This isn't necessarily the inventory itself, though that's the default implementation.</remarks>
        object InventoryReferenceId { get; }

        /// <summary>The underlying inventory.</summary>
        IInventory Inventory { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Find items in the pipe matching a predicate.</summary>
        /// <param name="predicate">Matches items that should be returned.</param>
        /// <param name="count">The number of items to find.</param>
        /// <returns>If the pipe has no matching item, returns <c>null</c>. Otherwise returns a tracked item stack, which may have less items than requested if no more were found.</returns>
        ITrackedStack? Get(Func<Item, bool> predicate, int count);

        /// <summary>Store an item stack.</summary>
        /// <param name="stack">The item stack to store.</param>
        /// <remarks>If the storage can't hold the entire stack, it should reduce the tracked stack accordingly.</remarks>
        void Store(ITrackedStack stack);

        /// <summary>Get the number of item stacks currently stored in the container.</summary>
        int GetFilled();

        /// <summary>Get the total number of item stacks that can be stored in the container.</summary>
        int GetCapacity();
    }
}
