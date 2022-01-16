using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pathoschild.Stardew.ChestsAnywhere.Framework.Containers;
using StardewValley;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.QuickStack
{

    internal class ChestInformation
    {
        public readonly ManagedChest Chest;

        public List<int> InventoryIndexOfStackableItemInChest;

        public int TotalStackNumberInChest;

        public ChestInformation(ManagedChest chest, int totalStackNumberInChest, List<int> inventoryIndexOfStackableItemInChest)
        {
            this.Chest = chest;
            this.TotalStackNumberInChest = totalStackNumberInChest;
            this.InventoryIndexOfStackableItemInChest = inventoryIndexOfStackableItemInChest;
        }
    }
    internal class QuickStackCommand
    {
        private readonly QuickStackConfig Config;

        private readonly List<ManagedChest> AvailableChests;

        private readonly Farmer Player;

        public QuickStackCommand(QuickStackConfig config, List<ManagedChest> availableChests, Farmer player)
        {
            this.Config = config ?? throw new ArgumentNullException(nameof(config));
            this.AvailableChests = availableChests ?? throw new ArgumentNullException(nameof(availableChests));
            this.Player = player = player ?? throw new ArgumentNullException(nameof(player));
        }
        public QuickStackCommandResult Execute()
        {
            var playerItems = this.Player.Items;
            var stackableGroupsToChestsItemsCanBeMovedTo = this.GetPlayerItemToChestMapping(playerItems);
            return this.MoveItemsToChests(stackableGroupsToChestsItemsCanBeMovedTo, playerItems);
        }

        private QuickStackCommandResult MoveItemsToChests(Dictionary<List<int>, List<ChestInformation>> itemGroupsToChestMapping, IList<Item> playerItems)
        {
            // Only select those item groups that can be moved to chests
            var itemGroupsThatCanBeMovedToChests = itemGroupsToChestMapping.Where(x => x.Value.Count > 0);
            // begin with those items that have only one chest they can be moved to
            var orderedPairs = itemGroupsThatCanBeMovedToChests.OrderBy(x => x.Value.Count);
            
            foreach (var pair in orderedPairs)
            {
                var possibleChestsToBeMovedTo = pair.Value;
                var stackableGroup = pair.Key;

                // begin with rightmost item of group
                var orderedStackableGroup = stackableGroup.OrderByDescending(i => i);
                // begin with chest that already has most of the given Item
                //var orderedPossibleChestsToBeMovedTo = possibleChestsToBeMovedTo.OrderBy(x => x.Container.)
                var orderedChests = possibleChestsToBeMovedTo.OrderByDescending(x => x.TotalStackNumberInChest);
                bool playerStillHasItemTypeInInventory = true;
                bool placeLeftInChests = true;
                while(playerStillHasItemTypeInInventory && placeLeftInChests)
                {
                    foreach (int inventoryItemIndex in orderedStackableGroup)
                    {
                        var inventoryItem = playerItems[inventoryItemIndex];
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Determines which item groups (stackable items) in inventory can be put into which chests
        /// </summary>
        private Dictionary<List<int>, List<ChestInformation>> GetPlayerItemToChestMapping(IList<Item> playerItems)
        {
            Dictionary<List<int>, List<ChestInformation>> stackablgeGroupsToChests = new();
            var stackableGroupsInInventory = GetStackableGroupsWithinItemList(playerItems);

            // check for every group if a chest is relevant
            // could be optimized
            foreach(var group in stackableGroupsInInventory)
            {
                var groupRepresentativeItem = playerItems[group[0]];
                var chestsItemsCanBeMovedTo = new List<ChestInformation>();
                foreach(var chest in this.AvailableChests)
                {
                    var container = chest.Container;
                    if((!this.Config.ConsiderJunimoHuts && container is JunimoHutContainer)
                       || (!this.Config.ConsiderShippingBin && container is ShippingBinContainer)
                       || (!this.Config.ConsiderAutoGrabber && container is AutoGrabberContainer)
                       || chest.Container is StorageFurnitureContainer)
                    {
                        continue;
                    }
                    if (!container.CanAcceptItem(groupRepresentativeItem))
                    {
                        continue;
                    }
                    if(!this.Config.MoveItemsToHiddenChests && chest.IsIgnored)
                    {
                        continue;
                    }
                    int numStacksOfItemFoundInChest = 0;
                    bool itemFoundInChest = false;
                    List<int> indexFoundItemsInChest = new List<int>();
                    for(int i = 0; i < container.Inventory.Count; i++)
                    {
                        var chestItem = container.Inventory[i];
                        if (groupRepresentativeItem.canStackWith(chestItem))
                        {
                            indexFoundItemsInChest.Add(i);
                            numStacksOfItemFoundInChest += chestItem.Stack;
                            itemFoundInChest = true;
                        }
                    }
                    if (itemFoundInChest)
                    {
                        chestsItemsCanBeMovedTo.Add(new ChestInformation(chest, numStacksOfItemFoundInChest, indexFoundItemsInChest));
                    }
                }
                stackablgeGroupsToChests.Add(group, chestsItemsCanBeMovedTo);
            }
            return stackablgeGroupsToChests;
        }

        /// <summary>
        /// Determines the stackable groups within the given inventory
        /// </summary>
        private static List<List<int>> GetStackableGroupsWithinItemList(IList<Item> itemList)
        {
            List<List<int>> stackableGroups = new();
            for (int i = 0; i < itemList.Count; i++)
            {
                var item = itemList[i];
                if (item != null)
                {
                    bool stackableGroupForItemAlreadyExists = false;
                    foreach (var group in stackableGroups)
                    {
                        var firstItemOfGroup = itemList[group[0]];
                        if (item.canStackWith(firstItemOfGroup))
                        {
                            group.Add(i);
                            stackableGroupForItemAlreadyExists = true;
                            break;
                        }
                    }
                    if (!stackableGroupForItemAlreadyExists)
                    {
                        stackableGroups.Add(new List<int> { i });
                    }
                }
            }
            return stackableGroups;
        }

        /// <summary>
        /// Whether the given item should be put to a chest
        /// </summary>
        /// <returns></returns>
        private bool ItemShouldBePutToChest(Item item)
        {
            return true;
        }

    }
}
