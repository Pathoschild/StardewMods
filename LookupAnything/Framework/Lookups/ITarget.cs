using System;
using Microsoft.Xna.Framework;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups
{
    /// <summary>Positional metadata about an object in the world.</summary>
    internal interface ITarget
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The subject type.</summary>
        SubjectType Type { get; }

        /// <summary>The object's tile position in the current location (if applicable).</summary>
        Vector2 Tile { get; }

        /// <summary>Get the subject info about the target.</summary>
        Func<ISubject> GetSubject { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get the sprite's source rectangle within its texture.</summary>
        Rectangle GetSpritesheetArea();

        /// <summary>Get a rectangle which roughly bounds the visible sprite relative the viewport.</summary>
        Rectangle GetWorldArea();

        /// <summary>Get whether the visible sprite intersects the specified coordinate. This can be an expensive test.</summary>
        /// <param name="tile">The tile to search.</param>
        /// <param name="position">The viewport-relative coordinates to search.</param>
        /// <param name="spriteArea">The approximate sprite area calculated by <see cref="GetWorldArea"/>.</param>
        bool SpriteIntersectsPixel(Vector2 tile, Vector2 position, Rectangle spriteArea);
    }
}
