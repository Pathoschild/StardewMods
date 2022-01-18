using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pathoschild.Stardew.ChestsAnywhere.Framework.Containers;
using StardewValley;
using StardewValley.Menus;
using static StardewValley.Objects.Chest;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.QuickStack
{
    /// <summary>
    /// Class to hold logic about quick stack
    /// </summary>
    internal class QuickStackCommand : InventoryToChestCommand
    {
        private readonly QuickStackConfig Config;

        public QuickStackCommand(QuickStackConfig config, List<ManagedChest> availableChests, Farmer player, IClickableMenu menu) : base(availableChests, player, menu)
        {
            this.Config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Performs the quickstacking with the parameters given in the constructor
        /// </summary>
        /// <returns></returns>
        public void Execute()
        {
            var playerItems = this.Player.Items;
            var stackableGroupsToChestsItemsCanBeMovedTo = this.GetPlayerItemToChestMapping(playerItems);
            this.MoveItemsToChests(stackableGroupsToChestsItemsCanBeMovedTo, playerItems);
            Game1.playSound("Ship");
        }

        private void MoveItemsToChests(List<Tuple<InventoryStackableItemGroup, List<ChestInventoryItemMetaData>>> itemGroupsToChestMapping, IList<Item> playerItems)
        {
            // Only select those inventory item groups where chests have been found that can take items from the group
            var itemGroupsThatCanBeMovedToChests = itemGroupsToChestMapping.Where(x => x.Item2.Count > 0);
            // prioritize those items that have the fewest chests they can be moved to
            var orderedPairs = itemGroupsThatCanBeMovedToChests.OrderBy(x => x.Item2.Count);

            foreach (var pair in orderedPairs)
            {
                var possibleChestsToBeMovedTo = pair.Item2;
                var stackableGroup = pair.Item1;
                this.PushItemGroupFromInventoryToChests(stackableGroup, possibleChestsToBeMovedTo, playerItems);
            }
        }

        /// <summary>
        /// Push items from the given stackable item group to the chests in the given order.
        /// Returns a list of indexes describing which items in inventory have been (partially) moved
        /// </summary>
        /// <param name="inventoryGroup"></param>
        /// <param name="chests"></param>
        /// <param name="playerItems"></param>
        private void PushItemGroupFromInventoryToChests(InventoryStackableItemGroup inventoryGroup, List<ChestInventoryItemMetaData> chests, IList<Item> playerItems)
        {
            // begin with chest that already has most of the given Item
            var orderedChests = chests.OrderByDescending(x => x.ItemGroup.GetTotalStackNumber()).ToList();
            // Push to chests as long as there are items in inventory and there is space in chests
            while (!inventoryGroup.IsEmpty() && orderedChests.Where(x => !x.IsFull()).ToList().Count > 0)
            {
                var currentChest = orderedChests.Where(x => !x.IsFull()).ToList().First();
                this.PushItemGroupFromInventoryToChest(inventoryGroup, currentChest, playerItems);
            }
        }

        /// <summary>
        /// Pushes the given items from the inventory to the given chest
        /// </summary>
        /// <param name="itemGroup"></param>
        /// <param name="chestInfo"></param>
        /// <param name="playerItems"></param>
        private void PushItemGroupFromInventoryToChest(InventoryStackableItemGroup itemGroup, ChestInventoryItemMetaData chestInfo, IList<Item> playerItems)
        {
            chestInfo.ItemGroup.InventoryIndexesOfStackableItemStackNotFull.Sort();
            int currentInventoryItemIndex;
            var chestInventory = chestInfo.Chest.Container.Inventory;
            // continue as long as there are items in inventory and the chest still has space
            while (!itemGroup.IsEmpty() && !chestInfo.IsFull())
            {
                var inventoryIndexes = itemGroup.GetIndexes().OrderByDescending(x => x);
                int rightmostInventoryItemIndex = inventoryIndexes.First();
                var rightmostInventoryItem = playerItems[rightmostInventoryItemIndex];
                int chestItemIndex;
                Item chestItem;
                // prefer stacks that are there but not full yet
                var chestStacksNotFull = chestInfo.ItemGroup.InventoryIndexesOfStackableItemStackNotFull;
                if (chestStacksNotFull.Count > 0)
                {
                    chestItemIndex = chestStacksNotFull[0];
                    chestItem = chestInventory[chestItemIndex];
                    rightmostInventoryItem.Stack = chestItem.addToStack(rightmostInventoryItem);
                    this.ShakeChestItemIfIsDisplayedChest(chestItemIndex, chestInfo.Chest.Container.Inventory);
                    chestInfo.ItemGroup.HandleItemWithIndex(chestItem, chestItemIndex);
                    if (rightmostInventoryItem.Stack <= 0)
                    {
                        itemGroup.HandleItemWithIndex(rightmostInventoryItem, rightmostInventoryItemIndex);
                        this.Player.removeItemFromInventory(rightmostInventoryItem);
                    }
                }
                // add new item if there is space in container
                else if (chestInventory.Count < chestInfo.Chest.Container.ActualCapacity)
                {
                    chestInventory.Add(rightmostInventoryItem);
                    chestItem = rightmostInventoryItem;
                    chestItemIndex = chestInventory.IndexOf(chestItem);
                    this.ShakeChestItemIfIsDisplayedChest(chestItemIndex, chestInfo.Chest.Container.Inventory);
                    // item has been fully transferred to chest
                    itemGroup.HandleItemWithIndex(rightmostInventoryItem, rightmostInventoryItemIndex);
                    this.Player.removeItemFromInventory(rightmostInventoryItem);
                    // Following items might be put in this stack
                    chestInfo.ItemGroup.HandleItemWithIndex(chestItem, chestItemIndex);
                }
            }
        }

        /// <summary>
        /// Determines which item groups (stackable items) in inventory can be put into which chests
        /// </summary>
        private List<Tuple<InventoryStackableItemGroup, List<ChestInventoryItemMetaData>>> GetPlayerItemToChestMapping(IList<Item> playerItems)
        {
            List<InventoryStackableItemGroup> groupsInInventory = InventoryStackableItemGroup.DetermineStackableItemGroups(playerItems);
            return CommonInventoryLogic.GetMappingBetweenInventoryGroupsAndChestGroups(playerItems, groupsInInventory, this.GetFilteredAvailableChests());
        }

        /// <summary>
        /// Returns all chests that are available and have to be considered according to config
        /// </summary>
        /// <returns></returns>
        private List<ManagedChest> GetFilteredAvailableChests()
        {
            var filteredChests = new List<ManagedChest>();
            foreach (var chest in this.AvailableChests)
            {
                var container = chest.Container;
                if ((!this.Config.ConsiderJunimoHuts && container is JunimoHutContainer)
                   || (!this.Config.ConsiderShippingBin && container is ShippingBinContainer)
                   || (!this.Config.ConsiderAutoGrabber && container is AutoGrabberContainer)
                   || chest.Container is StorageFurnitureContainer)
                {
                    continue;
                }
                if (!this.Config.ConsiderMiniShippingBins)
                {
                    if (container is ChestContainer chestContainer && chestContainer.Chest.SpecialChestType == SpecialChestTypes.MiniShippingBin)
                    {
                        continue;
                    }
                }
                filteredChests.Add(chest);
            }
            return filteredChests;
        }
    }
}
