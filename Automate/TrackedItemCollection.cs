using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;

namespace Pathoschild.Stardew.Automate
{
    /// <summary>An item stack which wraps an underlying collection of stacks.</summary>
    public class TrackedItemCollection : ITrackedStack
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying item stacks.</summary>
        private readonly IList<ITrackedStack> Stacks = new List<ITrackedStack>();


        /*********
        ** Accessors
        *********/
        /// <summary>A sample item for comparison.</summary>
        /// <remarks>This should be equivalent to the underlying item (except in stack size), but *not* a reference to it.</remarks>
        public Item Sample { get; private set; }

        /// <summary>The number of items in the stack.</summary>
        public int Count => this.Stacks.Sum(p => p.Count);


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="stacks">The underlying item stacks.</param>
        public TrackedItemCollection(params ITrackedStack[] stacks)
        {
            foreach (ITrackedStack stack in stacks)
                this.Add(stack);
        }

        /// <summary>Add a stack to the collection.</summary>
        /// <param name="stack">The item stack to add.</param>
        public void Add(ITrackedStack stack)
        {
            if (stack?.Sample == null)
                throw new InvalidOperationException("Can't track an item with no underlying item.");

            this.Stacks.Add(stack);
            if (this.Sample == null)
                this.Sample = stack.Sample;
        }

        /// <summary>Get whether the underlying items can stack with the items in another stack, based on their respective <see cref="Sample"/> values.</summary>
        /// <param name="stack">The other stack to check.</param>
        public bool CanStackWith(ITrackedStack stack)
        {
            if (stack?.Sample == null)
                return false;

            return this.Sample == null || this.Sample.canStackWith(stack.Sample);
        }

        /// <summary>Remove the specified number of this item from the stack.</summary>
        /// <param name="count">The number to consume.</param>
        public void Reduce(int count)
        {
            if (count <= 0 || !this.Stacks.Any())
                return;

            // reduce
            int left = count;
            foreach (ITrackedStack stack in this.Stacks)
            {
                // skip, stack empty
                if (stack.Count <= 0)
                    continue;

                // take entire stack
                if (stack.Count < left)
                {
                    left -= stack.Count;
                    stack.Reduce(stack.Count);
                    continue;
                }

                // take remaining items
                stack.Reduce(left);
                break;
            }
        }

        /// <summary>Remove the specified number of this item from the stack and return a new stack matching the count.</summary>
        /// <param name="count">The number to get.</param>
        public Item Take(int count)
        {
            if (count <= 0 || !this.Stacks.Any())
                return null;

            // reduce
            this.Reduce(count);

            // create new stack
            Item item = this.Sample.getOne();
            item.Stack = count;
            return item;
        }
    }
}
