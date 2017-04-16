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

        /// <summary>Get all matching items from the given pipes.</summary>
        /// <param name="pipes">The pipes to search.</param>
        /// <param name="predicate">Returns whether an item should be matched.</param>
        public static IEnumerable<ITrackedStack> GetItems(this IPipe[] pipes, Func<ITrackedStack, bool> predicate)
        {
            foreach (IPipe pipe in pipes)
            {
                foreach (ITrackedStack item in pipe)
                {
                    if (predicate(item))
                        yield return item;
                }
            }
        }

        /// <summary>Get an ingredient needed for a recipe.</summary>
        /// <param name="pipes">The pipes to search.</param>
        /// <param name="predicate">Returns whether an item should be matched.</param>
        /// <param name="count">The number of items to find.</param>
        /// <param name="requirement">The ingredient requirement with matching consumables.</param>
        /// <returns>Returns whether the requirement is met.</returns>
        public static bool TryGetIngredient(this IPipe[] pipes, Func<ITrackedStack, bool> predicate, int count, out Requirement requirement)
        {
            int countMissing = count;
            ITrackedStack[] consumables = pipes.GetItems(predicate)
                .TakeWhile(chestItem =>
                {
                    if (countMissing <= 0)
                        return false;

                    countMissing -= chestItem.Count;
                    return true;
                })
                .ToArray();

            requirement = new Requirement(new TrackedItemCollection(consumables), count);
            return requirement.IsMet;
        }

        /// <summary>Get an ingredient needed for a recipe.</summary>
        /// <param name="pipes">The pipes to search.</param>
        /// <param name="itemID">The item ID.</param>
        /// <param name="count">The number of items to find.</param>
        /// <param name="requirement">The ingredient requirement with matching consumables.</param>
        /// <returns>Returns whether the requirement is met.</returns>
        public static bool TryGetIngredient(this IPipe[] pipes, int itemID, int count, out Requirement requirement)
        {
            return pipes.TryGetIngredient(item => item.Sample.parentSheetIndex == itemID, count, out requirement);
        }

        /// <summary>Consume an ingredient needed for a recipe.</summary>
        /// <param name="pipes">The chests to search.</param>
        /// <param name="predicate">Returns whether an item should be matched.</param>
        /// <param name="count">The number of items to find.</param>
        /// <returns>Returns whether the item was consumed.</returns>
        public static bool TryConsume(this IPipe[] pipes, Func<ITrackedStack, bool> predicate, int count)
        {
            if (pipes.TryGetIngredient(predicate, count, out Requirement requirement))
            {
                requirement.Reduce();
                return true;
            }
            return false;
        }

        /// <summary>Consume an ingredient needed for a recipe.</summary>
        /// <param name="pipes">The chests to search.</param>
        /// <param name="itemID">The item ID.</param>
        /// <param name="count">The number of items to find.</param>
        /// <returns>Returns whether the item was consumed.</returns>
        public static bool TryConsume(this IPipe[] pipes, int itemID, int count)
        {
            return pipes.TryConsume(item => item.Sample.parentSheetIndex == itemID, count);
        }

        /// <summary>Add the given item stack to the pipes if there's space.</summary>
        /// <param name="pipes">The pipes to fill.</param>
        /// <param name="item">The item stack to push.</param>
        public static bool TryPush(this IPipe[] pipes, ITrackedStack item)
        {
            if (item == null || item.Count <= 0)
                return false;

            int originalCount = item.Count;
            foreach (IPipe pipe in pipes)
            {
                pipe.Store(item);
                if (item.Count <= 0)
                    break;
            }

            return item.Count < originalCount;
        }
    }
}
