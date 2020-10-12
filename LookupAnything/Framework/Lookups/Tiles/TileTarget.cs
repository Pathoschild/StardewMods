using System;
using Microsoft.Xna.Framework;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Tiles
{
    /// <summary>Positional metadata about a map tile.</summary>
    internal class TileTarget : GenericTarget<Vector2>
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="position">The tile position.</param>
        /// <param name="getSubject">Get the target subject.</param>
        public TileTarget(GameHelper gameHelper, Vector2 position, Func<ISubject> getSubject)
            : base(gameHelper, SubjectType.Tile, position, position, getSubject) { }

        /// <summary>Get the sprite's source rectangle within its texture.</summary>
        public override Rectangle GetSpritesheetArea()
        {
            return Rectangle.Empty;
        }
    }
}
