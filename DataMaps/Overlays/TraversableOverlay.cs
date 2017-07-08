using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.DataMaps.Framework;
using StardewValley;
using xTile.Dimensions;

namespace Pathoschild.Stardew.DataMaps.Overlays
{
    /// <summary>An overlay which shows whether tiles are traversable by the player.</summary>
    internal class TraversableOverlay : DataMapOverlay
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public TraversableOverlay()
            : base(new LegendComponent(Tuple.Create(Color.Green, "Clear"), Tuple.Create(Color.Orange, "Occupied"), Tuple.Create(Color.Red, "Impassable")))
        { }


        /*********
        ** Protected methods
        *********/
        /// <summary>Get updated tile overlay data.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleTiles">The tiles currently visible on the screen.</param>
        protected override IEnumerable<TileData> Update(GameLocation location, IEnumerable<Vector2> visibleTiles)
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
