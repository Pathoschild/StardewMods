using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>Provides utility extensions for machine automation.</summary>
    internal static class Extensions
    {
        /****
        ** Chests
        ****/
        /// <summary>Get all chests connected to a tile.</summary>
        /// <param name="location">The game location to search.</param>
        /// <param name="tile">The tile for which to find connected chests.</param>
        public static IEnumerable<Chest> GetConnectedChests(GameLocation location, Vector2 tile)
        {
            if (location == null)
                yield break;

            Vector2[] connectedTiles = Utility.getSurroundingTileLocationsArray(tile);
            foreach (Vector2 position in connectedTiles)
            {
                if (location.objects.TryGetValue(position, out SObject obj))
                {
                    if (obj is Chest chest)
                        yield return chest;
                }
            }
        }

        /// <summary>Get all matching items from the given chests.</summary>
        /// <param name="chests">The chests to search.</param>
        /// <param name="predicate">Returns whether an item should be matched.</param>
        public static IEnumerable<ChestItem> GetItems(this Chest[] chests, Func<Item, bool> predicate)
        {
            foreach (Chest chest in chests)
            {
                foreach (Item item in chest.items)
                {
                    if (predicate(item))
                        yield return new ChestItem(chest, item);
                }
            }
        }

        /// <summary>Get an ingredient needed for a given recipe.</summary>
        /// <param name="chests">The chests to search.</param>
        /// <param name="predicate">Returns whether an item should be matched.</param>
        /// <param name="count">The number of items to find.</param>
        /// <param name="requirement">The ingredient requirement with matching consumables.</param>
        /// <returns>Returns whether the requirement is met.</returns>
        public static bool TryGetIngredient(this Chest[] chests, Func<Item, bool> predicate, int count, out Requirement requirement)
        {
            int countMissing = count;
            ChestItem[] consumables = chests.GetItems(predicate)
                .TakeWhile(chestItem =>
                {
                    if (countMissing <= 0)
                        return false;

                    countMissing -= chestItem.Item.Stack;
                    return true;
                })
                .ToArray();

            requirement = new Requirement(consumables, count);
            return requirement.IsMet;
        }

        /// <summary>Get an ingredient needed for a given recipe.</summary>
        /// <param name="chests">The chests to search.</param>
        /// <param name="itemID">The item ID.</param>
        /// <param name="count">The number of items to find.</param>
        /// <param name="requirement">The ingredient requirement with matching consumables.</param>
        /// <returns>Returns whether the requirement is met.</returns>
        public static bool TryGetIngredient(this Chest[] chests, int itemID, int count, out Requirement requirement)
        {
            return chests.TryGetIngredient(item => item.parentSheetIndex == itemID, count, out requirement);
        }

        /// <summary>Consume an ingredient from a given chest.</summary>
        /// <param name="chests">The chests to search.</param>
        /// <param name="predicate">Returns whether an item should be matched.</param>
        /// <param name="count">The number of items to find.</param>
        /// <returns>Returns whether the item was consumed.</returns>
        public static bool TryConsume(this Chest[] chests, Func<Item, bool> predicate, int count)
        {
            if (chests.TryGetIngredient(predicate, count, out Requirement requirement))
            {
                requirement.Consume();
                return true;
            }
            return false;
        }

        /// <summary>Consume an ingredient from a given chest.</summary>
        /// <param name="chests">The chests to search.</param>
        /// <param name="itemID">The item ID.</param>
        /// <param name="count">The number of items to find.</param>
        /// <returns>Returns whether the item was consumed.</returns>
        public static bool TryConsume(this Chest[] chests, int itemID, int count)
        {
            return chests.TryConsume(item => item.parentSheetIndex == itemID, count);
        }

        /// <summary>Add the given item stack to the chests if there's space.</summary>
        /// <param name="chests">The chests to fill.</param>
        /// <param name="item">The item stack to push into the chest.</param>
        public static bool TryPush(this Chest[] chests, Item item)
        {
            if (item == null)
                return false;

            foreach (Chest chest in chests)
            {
                if (chest.addItem(item) == null)
                    return true;
            }
            return false;
        }
    }
}
