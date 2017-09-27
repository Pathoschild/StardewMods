using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>Provides and stores items for machines.</summary>
    internal interface IPipe : IEnumerable<ITrackedStack>
    {
        /// <summary>Find items in the pipe matching a predicate.</summary>
        /// <param name="predicate">Matches items that should be returned.</param>
        /// <param name="count">The number of items to find.</param>
        /// <returns>If the pipe has no matching item, returns <c>null</c>. Otherwise returns a tracked item stack, which may have less items than requested if no more were found.</returns>
        ITrackedStack Get(Func<Item, bool> predicate, int count);

        /// <summary>Store an item stack.</summary>
        /// <param name="stack">The item stack to store.</param>
        /// <remarks>If the storage can't hold the entire stack, it should reduce the tracked stack accordingly.</remarks>
        void Store(ITrackedStack stack);

        // Should this be HashSet<Vector2>?
        /// <summary>Gets the tile of the pipe.</summary>
        /// <returns>Returns the tile the pipe is connected to.</returns>
        Vector2 GetSourceTile();
    }
}