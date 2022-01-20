using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.QuickStack
{
    internal class InventorySameNameGroup : InventoryItemGroup
    {
        private readonly List<int> Indexes;

        private readonly Item RefItem;
        public InventorySameNameGroup(IList<Item> refInventory, Item refItem) : base(refInventory)
        {
            this.RefItem = refItem.getOne() ?? throw new ArgumentException(nameof(refItem));
            this.Indexes = new();
            this.DetermineIndexesForItem(this.RefItem);
        }

        public override List<int> GetIndexes()
        {
            return this.Indexes;
        }

        public override bool ItemBelongsToGroup(Item item)
        {
            if(item != null)
            {
                return this.RefItem.Name == item.Name;
            }
            return false;
        }

        public override bool TryAddItem(int index)
        {
            this.Indexes.Add(index);
            return true;
        }
    }
}
