using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.QuickStack
{
    internal abstract class InventoryToChestCommand
    {
        protected readonly List<ManagedChest> AvailableChests;

        protected readonly Farmer Player;

        protected readonly InventoryMenu OpenChestInventory;

        protected readonly InventoryMenu OwnInventoryPage;

        public InventoryToChestCommand(List<ManagedChest> availableChests, Farmer player, IClickableMenu menu)
        {
            this.AvailableChests = availableChests ?? throw new ArgumentNullException(nameof(availableChests));
            this.Player = player = player ?? throw new ArgumentNullException(nameof(player));
            if (menu is ItemGrabMenu itemGrabmenu)
            {
                this.OpenChestInventory = itemGrabmenu.ItemsToGrabMenu;
                this.OwnInventoryPage = itemGrabmenu.inventory;
            }
            else if (menu is MenuWithInventory menuWithInventory)
            {
                this.OwnInventoryPage = menuWithInventory.inventory;
            }
        }

        /// <summary>
        /// Shakes the given item index if given chest inventory is the one of the open chest
        /// </summary>
        /// <param name="chestItemIndex"></param>
        /// <param name="chestInventory"></param>
        protected void ShakeChestItemIfIsDisplayedChest(int chestItemIndex, IList<Item> chestInventory)
        {
            if (this.OpenChestInventory != null && this.OpenChestInventory.actualInventory == chestInventory)
            {
                this.OpenChestInventory.ShakeItem(chestItemIndex);
            }
        }

        /// <summary>
        /// Shakes the item at the given index in the inventory
        /// </summary>
        /// <param name="inventoryItemIndex"></param>
        protected void ShakeInventoryItem(int inventoryItemIndex)
        {
            this.OwnInventoryPage.ShakeItem(inventoryItemIndex);
        }
    }
}
