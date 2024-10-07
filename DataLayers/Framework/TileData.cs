using Microsoft.Xna.Framework;

namespace Pathoschild.Stardew.DataLayers.Framework
{
    /// <summary>Metadata for a tile.</summary>
    internal class TileData
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The associated legend entry.</summary>
        public LegendEntry Type { get; }

        /// <summary>The tile position.</summary>
        public Vector2 TilePosition { get; }

        /// <summary>The overlay color.</summary>
        public Color Color { get; }

        /// <summary>The pixel offset at which to draw this tile.</summary>
        public Point DrawOffset { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="tile">The tile position.</param>
        /// <param name="type">The associated legend entry.</param>
        /// <param name="color">The overlay color.</param>
        /// <param name="drawOffset">The pixel offset at which to draw this tile.</param>
        public TileData(Vector2 tile, LegendEntry type, Color color, Point? drawOffset = null)
        {
            this.TilePosition = tile;
            this.Type = type;
            this.Color = color;
            this.DrawOffset = drawOffset ?? Point.Zero;
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="tile">The tile position.</param>
        /// <param name="type">The associated legend entry.</param>
        public TileData(Vector2 tile, LegendEntry type)
            : this(tile, type, type.Color) { }
    }
}
