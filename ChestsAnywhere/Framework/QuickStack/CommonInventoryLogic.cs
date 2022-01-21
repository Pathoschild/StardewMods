using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewValley;
using static StardewValley.Menus.ItemGrabMenu;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.QuickStack
{
    internal class CommonInventoryLogic
    {
        private static readonly List<TransferredItemSprite> TransferredItemSprites = new();

        /// <summary>
        /// Determines all possible item groups in the list of given chests for every given item group in the given inventory
        /// </summary>
        /// <param name="inventory"></param>
        /// <param name="itemGroupsInInventory"></param>
        /// <param name="chestsToBeSearched"></param>
        /// <returns></returns>
        public static List<Tuple<InventoryStackableItemGroup, List<ChestInventoryItemMetaData>>> GetMappingBetweenInventoryGroupsAndChestGroups(IList<Item> inventory, List<InventoryStackableItemGroup> itemGroupsInInventory, List<ManagedChest> chestsToBeSearched)
        {
            List<Tuple<InventoryStackableItemGroup, List<ChestInventoryItemMetaData>>> stackablgeGroupsToChests = new();

            // check for every group if a chest is relevant
            foreach (var inventoryGroup in itemGroupsInInventory)
            {
                var groupRepresentativeItem = inventoryGroup.GetGroupRepresentativeItem();
                if(groupRepresentativeItem == null)
                {
                    continue;
                }
                var chestsItemsCanBeMovedTo = new List<ChestInventoryItemMetaData>();
                // check every available chest if items in the group can be pushed to it
                foreach (var chest in chestsToBeSearched)
                {
                    var container = chest.Container;
                    if (!container.CanAcceptItem(groupRepresentativeItem))
                    {
                        continue;
                    }
                    var chestMetaData = new ChestInventoryItemMetaData(groupRepresentativeItem, chest);
                    // Only if stackable items in the chest are found can items be pushed there
                    if (!chestMetaData.ItemGroup.IsEmpty())
                    {
                        chestsItemsCanBeMovedTo.Add(chestMetaData);
                    }
                }
                stackablgeGroupsToChests.Add(new Tuple<InventoryStackableItemGroup, List<ChestInventoryItemMetaData>>(inventoryGroup, chestsItemsCanBeMovedTo));
            }
            return stackablgeGroupsToChests;
        }
    }
}
