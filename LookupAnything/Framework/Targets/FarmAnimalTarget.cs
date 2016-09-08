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

        /// <summary>Get a rectangle which roughly bounds the visible sprite.</summary>
        public override Rectangle GetSpriteArea()
        {
            FarmAnimal animal = (FarmAnimal)this.Value;
            var boundingBox = animal.GetBoundingBox(); // the 'occupied' area at the animal's feet

            int height = animal.sprite.spriteHeight * Game1.pixelZoom;
            int width = animal.sprite.spriteWidth * Game1.pixelZoom;
            int x = boundingBox.Center.X - (width / 2);
            int y = boundingBox.Y + boundingBox.Height - height;
            return new Rectangle(x - Game1.viewport.X, y - Game1.viewport.Y, width, height);
        }
    }
}