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
        /// Reference item for which information is held in this class
        /// </summary>
        public readonly Item ReferenceItem;

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
            this.ReferenceItem = refItem.getOne();
            this.Chest = chest;
            this.ItemGroup = new InventoryStackableItemGroup(chest.Container.Inventory, this.ReferenceItem);
        }

        public bool IsFull()
        {
            return this.ItemGroup.RefInventory.Count == this.Chest.Container.ActualCapacity && this.ItemGroup.InventoryIndexesOfStackableItemStackNotFull.Count == 0;
        }
    }
}
