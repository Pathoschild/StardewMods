using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.QuickStack
{
    internal class ScrollChestItemsCommand : InventoryToChestCommand
    {


        public ScrollChestItemsCommand(List<ManagedChest> availableChests, Farmer player, IClickableMenu menu) : base(availableChests, player, menu)
        {
        }
    }
}
