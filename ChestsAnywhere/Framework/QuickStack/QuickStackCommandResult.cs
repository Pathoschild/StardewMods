using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.QuickStack
{
    /// <summary>
    /// Class to hold information about executed quick stacking
    /// </summary>
    internal class QuickStackCommandResult
    {
        public readonly List<int> InventoryIndexesItemMovedToChest;

        public QuickStackCommandResult(IEnumerable<int> inventoryIndexesItemMovedToChest)
        {
            var result = inventoryIndexesItemMovedToChest ?? throw new ArgumentNullException(nameof(inventoryIndexesItemMovedToChest));
            var list = result.ToList();
            list.Sort();
            this.InventoryIndexesItemMovedToChest = list;
        }

        public bool ItemsHaveBeenMoved()
        {
            return this.InventoryIndexesItemMovedToChest.Count > 0;
        }
    }
}
