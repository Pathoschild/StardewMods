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
        public readonly Dictionary<int, List<ManagedChest>> InventoryIndexToMovedChests = new();

        public readonly Dictionary<int, List<ManagedChest>> FullyMovedInventoryIndexToMovedChests = new();
    }
}
