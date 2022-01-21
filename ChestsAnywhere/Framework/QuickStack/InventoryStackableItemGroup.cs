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
        public InventoryStackableItemGroup(IList<Item> refInventory, Item item) : base(refInventory, item)
        {
        }

        /// <inheritdoc/>
        public override List<int> GetIndexes()
        {
            var union = this.InventoryIndexesOfStackableItemStackNotFull.Union(this.InventoryIndexesOfStackableItemStackFull);
            var list = new HashSet<int>(union).ToList();
            return list;
        }

        /// <summary>
        /// Removes the index, i.e. the item has disappeared from the inventory
        /// </summary>
        /// <param name="index"></param>
        public void RemoveIndex(int index)
        {
            this.InventoryIndexesOfStackableItemStackFull.RemoveAll(x => x == index);
            this.InventoryIndexesOfStackableItemStackNotFull.RemoveAll(x => x == index);
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
                var inventoryItem = refInventory[i];
                if (inventoryItem != null && inventoryItem.maximumStackSize() > 1)
                {
                    bool stackableGroupForItemAlreadyExists = false;
                    foreach (var group in itemGroups)
                    {
                        var groupRefItem = group.GetGroupRepresentativeItem();
                        if (groupRefItem != null && inventoryItem.canStackWith(groupRefItem))
                        {
                            group.AddIndex(i);
                            stackableGroupForItemAlreadyExists = true;
                            break;
                        }
                    }
                    if (!stackableGroupForItemAlreadyExists)
                    {
                        var newStackableGroup = new InventoryStackableItemGroup(refInventory, inventoryItem);
                        newStackableGroup.AddIndex(i);
                        itemGroups.Add(newStackableGroup);
                    }
                }
            }
            return itemGroups;
        }

        public override bool ItemBelongsToGroup(Item item)
        {
            var groupRepresentative = this.GetGroupRepresentativeItem();
            if(groupRepresentative != null)
            {
                return groupRepresentative.canStackWith(item);
            }
            return false;
        }

        protected override void AddIndex(int index)
        {
            var item = this.GetItemAtIndex(index);
            if(item != null)
            {
                if (item.Stack >= item.maximumStackSize())
                {
                    if(!this.InventoryIndexesOfStackableItemStackFull.Contains(index))
                        this.InventoryIndexesOfStackableItemStackFull.Add(index);
                    this.InventoryIndexesOfStackableItemStackNotFull.RemoveAll(x => x == index);
                }
                else
                {
                    if (!this.InventoryIndexesOfStackableItemStackNotFull.Contains(index))
                        this.InventoryIndexesOfStackableItemStackNotFull.Add(index);
                    this.InventoryIndexesOfStackableItemStackFull.RemoveAll(x => x == index);
                }
            }
        }
    }
}
