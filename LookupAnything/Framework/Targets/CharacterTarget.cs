using Microsoft.Xna.Framework;
using StardewValley;

namespace Pathoschild.LookupAnything.Framework.Targets
{
    /// <summary>Positional metadata about an NPC.</summary>
    public class CharacterTarget : GenericTarget
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="type">The target type.</param>
        /// <param name="obj">The underlying in-game object.</param>
        /// <param name="tilePosition">The object's tile position in the current location (if applicable).</param>
        public CharacterTarget(TargetType type, NPC obj, Vector2? tilePosition = null)
            : base(type, obj, tilePosition) { }

        /// <summary>Get a rectangle which roughly bounds the visible sprite.</summary>
        public override Rectangle GetSpriteArea()
        {
            NPC npc = (NPC)this.Value;
            var boundingBox = npc.GetBoundingBox(); // the 'occupied' area at the animal's feet

            int height = npc.sprite.spriteHeight * Game1.pixelZoom;
            int width = npc.sprite.spriteWidth * Game1.pixelZoom;
            int x = boundingBox.Center.X - (width / 2);
            int y = boundingBox.Y + boundingBox.Height - height;
            return new Rectangle(x - Game1.viewport.X, y - Game1.viewport.Y, width, height);
        }

        /// <summary>Get whether the visible sprite intersects the specified coordinate. This can be an expensive test.</summary>
        /// <param name="tile">The tile to search.</param>
        /// <param name="position">The viewport-relative coordinates to search.</param>
        /// <param name="spriteArea">The approximate sprite area calculated by <see cref="GenericTarget.GetSpriteArea"/>.</param>
        public override bool SpriteIntersectsPixel(Vector2 tile, Vector2 position, Rectangle spriteArea)
        {
            NPC npc = (NPC)this.Value;
            return this.SpriteIntersectsPixel(tile, position, spriteArea, npc.sprite.Texture, npc.sprite.sourceRect);
        }
    }
}