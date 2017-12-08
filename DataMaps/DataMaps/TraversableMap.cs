using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.DataMaps.Framework;
using StardewValley;
using xTile.Dimensions;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Pathoschild.Stardew.DataMaps.DataMaps
{
    /// <summary>A data map which shows whether tiles are traversable by the player.</summary>
    internal class TraversableMap : IDataMap
    {
        /*********
        ** Properties
        *********/
        /// <summary>The color for a passable tile.</summary>
        private readonly Color ClearColor = Color.Green;

        /// <summary>The color for a passable but occupied tile.</summary>
        private readonly Color OccupiedColor = Color.Orange;

        /// <summary>The color for an impassable tile.</summary>
        private readonly Color ImpassableColor = Color.Red;


        /*********
        ** Accessors
        *********/
        /// <summary>The map's display name.</summary>
        public string Name { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public TraversableMap()
        {
            this.Name = "Accessibility";
        }

        /// <summary>Get the legend entries to display.</summary>
        public IEnumerable<LegendEntry> GetLegendEntries()
        {
            return new[]
            {
                new LegendEntry("Clear", this.ClearColor),
                new LegendEntry("Occupied", this.OccupiedColor),
                new LegendEntry("Impassable", this.ImpassableColor)
            };
        }

        /// <summary>Get the updated data map tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleArea">The tiles currently visible on the screen.</param>
        public IEnumerable<TileGroup> Update(GameLocation location, Rectangle visibleArea)
        {
            TileData[] tiles = this.GetTiles(location, visibleArea.GetTiles()).ToArray();

            // passable tiles
            TileData[] passableTiles = tiles.Where(p => p.Color == this.ClearColor).ToArray();
            yield return new TileGroup(passableTiles, outerBorders: true);

            // other tiles
            yield return new TileGroup(tiles.Except(passableTiles).ToArray());
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the updated data map tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleTiles">The tiles currently visible on the screen.</param>
        private IEnumerable<TileData> GetTiles(GameLocation location, IEnumerable<Vector2> visibleTiles)
        {
            foreach (Vector2 tile in visibleTiles)
            {
                bool isPassable = location.isTilePassable(new Location((int)tile.X, (int)tile.Y), Game1.viewport);
                bool isClear = isPassable && !location.isTileOccupiedIgnoreFloors(tile);

                Color color;
                if (isPassable && isClear)
                    color = this.ClearColor;
                else if (isPassable)
                    color = this.OccupiedColor;
                else
                    color = this.ImpassableColor;

                yield return new TileData(tile, color);
            }
        }
    }
}
