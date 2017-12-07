using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.DataMaps.Framework;
using StardewValley;
using xTile.Dimensions;

namespace Pathoschild.Stardew.DataMaps.DataMaps
{
    /// <summary>A data map which shows whether tiles are traversable by the player.</summary>
    internal class TraversableMap : IDataMap
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get the legend entries to display.</summary>
        public IEnumerable<LegendEntry> GetLegendEntries()
        {
            return new[]
            {
                new LegendEntry("Clear", Color.Green),
                new LegendEntry("Occupied", Color.Orange),
                new LegendEntry("Impassable", Color.Red)
            };
        }

        /// <summary>Get the updated data map tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleTiles">The tiles currently visible on the screen.</param>
        public IEnumerable<TileData> Update(GameLocation location, IEnumerable<Vector2> visibleTiles)
        {
            foreach (Vector2 tile in visibleTiles)
            {
                bool isPassable = location.isTilePassable(new Location((int)tile.X, (int)tile.Y), Game1.viewport);
                bool isClear = isPassable && !location.isTileOccupiedIgnoreFloors(tile);

                Color color;
                if (isPassable && isClear)
                    color = Color.Green;
                else if (isPassable)
                    color = Color.Orange;
                else
                    color = Color.Red;

                yield return new TileData(tile, color);
            }
        }
    }
}
