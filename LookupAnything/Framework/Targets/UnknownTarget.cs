using Microsoft.Xna.Framework;

namespace Pathoschild.Stardew.LookupAnything.Framework.Targets
{
    /// <summary>Positional metadata about an unknown object.</summary>
    internal class UnknownTarget : GenericTarget
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="obj">The underlying in-game object.</param>
        /// <param name="tilePosition">The object's tile position in the current location (if applicable).</param>
        public UnknownTarget(object obj, Vector2? tilePosition = null)
            : base(TargetType.Unknown, obj, tilePosition) { }
    }
}
