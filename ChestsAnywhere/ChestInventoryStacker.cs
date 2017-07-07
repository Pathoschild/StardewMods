using System;
using System.Linq;
using System.Collections.Generic;
using Pathoschild.Stardew.ChestsAnywhere.Framework;
using StardewValley;

namespace Pathoschild.Stardew.ChestsAnywhere
{
    internal class ChestInventoryStacker
    {
        public static void StackItemsInInventory(List<ManagedChest> chests)
        {
            // Combine an item stack with another of the same type.
            //
            // Returns whether the item stack has been completely combined.
            Func<Item, Func<Item, bool>> combineItems = item => itemInChest =>
            {
                int space = item.getStack();
                int availableSpace = Math.Min(space, itemInChest.getRemainingStackSpace());

                if (availableSpace > 0)
                {
                    itemInChest.addToStack(availableSpace);
                    item.addToStack(-1 * availableSpace);
                }

                return Math.Max(0, space - availableSpace) == 0;
            };

            // Index all available items in all chests for O(1) access later.
            var itemsInChests = chests
                .Aggregate(new List<Item> { }, (list, managedChest) =>
                {
                    return list.Concat(managedChest.Chest.items).ToList();
                })
                .Aggregate(new Dictionary<string, List<Item>> { }, (map, item) =>
                {
                    if (!map.ContainsKey(item.Name))
                    {
                        map.Add(item.Name, new List<Item> { });
                    }

                    map[item.Name].Add(item);

                    return map;
                })
            ;

            // Player inventory items; the check is necessary because empty items are included
            var items = Game1.player.items.Where(x => x != null);

            // Items that have an existing stack in some of the chests
            var eligibleItems = items.Where(item => (
                itemsInChests.ContainsKey(item.Name) &&
                itemsInChests[item.Name].Any(itemInChest => itemInChest.canStackWith(item))
            ));

            var moves = eligibleItems.Select(item =>
            {
                return new
                {
                    item,
                    // keep combining the stack until it is drained, if possible:
                    drained = (
                        itemsInChests[item.Name]
                            .Where(itemInChest => itemInChest.canStackWith(item))
                            .Any(combineItems(item))
                    )
                };
            });

            // Remove the items that were completely stacked from the player's inventory.
            moves
                .Where(move => move.drained)
                .Select(move => move.item)
                .ToList()
                .ForEach(item => Game1.player.removeItemFromInventory(item))
            ;

            if (eligibleItems.Count() > 0) {
                Game1.playSound("Ship");
            }
        }
    }
}
