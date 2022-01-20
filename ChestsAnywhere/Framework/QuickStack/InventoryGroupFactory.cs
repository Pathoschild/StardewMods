using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.QuickStack
{
    internal class InventoryGroupFactory
    {
        public static InventoryItemGroup GetEmptyInventoryGroupFactory<T>(IList<Item> refInventory, InventoryGroupType groupType)
        {
            switch(groupType)
            {
                case InventoryGroupType.StackableGroup:
                    return new InventoryStackableItemGroup(refInventory);
                case InventoryGroupType.SameNameGroup:
                    return null;
                    //return new InventorySameNameGroup(refInventory, new Tool("Axe"));
                default: throw new ArgumentOutOfRangeException($"Unknown how to instantiate inventory group with type {groupType}");
            }
        }
    }
}
