using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Pathoschild.Stardew.DataMaps.Framework
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
        public HashSet<Color> Colors { get; } = new HashSet<Color>();

        /// <summary>The border colors to draw.</summary>
        public IDictionary<Color, TileEdge> BorderColors { get; } = new Dictionary<Color, TileEdge>();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="position">The tile position.</param>
        public TileDrawData(Vector2 position)
        {
            this.TilePosition = position;
        }
    }
}
