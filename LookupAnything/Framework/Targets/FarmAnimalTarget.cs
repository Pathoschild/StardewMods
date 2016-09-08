using Microsoft.Xna.Framework;
using StardewValley;

namespace Pathoschild.LookupAnything.Framework.Targets
{
    /// <summary>Positional metadata about a farm animal.</summary>
    public class FarmAnimalTarget : GenericTarget
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="obj">The underlying in-game object.</param>
        /// <param name="tilePosition">The object's tile position in the current location (if applicable).</param>
        public FarmAnimalTarget(FarmAnimal obj, Vector2? tilePosition = null)
            : base(TargetType.FarmAnimal, obj, tilePosition) { }
    }
}