using System;

namespace Pathoschild.Stardew.DataMaps.Framework
{
    /// <summary>A tile edge direction.</summary>
    [Flags]
    internal enum TileEdge
    {
        /// <summary>No edge.</summary>
        None = 0,

        /// <summary>The top tile edge.</summary>
        Top = 1,

        /// <summary>The left tile edge.</summary>
        Left = 2,

        /// <summary>The right tile edge.</summary>
        Right = 4,

        /// <summary>The bottom tile edge.</summary>
        Bottom = 8
    }
}
