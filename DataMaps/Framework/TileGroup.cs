using Microsoft.Xna.Framework;

namespace Pathoschild.Stardew.DataMaps.Framework
{
    /// <summary>A group of tiles.</summary>
    internal class TileGroup
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The tiles in the group.</summary>
        public TileData[] Tiles { get; }

        /// <summary>A border color to draw along edges that aren't touching another tile in the group (if any).</summary>
        public Color? OuterBorderColor { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="tiles">The tiles in the group.</param>
        /// <param name="outerBorderColor">A border color to draw along edges that aren't touching another tile in the group (if any).</param>
        public TileGroup(TileData[] tiles, Color? outerBorderColor = null)
        {
            this.Tiles = tiles;
            this.OuterBorderColor = outerBorderColor;
        }
    }
}
