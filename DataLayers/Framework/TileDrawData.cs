using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Pathoschild.Stardew.DataLayers.Framework
{
    /// <summary>Aggregate drawing metadata for a tile.</summary>
    internal class TileDrawData
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The tile position.</summary>
        public Vector2 TilePosition { get; }

        /// <summary>The overlay colors to draw.</summary>
        public HashSet<Color> Colors { get; } = [];

        /// <summary>The border colors to draw.</summary>
        public Dictionary<Color, TileEdge> BorderColors { get; } = new();

        /// <summary>The pixel offset at which to draw this tile.</summary>
        public Point DrawOffset { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="position">The tile position.</param>
        /// <param name="drawOffset">The pixel offset at which to draw this tile.</param>
        public TileDrawData(Vector2 position, Point drawOffset)
        {
            this.TilePosition = position;
            this.DrawOffset = drawOffset;
        }
    }
}
