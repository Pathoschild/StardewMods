using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.QuickStack
{
    /// <summary>
    /// Class to hold information about a chest containing a certain item
    /// </summary>
    internal class ChestInventoryItemMetaData
    {
        /// <summary>
        /// The chest holding the type of item
        /// </summary>
        public readonly ManagedChest Chest;

        /// <summary>
        /// stackable item group in chest inventory
        /// </summary>
        public readonly InventoryStackableItemGroup ItemGroup;
    
        public ChestInventoryItemMetaData(Item refItem, ManagedChest chest)
        {
            this.Chest = chest;
            this.ItemGroup = new InventoryStackableItemGroup(chest.Container.Inventory, refItem);
            this.ItemGroup.DetermineGroupInThisInventory();
        }

        /// <summary>
        /// Returns true if the chest is full for the stackable item group.
        /// Returns false otherwise.
        /// </summary>
        /// <returns>Whether the chest is full for the stackable item group</returns>
        public bool IsFull()
        {
            return this.ItemGroup.GetInventoryCount() == this.Chest.Container.ActualCapacity && this.ItemGroup.InventoryIndexesOfStackableItemStackNotFull.Count == 0;
        }
    }
}
