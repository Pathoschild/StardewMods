using System.Collections.Generic;
using System.Linq;
using StardewValley;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>Handles accumulating items into stacks based on their <see cref="Item.canStackWith"/> method.</summary>
    internal class StackAccumulator : List<TrackedItemCollection>
    {
        /// <summary>Add an item to its corresponding stack or add a new stack.</summary>
        /// <param name="input">The input item to add.</param>
        /// <returns>Returns the affected stack.</returns>
        public TrackedItemCollection Add(ITrackedStack input)
        {
            TrackedItemCollection stack = this.FirstOrDefault(p => p.CanStackWith(input));
            if (stack != null)
                stack.Add(input);
            else
                base.Add(stack = new TrackedItemCollection(input));

            return stack;
        }
    }
}
