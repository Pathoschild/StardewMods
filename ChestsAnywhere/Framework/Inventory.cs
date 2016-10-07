using System;
using System.Collections.Generic;
using StardewValley;
using StardewValley.Menus;

namespace ChestsAnywhere.Framework
{
    /// <summary>Manages access to an inventory.</summary>
    public class Inventory
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The items in the inventory.</summary>
        public List<Item> Items { get; }

        /// <summary>Whether the inventory should avoid gaps.</summary>
        public bool AvoidGaps { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="items">The items in the inventory.</param>
        /// <param name="avoidGaps">Whether the inventory should avoid gaps.</param>
        public Inventory(List<Item> items, bool avoidGaps = false)
        {
            this.Items = items;
            this.AvoidGaps = avoidGaps;
        }

        /// <summary>Get the item at the specified index (if any).</summary>
        /// <param name="index">The index to get.</param>
        public Item GetAt(int index)
        {
            this.AssertValidIndex(index);
            return index < this.Items.Count
                ? this.Items[index]
                : null;
        }

        /// <summary>Remove the item at the specified index (if any).</summary>
        /// <param name="index">The index to remove.</param>
        public void RemoveAt(int index)
        {
            this.AssertValidIndex(index);

            if (index >= this.Items.Count)
                return;
            if (this.AvoidGaps)
                this.Items.RemoveAt(index);
            else
                this.Items[index] = null;
        }

        /// <summary>Reduce the size of the stack at the specified index.</summary>
        /// <param name="index">The index to reduce.</param>
        /// <param name="amount">The amount by which to reduce the stack.</param>
        public void ReduceStack(int index, int amount)
        {
            this.AssertValidIndex(index);

            // get item
            Item item = this.GetAt(index);
            if (item == null)
                return;

            // reduce stack
            if (amount >= item.Stack)
                this.RemoveAt(index);
            else
                item.Stack -= amount;
        }

        /// <summary>Add a stack of items to the inventory, deducting the accepted items from the input stack.</summary>
        /// <param name="input">The stack to accept.</param>
        /// <returns>Returns the number of items accepted.</returns>
        public int AcceptStack(Item input)
        {
            int originalSize = input.Stack;
            Item remaining = this.Clone(input);

            // add to existing slots
            foreach (Item slot in this.Items)
            {
                // make sure stack can accept item
                if (slot == null || !slot.canStackWith(remaining))
                    continue;

                // add to stack
                remaining.Stack = slot.addToStack(remaining.Stack);
                if (remaining.Stack <= 0)
                    return originalSize;
            }

            // fill empty slots
            for (int i = 0; i < this.Items.Count; i++)
            {
                // make sure slot is empty
                if (this.Items[i] != null)
                    continue;

                // add stack
                this.Items[i] = remaining;
                return originalSize;
            }

            // add new slot
            if (this.Items.Count < Constant.SlotCount)
            {
                this.Items.Add(remaining);
                return originalSize;
            }

            // return remaining count
            return originalSize - remaining.Stack;
        }

        /// <summary>Sort the items in the inventory.</summary>
        public void Sort()
        {
            ItemGrabMenu.organizeItemsInList(this.Items);
        }

        /// <summary>Clean up the underlying inventory list.</summary>
        public void Cleanup()
        {
            // remove gaps
            for (int i = 0; i < this.Items.Count; i++)
            {
                // remove empty stack
                if (this.Items[i]?.Stack <= 0)
                    this.Items[i] = null;

                // remove gaps
                if (this.AvoidGaps && this.Items[i] == null)
                {
                    this.Items.RemoveAt(i);
                    i--;
                }
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Assert that the specified index is within valid inventory bounds.</summary>
        /// <param name="index">The index.</param>
        private void AssertValidIndex(int index)
        {
            // too low
            if (index < 0)
                throw new IndexOutOfRangeException("The inventory index cannot be less than 0.");

            // too high
            int maxIndex = Math.Max(Constant.SlotCount, this.Items.Count) - 1;
            if (index > maxIndex)
                throw new IndexOutOfRangeException($"The inventory index cannot exceed {maxIndex}.");
        }

        /// <summary>Clone an item stack.</summary>
        /// <param name="stack">The stack to clone.</param>
        private Item Clone(Item stack)
        {
            Item clone = stack.getOne();
            clone.Stack = stack.Stack;
            return clone;
        }
    }
}