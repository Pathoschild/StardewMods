using Microsoft.Xna.Framework;
using StardewValley.TerrainFeatures;

namespace Pathoschild.LookupAnything.Framework.Targets
{
    /// <summary>Positional metadata about a wild tree.</summary>
    public class TreeTarget : GenericTarget
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="obj">The underlying in-game object.</param>
        /// <param name="tilePosition">The object's tile position in the current location (if applicable).</param>
        public TreeTarget(Tree obj, Vector2? tilePosition = null)
            : base(TargetType.WildTree, obj, tilePosition) { }
    }
}