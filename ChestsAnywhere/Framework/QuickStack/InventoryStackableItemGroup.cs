using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.QuickStack
{
    internal class InventoryStackableItemGroup : InventoryItemGroup
    {
        /// <summary>
        /// Set of indexes of stackable items in inventory that are not at max stack size yet
        /// </summary>
        public readonly List<int> InventoryIndexesOfStackableItemStackNotFull = new();

        /// <summary>
        /// Set of indexes of stackable items in inventory that are at max stack size
        /// </summary>
        public readonly List<int> InventoryIndexesOfStackableItemStackFull = new();

        /// <summary>
        /// Determines a stackable item group in the given inventory by having an item group representative
        /// </summary>
        public InventoryStackableItemGroup(IList<Item> refInventory, Item item) : base(refInventory)
        {
            this.DetermineIndexesForItem(item);
        }

        /// <summary>
        /// Initializes empty group
        /// </summary>
        /// <param name="refInventory"></param>
        public InventoryStackableItemGroup(IList<Item> refInventory) : base(refInventory)
        {
        }

        /// <summary>
        /// Adds or removes the given index to the group
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public void HandleItemWithIndex(Item item, int index)
        {
            if (item != null)
            {
                if (item.Stack >= item.maximumStackSize())
                {
                    this.AddItemIndexFull(index);
                }
                else if(item.Stack <= 0)
                {
                    this.RemoveIndex(index);
                }
                else
                {
                    this.AddItemIndexNotFull(index);
                }
            }
        }

        private void RemoveIndex(int index)
        {
            this.InventoryIndexesOfStackableItemStackFull.RemoveAll(x => x == index);
            this.InventoryIndexesOfStackableItemStackNotFull.RemoveAll(x => x == index);
        }

        /// <summary>
        /// Determines all items that are stackable with the given item in the given inventory and returns their indexes
        /// </summary>
        /// <param name="inventory"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private void DetermineIndexesForItem(Item item)
        {
            if (item != null)
            {
                for (int i = 0; i < this.RefInventory.Count; i++)
                {
                    var inventoryItem = this.RefInventory[i];
                    if (item.canStackWith(inventoryItem))
                    {
                        this.HandleItemWithIndex(inventoryItem, i);
                    }
                }
            }
        }

        /// <summary>
        /// Returns a group representative of this stackable item group
        /// </summary>
        /// <returns></returns>
        public Item GetGroupRepresentativeItem()
        {
            var items = this.GetIndexes();
            if (items.Count > 0)
            {
                return this.RefInventory[items.First()];
            }
            return null;
        }

        /// <inheritdoc/>
        public override List<int> GetIndexes()
        {
            var union = this.InventoryIndexesOfStackableItemStackNotFull.Union(this.InventoryIndexesOfStackableItemStackFull);
            var list = new List<int>(union);
            return list;
        }

        /// <summary>
        /// Adds the given item index to the group as not full stack
        /// </summary>
        /// <param name="index"></param>
        private void AddItemIndexNotFull(int index)
        {
            this.InventoryIndexesOfStackableItemStackFull.RemoveAll(x => x == index);
            this.InventoryIndexesOfStackableItemStackNotFull.Add(index);
        }

        /// <summary>
        /// Adds the given item index to the group as full stack
        /// </summary>
        /// <param name="index"></param>
        private void AddItemIndexFull(int index)
        {
            this.InventoryIndexesOfStackableItemStackNotFull.RemoveAll(x => x == index);
            this.InventoryIndexesOfStackableItemStackFull.Add(index);
        }

        /// <summary>
        /// Returns the total summed number of stacks of the item group
        /// </summary>
        /// <returns></returns>
        public int GetTotalStackNumber()
        {
            int totalStacks = 0;
            foreach (int index in this.GetIndexes())
            {
                var item = this.RefInventory[index];
                if (item != null)
                {
                    totalStacks += item.Stack;
                }
            }
            return totalStacks;
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
        /// Determines all stackable item groups in the given inventory
        /// </summary>
        /// <param name="refInventory"></param>
        /// <returns></returns>
        public static List<InventoryStackableItemGroup> DetermineStackableItemGroups(IList<Item> refInventory)
        {
            List<InventoryStackableItemGroup> itemGroups = new();
            for (int i = 0; i < refInventory.Count; i++)
            {
                var refItem = refInventory[i];
                if (refItem != null && refItem.maximumStackSize() > 1)
                {
                    bool stackableGroupForItemAlreadyExists = false;
                    foreach (var group in itemGroups)
                    {
                        var groupRefItem = group.GetGroupRepresentativeItem();
                        if (groupRefItem != null && refItem.canStackWith(groupRefItem))
                        {
                            group.HandleItemWithIndex(refItem, i);
                            stackableGroupForItemAlreadyExists = true;
                            break;
                        }
                    }
                    if (!stackableGroupForItemAlreadyExists)
                    {
                        var newStackableGroup = new InventoryStackableItemGroup(refInventory);
                        newStackableGroup.HandleItemWithIndex(refItem, i);
                        itemGroups.Add(newStackableGroup);
                    }
                }
            }
            return itemGroups;
        }
    }
}
