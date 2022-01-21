using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.QuickStack
{
    internal class InventorySameNameGroup : InventoryItemGroup
    {
        private readonly HashSet<int> Indexes;

        private readonly Item RefItem;

        /// <summary>
        /// At initializing, determines whole group for the given inventory
        /// </summary>
        /// <param name="refInventory"></param>
        /// <param name="refItem"></param>
        public InventorySameNameGroup(IList<Item> refInventory, Item refItem) : base(refInventory, refItem)
        {
            this.Indexes = new();
        }

        public override List<int> GetIndexes()
        {
            return this.Indexes.ToList();
        }

        public override bool ItemBelongsToGroup(Item item)
        {
            if(item != null)
            {
                return this.RefItem.Name == item.Name;
            }
            return false;
        }

        protected override void AddIndex(int index)
        {
            this.Indexes.Add(index);
        }
    }
}
