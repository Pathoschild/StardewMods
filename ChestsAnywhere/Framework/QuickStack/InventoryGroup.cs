using System;
using System.Collections.Generic;
using StardewValley;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.QuickStack
{
    /// <summary>
    /// Class to hold information about any group of items in a given inventory
    /// </summary>
    internal abstract class InventoryItemGroup
    {
        /// <summary>
        /// The inventory where the item group resides
        /// </summary>
        public readonly IList<Item> RefInventory;

        public InventoryItemGroup(IList<Item> refInventory)
        {
            this.RefInventory = refInventory ?? throw new ArgumentNullException(nameof(refInventory));
        }

        /// <summary>
        /// Returns all indexes of items in the group
        /// </summary>
        /// <returns></returns>
        public abstract List<int> GetIndexes();

        /// <summary>
        /// Returns a list of all non null items indicated by the given indexes
        /// </summary>
        /// <returns></returns>
        public List<Item> GetNonNullItems()
        {
            List<Item> nonNullItems = new();
            foreach (int i in this.GetIndexes())
            {
                var item = this.RefInventory[i];
                if (item != null)
                {
                    nonNullItems.Add(item);
                }
            }
            return nonNullItems;
        }
    }
}
