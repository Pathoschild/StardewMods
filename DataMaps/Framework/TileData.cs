using Microsoft.Xna.Framework;

namespace Pathoschild.Stardew.DataMaps.Framework
{
    /// <summary>Metadata for a tile.</summary>
    internal class TileData
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The tile position.</summary>
        public Vector2 TilePosition { get; }

        /// <summary>The overlay color.</summary>
        public Color Color { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="tile">The tile position.</param>
        /// <param name="color">The overlay color.</param>
        public TileData(Vector2 tile, Color color)
        {
            this.TilePosition = tile;
            this.Color = color;
        }
    }
}
