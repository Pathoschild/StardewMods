using System;
using xTile;
using xTile.Dimensions;
using xTile.Layers;

namespace Pathoschild.Stardew.Common
{
    /// <summary>Provides extension methods for <see cref="Map"/>.</summary>
    internal static class MapExtensions
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get the map's size in tiles.</summary>
        /// <param name="map">The map instance.</param>
        public static Size GetSizeInTiles(this Map map)
        {
            int width = 1;
            int height = 1;

            foreach (Layer layer in map.Layers)
            {
                width = Math.Max(width, layer.LayerWidth);
                height = Math.Max(height, layer.LayerHeight);
            }

            return new Size(width, height);
        }
    }
}
