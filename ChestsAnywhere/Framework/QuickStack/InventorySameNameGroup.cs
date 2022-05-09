using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.QuickStack
{
    internal class InventorySameNameGroup : InventoryItemGroup
    {
        private readonly HashSet<int> Indexes;

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
                return this.GroupRepresentative.Name == item.Name;
            }
            return false;
        }

        protected override void AddIndex(int index)
        {
            this.Indexes.Add(index);
        }
    }
}
