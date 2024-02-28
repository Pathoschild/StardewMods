using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.TerrainFeatures
{
    /// <summary>Positional metadata about a bush.</summary>
    internal class BushTarget : GenericTarget<Bush>
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="value">The underlying in-game entity.</param>
        /// <param name="getSubject">Get the subject info about the target.</param>
        public BushTarget(GameHelper gameHelper, Bush value, Func<ISubject> getSubject)
            : base(gameHelper, SubjectType.Bush, value, value.Tile, getSubject) { }

        /// <summary>Get the sprite's source rectangle within its texture.</summary>
        public override Rectangle GetSpritesheetArea()
        {
            Bush bush = this.Value;
            return bush.sourceRect.Value;
        }

        /// <summary>Get a rectangle which roughly bounds the visible sprite relative the viewport.</summary>
        /// <remarks>Reverse-engineered from <see cref="Tree.draw"/>.</remarks>
        public override Rectangle GetWorldArea()
        {
            return this.GetSpriteArea(this.Value.getBoundingBox(), this.GetSpritesheetArea());
        }

        /// <summary>Get whether the visible sprite intersects the specified coordinate. This can be an expensive test.</summary>
        /// <param name="tile">The tile to search.</param>
        /// <param name="position">The viewport-relative coordinates to search.</param>
        /// <param name="spriteArea">The approximate sprite area calculated by <see cref="GetWorldArea"/>.</param>
        /// <remarks>Reverse engineered from <see cref="Tree.draw"/>.</remarks>
        public override bool SpriteIntersectsPixel(Vector2 tile, Vector2 position, Rectangle spriteArea)
        {
            SpriteEffects spriteEffects = this.Value.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            return this.SpriteIntersectsPixel(tile, position, spriteArea, Bush.texture.Value, this.GetSpritesheetArea(), spriteEffects);
        }
    }
}
