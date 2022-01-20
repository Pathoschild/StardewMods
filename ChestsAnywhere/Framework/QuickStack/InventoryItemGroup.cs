using System;
using System.Collections.Generic;
using StardewValley;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.QuickStack
{
    /// <summary>
    /// Class to hold information about any group of items in a given inventory
    /// </summary>
    internal abstract class InventoryItemGroup : ItemGroup
    {
        /// <summary>
        /// The inventory where the item group resides
        /// </summary>
        protected readonly IList<Item> RefInventory;

        public InventoryItemGroup(IList<Item> refInventory)
        {
            this.RefInventory = refInventory ?? throw new ArgumentNullException(nameof(refInventory));
        }

        /// <summary>
        /// Determines all items in this inventory that belong to the group defined by the given item
        /// </summary>
        /// <param name="inventory"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        protected void DetermineIndexesForItem(Item item)
        {
            if (item != null)
            {
                for (int i = 0; i < this.RefInventory.Count; i++)
                {
                    var inventoryItem = this.RefInventory[i];
                    if (this.ItemBelongsToGroup(inventoryItem))
                    {
                        this.TryAddItem(i);
                    }
                }
            }
        }

        /// <summary>
        /// Adds the given item to the inventory group
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public abstract bool TryAddItem(int index);

        /// <summary>
        /// Returns the size of the inventory
        /// </summary>
        /// <returns></returns>
        public int GetInventoryCount()
        {
            return this.RefInventory.Count;
        }

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
            foreach(var item in this.GetItems())
            {
                if(item != null)
                {
                    totalStacks += item.Stack;
                }
            }
            return totalStacks;
        }

        /// <summary>
        /// Determines all item groups in the given inventory
        /// </summary>
        /// <param name="refInventory"></param>
        /// <returns></returns>
        public static List<T> DetermineAllItemGroupsInInventory<T>(IList<Item> refInventory) where T : InventoryItemGroup
        {
            List<T> itemGroups = new();
            for (int i = 0; i < refInventory.Count; i++)
            {
                var refItem = refInventory[i];
                if(refItem != null)
                {
                    bool groupForItemExists = false;
                    foreach (var group in itemGroups)
                    {
                        if (group.ItemBelongsToGroup(refItem))
                        {
                            group.AddItem(refItem, i);
                            groupForItemExists = true;
                            // don't break here as some implementations may allow an item to be in different item groups
                        }
                    }
                    if (!groupForItemExists)
                    {
                        //T.
                        //var newGroup = new T(refInventory);
                        //itemGroups.Add()
                        //newStackableGroup.HandleItemWithIndex(refItem, i);
                        //itemGroups.Add(newGroup);
                    }
                }
            }
            return itemGroups;
        }

        private void AddItem(Item refItem, int i)
        {
            throw new NotImplementedException();
        }
    }
}
