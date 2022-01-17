using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pathoschild.Stardew.ChestsAnywhere.Framework.Containers;
using StardewValley;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.QuickStack
{

    internal class ChestItemInformation
    {
        /// <summary>
        /// The chest holding the type of item
        /// </summary>
        public readonly ManagedChest Chest;

        /// <summary>
        /// List of index of items in chest that can stack with a given item and is not at max stack size yet
        /// </summary>
        public List<int> InventoryIndexesOfStackableItemInChestNotFull;

        /// <summary>
        /// How many stacks of this item are already in the chest
        /// </summary>
        public int TotalStackNumberInChest;

        /// <summary>
        /// If chest is full and can't hold any of the item
        /// </summary>
        public bool IsFull;

        public ChestItemInformation(ManagedChest chest, int totalStackNumberInChest, List<int> inventoryIndexesOfStackableItemInChestNotFull, bool chestFull)
        {
            this.Chest = chest;
            this.TotalStackNumberInChest = totalStackNumberInChest;
            this.InventoryIndexesOfStackableItemInChestNotFull = inventoryIndexesOfStackableItemInChestNotFull;
            this.IsFull = chestFull;
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

        private QuickStackCommandResult MoveItemsToChests(Dictionary<List<int>, List<ChestItemInformation>> itemGroupsToChestMapping, IList<Item> playerItems)
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
                var orderedStackableGroup = stackableGroup.OrderByDescending(i => i).ToList();
                // begin with chest that already has most of the given Item
                var orderedChests = possibleChestsToBeMovedTo.OrderByDescending(x => x.TotalStackNumberInChest).ToList();

                this.PushItemGroupFromInventoryToChests(orderedStackableGroup, orderedChests, playerItems);
                // all items have been pushed to chest
                if(orderedStackableGroup.Count == 0)
                {

                }
            }
            return null;
        }

        /// <summary>
        /// Push items from the given stackable item group to the chests in the given order.
        /// </summary>
        /// <param name="orderedItemsInInventory"></param>
        /// <param name="orderedChests"></param>
        /// <param name="playerItems"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void PushItemGroupFromInventoryToChests(List<int> orderedItemsInInventory, List<ChestItemInformation> orderedChests, IList<Item> playerItems)
        {
            // Push to chests as long as there are items in inventory and there is space in chests
            while (orderedItemsInInventory.Count > 0 && orderedChests.Where(x => !x.IsFull).ToList().Count > 0)
            {
                var currentChest = orderedChests.Where(x => !x.IsFull).ToList().First();
                this.PushItemGroupFromInventoryToChest(orderedItemsInInventory, currentChest, playerItems);
            }
        }

        /// <summary>
        /// Pushes the given items from the inventory to the given chest
        /// </summary>
        /// <param name="orderedStackableGroup"></param>
        /// <param name="chestInfo"></param>
        /// <param name="playerItems"></param>
        /// <returns></returns>
        private void PushItemGroupFromInventoryToChest(List<int> itemIndexesInInventory, ChestItemInformation chestInfo, IList<Item> playerItems)
        {
            // continue as long as there are items in inventory and the chest still has space
            int currentInventoryItemIndex;
            var chestInventory = chestInfo.Chest.Container.Inventory;
            while(itemIndexesInInventory.Count > 0 && !chestInfo.IsFull)
            {
                currentInventoryItemIndex = itemIndexesInInventory[0];
                var rightMostInventoryItem = playerItems[currentInventoryItemIndex];
                int chestItemIndex;
                Item chestItem;
                // prefer stacks that are there but not full yet
                if (chestInfo.InventoryIndexesOfStackableItemInChestNotFull.Count > 0)
                {
                    chestItemIndex = chestInfo.InventoryIndexesOfStackableItemInChestNotFull[0];
                    chestItem = chestInventory[chestItemIndex];
                    rightMostInventoryItem.Stack = chestItem.addToStack(rightMostInventoryItem);
                    // if item stack in chest is at maximum ignore it from now on
                    if(chestItem.Stack == chestItem.maximumStackSize())
                    {
                        chestInfo.InventoryIndexesOfStackableItemInChestNotFull.RemoveAt(0);
                    }
                    if (rightMostInventoryItem.Stack <= 0)
                    {
                        this.Player.removeItemFromInventory(rightMostInventoryItem);
                        itemIndexesInInventory.RemoveAt(0);
                    }
                }
                // add new item if there is space in container
                else if(chestInventory.Count < chestInfo.Chest.Container.ActualCapacity)
                {
                    chestInventory.Add(rightMostInventoryItem);
                    chestItem = rightMostInventoryItem;
                    chestItemIndex = chestInventory.IndexOf(chestItem);
                    // item has been fully transferred to chest
                    this.Player.removeItemFromInventory(rightMostInventoryItem);
                    itemIndexesInInventory.RemoveAt(0);
                    // Following items might be put in this stack
                    if (chestItem.Stack < chestItem.maximumStackSize())
                    {
                        chestInfo.InventoryIndexesOfStackableItemInChestNotFull.Add(chestItemIndex);
                    }
                }
                else
                {
                    chestInfo.IsFull = true;
                }
            }
        }

        /// <summary>
        /// Determines which item groups (stackable items) in inventory can be put into which chests
        /// </summary>
        private Dictionary<List<int>, List<ChestItemInformation>> GetPlayerItemToChestMapping(IList<Item> playerItems)
        {
            Dictionary<List<int>, List<ChestItemInformation>> stackablgeGroupsToChests = new();
            var stackableGroupsInInventory = GetStackableGroupsWithinItemList(playerItems);

            // check for every group if a chest is relevant
            // could be optimized
            foreach(var group in stackableGroupsInInventory)
            {
                var groupRepresentativeItem = playerItems[group[0]];
                var chestsItemsCanBeMovedTo = new List<ChestItemInformation>();
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
                            numStacksOfItemFoundInChest += chestItem.Stack;
                            itemFoundInChest = true;
                            // only rember item if stack isn't full yet
                            if(chestItem.Stack < chestItem.maximumStackSize())
                            {
                                indexFoundItemsInChest.Add(i);
                            }
                        }
                    }
                    // sort found items, that way items will be tried to be pushed leftmost first
                    indexFoundItemsInChest.Sort();
                    bool chestIsFull = true;
                    if(indexFoundItemsInChest.Count > 0 || chest.Container.Inventory.Count < chest.Container.ActualCapacity)
                    {
                        chestIsFull = false;
                    }
                    if (itemFoundInChest)
                    {
                        chestsItemsCanBeMovedTo.Add(new ChestItemInformation(chest, numStacksOfItemFoundInChest, indexFoundItemsInChest, chestIsFull));
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
