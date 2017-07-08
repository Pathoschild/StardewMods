using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Targets
{
    /// <summary>Positional metadata about a farm animal.</summary>
    internal class FarmAnimalTarget : GenericTarget
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="obj">The underlying in-game object.</param>
        /// <param name="tilePosition">The object's tile position in the current location (if applicable).</param>
        public FarmAnimalTarget(FarmAnimal obj, Vector2? tilePosition = null)
            : base(TargetType.FarmAnimal, obj, tilePosition) { }

        /// <summary>Get a rectangle which roughly bounds the visible sprite relative the viewport.</summary>
        public override Rectangle GetSpriteArea()
        {
            FarmAnimal animal = (FarmAnimal)this.Value;
            return this.GetSpriteArea(animal.GetBoundingBox(), animal.Sprite.SourceRect);
        }

        /// <summary>Get whether the visible sprite intersects the specified coordinate. This can be an expensive test.</summary>
        /// <param name="tile">The tile to search.</param>
        /// <param name="position">The viewport-relative coordinates to search.</param>
        /// <param name="spriteArea">The approximate sprite area calculated by <see cref="GenericTarget.GetSpriteArea"/>.</param>
        public override bool SpriteIntersectsPixel(Vector2 tile, Vector2 position, Rectangle spriteArea)
        {
            FarmAnimal animal = (FarmAnimal)this.Value;
            SpriteEffects spriteEffects = animal.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            return this.SpriteIntersectsPixel(tile, position, spriteArea, animal.sprite.Texture, animal.sprite.sourceRect, spriteEffects);
        }
    }
}
