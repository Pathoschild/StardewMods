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
        /*********
        ** Public methods
        *********/
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

        /****
        ** GetItems
        ****/
        /// <summary>Get all items from the given pipes.</summary>
        /// <param name="pipes">The pipes to search.</param>
        public static IEnumerable<ITrackedStack> GetItems(this IPipe[] pipes)
        {
            foreach (IPipe pipe in pipes)
            {
                foreach (ITrackedStack item in pipe)
                    yield return item;
            }
        }

        /****
        ** TryGetIngredient
        ****/
        /// <summary>Get an ingredient needed for a recipe.</summary>
        /// <param name="pipes">The pipes to search.</param>
        /// <param name="predicate">Returns whether an item should be matched.</param>
        /// <param name="count">The number of items to find.</param>
        /// <param name="consumable">The matching consumables.</param>
        /// <returns>Returns whether the requirement is met.</returns>
        public static bool TryGetIngredient(this IPipe[] pipes, Func<ITrackedStack, bool> predicate, int count, out Consumable consumable)
        {
            int countMissing = count;
            ITrackedStack[] consumables = pipes.GetItems().Where(predicate)
                .TakeWhile(chestItem =>
                {
                    if (countMissing <= 0)
                        return false;

                    countMissing -= chestItem.Count;
                    return true;
                })
                .ToArray();

            consumable = new Consumable(new TrackedItemCollection(consumables), count);
            return consumable.IsMet;
        }

        /// <summary>Get an ingredient needed for a recipe.</summary>
        /// <param name="pipes">The pipes to search.</param>
        /// <param name="id">The item or category ID.</param>
        /// <param name="count">The number of items to find.</param>
        /// <param name="consumable">The matching consumables.</param>
        /// <returns>Returns whether the requirement is met.</returns>
        public static bool TryGetIngredient(this IPipe[] pipes, int id, int count, out Consumable consumable)
        {
            return pipes.TryGetIngredient(item => item.Sample.parentSheetIndex == id || item.Sample.category == id, count, out consumable);
        }

        /// <summary>Get an ingredient needed for a recipe.</summary>
        /// <param name="pipes">The pipes to search.</param>
        /// <param name="recipes">The items to match.</param>
        /// <param name="consumable">The matching consumables.</param>
        /// <param name="recipe">The matched requisition.</param>
        /// <returns>Returns whether the requirement is met.</returns>
        public static bool TryGetIngredient(this IPipe[] pipes, Recipe[] recipes, out Consumable consumable, out Recipe recipe)
        {
            IDictionary<Recipe, List<ITrackedStack>> accumulator = recipes.ToDictionary(req => req, req => new List<ITrackedStack>());

            foreach (ITrackedStack stack in pipes.GetItems())
            {
                foreach (var entry in accumulator)
                {
                    recipe = entry.Key;
                    List<ITrackedStack> found = entry.Value;

                    if (recipe.AcceptsInput(stack))
                    {
                        found.Add(stack);
                        if (found.Sum(p => p.Count) >= recipe.InputCount)
                        {
                            consumable = new Consumable(new TrackedItemCollection(found), entry.Key.InputCount);
                            return true;
                        }
                    }
                }
            }

            consumable = null;
            recipe = null;
            return false;
        }

        /****
        ** TryConsume
        ****/
        /// <summary>Consume an ingredient needed for a recipe.</summary>
        /// <param name="pipes">The chests to search.</param>
        /// <param name="predicate">Returns whether an item should be matched.</param>
        /// <param name="count">The number of items to find.</param>
        /// <returns>Returns whether the item was consumed.</returns>
        public static bool TryConsume(this IPipe[] pipes, Func<ITrackedStack, bool> predicate, int count)
        {
            if (pipes.TryGetIngredient(predicate, count, out Consumable requirement))
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


        /****
        ** TryPush
        ****/
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
