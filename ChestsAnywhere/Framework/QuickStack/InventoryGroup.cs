using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.QuickStack
{
    /// <summary>
    /// Class to hold information about any group of items for a given inventory
    /// </summary>
    internal abstract class InventoryGroup
    {
        /// <summary>
        /// The inventory where the item group resides
        /// </summary>
        protected readonly IList<Item> RefInventory;

        protected InventoryGroup(IList<Item> refInventory)
        {
            this.RefInventory = refInventory ?? throw new ArgumentNullException(nameof(refInventory));
        }

        /// <summary>
        /// Logic how this item group determines any given item belonging to this group
        /// </summary>
        /// <returns></returns>
        public abstract bool ItemBelongsToGroup(Item item);

        public IList<Item> GetInventory()
        {
            return this.RefInventory;
        }

        /// <summary>
        /// Returns the size of the inventory
        /// </summary>
        /// <returns></returns>
        public int GetInventoryCount()
        {
            return this.RefInventory.Count;
        }

        /// <summary>
        /// Determines the item group in this inventory defined by the given item
        /// </summary>
        /// <param name="inventory"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public void DetermineGroupInThisInventory()
        {
            for (int i = 0; i < this.RefInventory.Count; i++)
            {
                this.TryAddItem(i);
            }
        }

        /// <summary>
        /// Adds the item at the given index to the inventory group, if it belongs to the group.
        /// Returns true if it has been added and belongs to the group.
        /// Returns false otherwise.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public bool TryAddItem(int index)
        {
            if (0 <= index && index <= this.RefInventory.Count)
            {
                var item = this.RefInventory[index];
                if (this.ItemBelongsToGroup(item))
                {
                    this.AddIndex(index);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Adds the item at the given index to the group
        /// </summary>
        /// <param name="index"></param>
        protected abstract void AddIndex(int index);

        /// <summary>
        /// Returns all indexes of items in the group
        /// </summary>
        /// <returns></returns>
        public abstract List<int> GetIndexes();

        /// <summary>
        /// Returns a list of all items indicated by the given indexes
        /// </summary>
        /// <returns></returns>
        public List<Item> GetItems()
        {
            List<Item> items = new();
            foreach (int i in this.GetIndexes())
            {
                var item = this.RefInventory[i];
                items.Add(item);
            }
            return items;
        }

        /// <summary>
        /// Returns the item at the given index of the inventory if index is within bounds
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Item GetItemAtIndex(int index)
        {
            if (0 <= index && index < this.RefInventory.Count)
            {
                var item = this.RefInventory[index];
                return item;
            }
            return null;
        }

        /// <summary>
        /// Returns true if there are no indexes in this group
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return this.GetIndexes().Count == 0;
        }

        /// <summary>
        /// Returns the total summed number of stacks of the item group
        /// </summary>
        /// <returns></returns>
        public int GetTotalStackNumber()
        {
            int totalStacks = 0;
            foreach (var item in this.GetItems())
            {
                if (item != null)
                {
                    totalStacks += item.Stack;
                }
            }
            return totalStacks;
        }
    }
}
