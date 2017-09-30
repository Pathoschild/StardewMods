using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;

namespace Pathoschild.Stardew.Automate.Framework
{
    internal class FactoryGroup
    {
        public FactoryGroupData Data { get; }
        //When we connect then we can store this
        //public IEnumerable<MachineMetadata> Machines { get; }
        //public IEnumerable<IPipe> Pipes { get; }

        /// <summary>Checks if the tile already exist.</summary>
        /// <param name="tile">The given tile.</param>
        public bool Contains(Vector2 tile) {
            return this.Data.Tiles.Contains(tile);
        }

        public bool CanAddToGroup(Vector2 tile, HashSet<Vector2> groupTiles)
        {
            // first tile
            if (!groupTiles.Any())
                return true;

            // adjacent to any tile in group
            return IsAdjacentTile(tile, groupTiles);
        }

        public bool IsAdjacentTile(Vector2 tile, HashSet<Vector2> groupTiles)
        {
            return Utility
                .getAdjacentTileLocationsArray(tile)
                .Intersect(groupTiles)
                .Any();
        }

        /// <summary>Get whether a given tile is the only link between two parts of a contiguous group.</summary>
        /// <param name="group">The contiguous group of tiles to check.</param>
        /// <param name="tile">The tile to check.</param>
        public bool IsBridgeTile(Vector2 tile, Vector2[] group)
        {
            // validate
            if (group.Length < 3)
                return false; // can't have a bridge with 2 tiles
            if (!group.Contains(tile))
                return false;

            // visit all tiles accessible from an arbitrary tile
            var unvisited = new HashSet<Vector2>(group.Except(new[] { tile }));
            var queue = new Queue<Vector2>(new[] { group.First(p => p != tile) });
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

            // check if any tiles couldn't be visited
            return unvisited.Any();
        }
    }
}
