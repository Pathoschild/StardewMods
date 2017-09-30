using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>A collection of machines and chests that operate as one unit.</summary>
    internal class FactoryGroup : IEnumerable<Vector2>
    {
        /*********
        ** Properties
        *********/
        /// <summary>The tiles which are part of this group.</summary>
        public HashSet<Vector2> Tiles { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public FactoryGroup()
            : this(new HashSet<Vector2>()) { }

        /// <summary>Construct an instance.</summary>
        /// <param name="tiles">The tile which are part of this group.</param>
        public FactoryGroup(HashSet<Vector2> tiles)
        {
            this.Tiles = tiles;
        }

        /// <summary>Add an tile to the factory.</summary>
        /// <param name="tile">The tile to add.</param>
        /// <returns>Whether the tile was added.</returns>
        public bool Add(Vector2 tile)
        {
            if (!this.CanAdd(tile))
                return false;

            this.Tiles.Add(tile);
            return true;
        }

        /// <summary>Remove a tile from the factory.</summary>
        /// <param name="tile">The tile to remove.</param>
        public bool Remove(Vector2 tile)
        {
            if (!this.CanRemove(tile))
                return false;

            return this.Tiles.Remove(tile);
        }

        /// <summary>Add a tile to the factory if it's valid and not already present, else remove it if possible.</summary>
        /// <param name="tile">The tile to remove.</param>
        public bool AddOrRemove(Vector2 tile)
        {
            return this.Remove(tile) || this.Add(tile);
        }

        /// <summary>Get whether any tile in the group is adjacent to the given tile.</summary>
        /// <param name="tile">The tile to check.</param>
        public bool IsAdjacentTo(Vector2 tile)
        {
            return Utility
                .getAdjacentTileLocationsArray(tile)
                .Intersect(this.Tiles)
                .Any();
        }

        /// <summary>Get an enumerator that iterates through the collection.</summary>
        public IEnumerator<Vector2> GetEnumerator()
        {
            return this.Tiles.GetEnumerator();
        }

        /// <summary>Get an enumerator that iterates through the collection.</summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get whether a tile can be added to a group.</summary>
        /// <param name="tile">The tile to check.</param>
        private bool CanAdd(Vector2 tile)
        {
            if (this.Tiles.Contains(tile))
                return false;

            return
                !this.Tiles.Any() // first tile
                || this.IsAdjacentTo(tile); // adjacent to any group tile
        }

        /// <summary>Get whether a tile can be removed from the factory.</summary>
        /// <param name="tile">The tile to check.</param>
        private bool CanRemove(Vector2 tile)
        {
            if (!this.Tiles.Contains(tile))
                return false;

            return
                this.Tiles.Count <= 2
                || this.IsContiguousGrid(this.Tiles.Except(new[] { tile }));
        }

        /// <summary>Get whether the given tiles form a contiguous grid.</summary>
        /// <param name="tiles">The tiles to check.</param>
        private bool IsContiguousGrid(IEnumerable<Vector2> tiles)
        {
            // get unvisit
            HashSet<Vector2> unvisited = new HashSet<Vector2>(tiles);
            if (unvisited.Count <= 1)
                return true; // can't be non-contiguous

            // visit all tiles accessible from an arbitrary tile
            var queue = new Queue<Vector2>(unvisited.Take(1));
            while (queue.Any())
            {
                // visit current tile
                Vector2 cur = queue.Dequeue();
                if (!unvisited.Contains(cur))
                    continue; // already visited or not in group
                unvisited.Remove(cur);

                // queue its neighbours
                foreach (Vector2 next in Utility.getAdjacentTileLocationsArray(cur))
                {
                    if (unvisited.Contains(next))
                        queue.Enqueue(next);
                }
            }

            // check if removing this tile would make the factory non-contiguous
            return !unvisited.Any();
        }
    }
}
