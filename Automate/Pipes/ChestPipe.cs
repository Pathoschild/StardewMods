using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Automate.Framework;
using StardewValley;
using StardewValley.Objects;

namespace Pathoschild.Stardew.Automate.Pipes
{
    /// <summary>An object which can store or provide items for machines to use.</summary>
    internal class ChestPipe : IPipe
    {
        /*********
        ** Properties
        *********/
        /// <summary>The underlying chest.</summary>
        private readonly Chest Chest;

        /// <summary>The chest tile.</summary>
        private readonly Vector2 SourceTile;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="chest">The underlying chest.</param>
        /// <param name="sourceTile">The tile the pipe is connected to.</param>
        public ChestPipe(Chest chest, Vector2 sourceTile)
        {
            this.Chest = chest;
            this.SourceTile = sourceTile;
        }

        /// <summary>Get the tile connected to this pipe.</summary>
        public Vector2 GetSourceTile()
        {
            return this.SourceTile;
        }

        /// <summary>Store an item stack.</summary>
        /// <param name="stack">The item stack to store.</param>
        /// <remarks>If the storage can't hold the entire stack, it should reduce the tracked stack accordingly.</remarks>
        public void Store(ITrackedStack stack)
        {
            if (stack.Count <= 0)
                return;

            List<Item> inventory = this.Chest.items;

            // try stack into existing slot
            foreach (Item slot in inventory)
            {
                if (slot != null && stack.Sample.canStackWith(slot))
                {
                    int added = stack.Count - slot.addToStack(stack.Count);
                    stack.Reduce(added);
                    if (stack.Count <= 0)
                        return;
                }
            }

            // try add to empty slot
            for (int i = 0; i < Chest.capacity && i < inventory.Count; i++)
            {
                if (inventory[i] == null)
                {
                    inventory[i] = stack.Take(stack.Count);
                    return;
                }

            }

            // try add new slot
            if (inventory.Count < Chest.capacity)
                inventory.Add(stack.Take(stack.Count));
        }

        /// <summary>Find items in the pipe matching a predicate.</summary>
        /// <param name="predicate">Matches items that should be returned.</param>
        /// <param name="count">The number of items to find.</param>
        /// <returns>If the pipe has no matching item, returns <c>null</c>. Otherwise returns a tracked item stack, which may have less items than requested if no more were found.</returns>
        public ITrackedStack Get(Func<Item, bool> predicate, int count)
        {
            ITrackedStack[] stacks = this.GetImpl(predicate, count).ToArray();
            if (!stacks.Any())
                return null;
            return new TrackedItemCollection(stacks);
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<ITrackedStack> GetEnumerator()
        {
            foreach (Item item in this.Chest.items.ToArray())
            {
                if (item != null)
                    yield return this.GetTrackedItem(item);
            }
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Find items in the pipe matching a predicate.</summary>
        /// <param name="predicate">Matches items that should be returned.</param>
        /// <param name="count">The number of items to find.</param>
        /// <remarks>If there aren't enough items in the pipe, it should return those it has.</remarks>
        private IEnumerable<ITrackedStack> GetImpl(Func<Item, bool> predicate, int count)
        {
            int countFound = 0;
            foreach (Item item in this.Chest.items)
            {
                if (item != null && predicate(item))
                {
                    countFound += item.Stack;
                    yield return this.GetTrackedItem(item);
                    if (countFound >= count)
                        yield break;
                }
            }
        }

        /// <summary>Get a tracked item sync'd with the chest inventory.</summary>
        /// <param name="item">The item to track.</param>
        private ITrackedStack GetTrackedItem(Item item)
        {
            return new TrackedItem(item, onEmpty: i => this.Chest.items.Remove(i));
        }
    }
}
