using System;
using System.Diagnostics.CodeAnalysis;
using StardewValley;

namespace Pathoschild.Stardew.Automate
{
    /// <summary>An item stack which notifies callbacks when it's reduced.</summary>
    public class TrackedItem : ITrackedStack
    {
        /*********
        ** Fields
        *********/
        /// <summary>The item stack.</summary>
        private readonly Item Item;

        /// <summary>The callback invoked when the stack size is reduced (including reduced to zero).</summary>
        protected readonly Action<Item>? OnReduced;

        /// <summary>The callback invoked when the stack is empty.</summary>
        protected readonly Action<Item>? OnEmpty;

        /// <summary>The last stack size handlers were notified of.</summary>
        private int LastCount;


        /*********
        ** Accessors
        *********/
        /// <inheritdoc />
        public Item Sample { get; }

        /// <inheritdoc />
        public string Type { get; }

        /// <inheritdoc />
        public int Count { get; private set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="item">The item stack.</param>
        /// <param name="onReduced">The callback invoked when the stack size is reduced (including reduced to zero).</param>
        /// <param name="onEmpty">The callback invoked when the stack is empty.</param>
        public TrackedItem(Item item, Action<Item>? onReduced = null, Action<Item>? onEmpty = null)
        {
            this.Item = item ?? throw new InvalidOperationException("Can't track a null item stack.");
            this.Type = item.TypeDefinitionId;
            this.Sample = this.GetNewStack(item);
            this.OnReduced = onReduced;
            this.OnEmpty = onEmpty;
            this.Count = item.Stack; // we can't trust Item.Stack to reduce correctly (e.g. Hat.Stack always return 1), so we need to track it ourselves
            this.LastCount = this.Count;
        }

        /// <inheritdoc />
        public void Reduce(int count)
        {
            if (count <= 0)
                return;

            this.Item.Stack -= count;
            this.Count -= count;

            this.Delegate();
        }

        /// <inheritdoc />
        public Item? Take(int count)
        {
            if (count <= 0)
                return null;

            this.Reduce(count);
            return this.GetNewStack(this.Item, count);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Notify handlers.</summary>
        private void Delegate()
        {
            // skip if not reduced
            if (this.Count >= this.LastCount)
                return;
            this.LastCount = this.Count;

            // notify handlers
            this.OnReduced?.Invoke(this.Item);
            if (this.Count <= 0)
                this.OnEmpty?.Invoke(this.Item);
        }

        /// <summary>Create a new stack of the given item.</summary>
        /// <param name="original">The item stack to clone.</param>
        /// <param name="stackSize">The new stack size.</param>
        [return: NotNullIfNotNull("original")]
        private Item? GetNewStack(Item? original, int stackSize = 1)
        {
            if (original == null)
                return null;

            Item stack = original.getOne();
            stack.Stack = stackSize;
            return stack;
        }
    }
}
