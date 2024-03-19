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
        /// <inheritdoc />
        public Item Sample { get; private set; }

        /// <inheritdoc />
        public string Type { get; private set; }

        /// <inheritdoc />
        public int Count => this.Stacks.Sum(p => p.Count);


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="stacks">The underlying item stacks.</param>
        public TrackedItemCollection(params ITrackedStack[] stacks)
        {
            if (!stacks.Any())
                throw new InvalidOperationException("Can't create a tracked item collection containing no items.");

            this.Sample = stacks[0].Sample;
            this.Type = this.Sample.TypeDefinitionId;

            foreach (ITrackedStack stack in stacks)
                this.Add(stack);
        }

        /// <summary>Add a stack to the collection.</summary>
        /// <param name="stack">The item stack to add.</param>
        public void Add(ITrackedStack stack)
        {
            if (stack == null)
                throw new ArgumentNullException(nameof(stack));
            if (stack.Sample == null)
                throw new InvalidOperationException("Can't track an item with no underlying item.");

            this.Stacks.Add(stack);
        }

        /// <summary>Get whether the underlying items can stack with the items in another stack, based on their respective <see cref="Sample"/> values.</summary>
        /// <param name="stack">The other stack to check.</param>
        public bool CanStackWith(ITrackedStack? stack)
        {
            return
                stack?.Sample != null
                && this.Sample.canStackWith(stack.Sample);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public Item? Take(int count)
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
