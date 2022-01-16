using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pathoschild.Stardew.ChestsAnywhere.Framework.Containers;
using StardewValley;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.QuickStack
{
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
            var stackableGroupsToChestsItemsCanBeMovedTo = this.GetPlayerItemToChestItemMapping();

            return null;
        }

        /// <summary>
        /// Determines which item groups (stackable items) in inventory can be put into which chests
        /// </summary>
        private Dictionary<List<int>, List<ManagedChest>> GetPlayerItemToChestItemMapping()
        {
            Dictionary<List<int>, List<ManagedChest>> stackablgeGroupsToChests = new();
            var playerItems = this.Player.Items;
            var stackableGroupsInInventory = GetStackableGroupsWithinItemList(playerItems);

            // check for every group if a chest is relevant
            // could be optimized
            foreach(var group in stackableGroupsInInventory)
            {
                var groupRepresentativeItem = playerItems[group[0]];
                var chestsItemsCanBeMovedTo = new List<ManagedChest>();
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
                    foreach(var chestItem in container.Inventory)
                    {
                        if(groupRepresentativeItem.canStackWith(chestItem))
                        {
                            chestsItemsCanBeMovedTo.Add(chest);
                            // item has been found in this chest, go to next
                            continue;
                        }
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
                    foreach (var group in stackableGroups)
                    {
                        var firstItemOfGroup = itemList[group[0]];
                        if (item.canStackWith(firstItemOfGroup))
                        {
                            group.Add(i);
                            break;
                        }
                        else
                        {
                            stackableGroups.Add(new List<int> { i });
                        }
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
