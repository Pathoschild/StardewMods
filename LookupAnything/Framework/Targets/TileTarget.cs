using Microsoft.Xna.Framework;

namespace Pathoschild.Stardew.LookupAnything.Framework.Targets
{
    /// <summary>Positional metadata about a map tile.</summary>
    internal class TileTarget : GenericTarget
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="position">The tile position.</param>
        public TileTarget(Vector2 position)
            : base(TargetType.Tile, position, position) { }
    }
}
