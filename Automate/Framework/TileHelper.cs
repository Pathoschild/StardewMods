using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using xTile.Layers;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>Provides extension methods for working with tiles.</summary>
    internal static class TileHelper
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get the tile coordinates in the game location.</summary>
        /// <param name="location">The game location to search.</param>
        public static IEnumerable<Vector2> GetTiles(this GameLocation location)
        {
            Layer layer = location.Map.Layers[0];
            return TileHelper.GetTiles(0, 0, layer.LayerWidth, layer.LayerHeight);
        }

        /// <summary>Get the tile coordinates in the tile area.</summary>
        /// <param name="area">The tile area to search.</param>
        public static IEnumerable<Vector2> GetTiles(this Rectangle area)
        {
            return TileHelper.GetTiles(area.X, area.Y, area.Width, area.Height);
        }

        /// <summary>Get the eight tiles surrounding the given tile.</summary>
        /// <param name="tile">The center tile.</param>
        public static IEnumerable<Vector2> GetSurroundingTiles(this Vector2 tile)
        {
            return Utility.getSurroundingTileLocationsArray(tile);
        }

        /// <summary>Get the four tiles adjacent to the given tile.</summary>
        /// <param name="tile">The center tile.</param>
        public static IEnumerable<Vector2> GetAdjacentTiles(this Vector2 tile)
        {
            return Utility.getAdjacentTileLocationsArray(tile);
        }

        /// <summary>Get a rectangular grid of tiles.</summary>
        /// <param name="x">The X coordinate of the top-left tile.</param>
        /// <param name="y">The Y coordinate of the top-left tile.</param>
        /// <param name="width">The grid width.</param>
        /// <param name="height">The grid height.</param>
        public static IEnumerable<Vector2> GetTiles(int x, int y, int width, int height)
        {
            for (int curX = x, maxX = x + width - 1; curX <= maxX; curX++)
            {
                for (int curY = y, maxY = y + height - 1; curY <= maxY; curY++)
                    yield return new Vector2(curX, curY);
            }
        }
    }
}
