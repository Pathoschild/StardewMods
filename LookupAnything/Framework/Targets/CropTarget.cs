using Microsoft.Xna.Framework;
using StardewValley.TerrainFeatures;

namespace Pathoschild.LookupAnything.Framework.Targets
{
    /// <summary>Positional metadata about a crop.</summary>
    public class CropTarget : GenericTarget
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="obj">The underlying in-game object.</param>
        /// <param name="tilePosition">The object's tile position in the current location (if applicable).</param>
        public CropTarget(TerrainFeature obj, Vector2? tilePosition = null)
            : base(TargetType.Crop, obj, tilePosition) { }
    }
}