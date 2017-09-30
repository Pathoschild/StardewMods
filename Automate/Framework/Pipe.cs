using System;
using System.Collections;
using System.Collections.Generic;
using StardewValley;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>Implements the logic for exchanging items between two sources.</summary>
    internal class Pipe : IPipe
    {
        /*********
        ** Properties
        *********/
        /// <summary>The endpoint from which items are pushed and pulled.</summary>
        public IStorage Endpoint { get; }



        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="endpoint">The endpoint from which items are pushed and pulled.</param>
        public Pipe(IStorage endpoint)
        {
            this.Endpoint = endpoint;
        }

        /// <summary>Store an item stack.</summary>
        /// <param name="stack">The item stack to store.</param>
        /// <remarks>If the storage can't hold the entire stack, it should reduce the tracked stack accordingly.</remarks>
        public void Store(ITrackedStack stack)
        {
            this.Endpoint.Store(stack);
        }

        /// <summary>Find items in the pipe matching a predicate.</summary>
        /// <param name="predicate">Matches items that should be returned.</param>
        /// <param name="count">The number of items to find.</param>
        /// <returns>If the pipe has no matching item, returns <c>null</c>. Otherwise returns a tracked item stack, which may have less items than requested if no more were found.</returns>
        public ITrackedStack Get(Func<Item, bool> predicate, int count)
        {
            return this.Endpoint.Get(predicate, count);
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<ITrackedStack> GetEnumerator()
        {
            return this.Endpoint.GetEnumerator();
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
