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

        /// <summary>Whether to draw borders along edges that aren't touching another tile in the group.</summary>
        public bool OuterBorders { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="tiles">The tiles in the group.</param>
        /// <param name="outerBorders">Whether to draw borders along edges that aren't touching another tile in the group.</param>
        public TileGroup(TileData[] tiles, bool outerBorders = false)
        {
            this.Tiles = tiles;
            this.OuterBorders = outerBorders;
        }
    }
}
