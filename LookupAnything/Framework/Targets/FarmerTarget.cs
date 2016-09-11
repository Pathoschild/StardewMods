using Microsoft.Xna.Framework;
using StardewValley;

namespace Pathoschild.LookupAnything.Framework.Targets
{
    /// <summary>Positional metadata about a farmer (i.e. player).</summary>
    public class FarmerTarget : GenericTarget
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="farmer">The underlying in-game object.</param>
        public FarmerTarget(Farmer farmer)
            : base(TargetType.Farmer, farmer, farmer.getTileLocation()) { }

        /// <summary>Get a rectangle which roughly bounds the visible sprite.</summary>
        public override Rectangle GetSpriteArea()
        {
            Farmer npc = (Farmer)this.Value;
            var boundingBox = npc.GetBoundingBox(); // the 'occupied' area at the NPC's feet

            // get bounding box
            int height = npc.FarmerSprite.spriteHeight * Game1.pixelZoom;
            int width = npc.FarmerSprite.spriteWidth * Game1.pixelZoom;
            float x = boundingBox.Center.X - (width / 2);
            float y = boundingBox.Top + boundingBox.Height - height + npc.yJumpOffset * 2;

            return new Rectangle((int)(x - Game1.viewport.X), (int)(y - Game1.viewport.Y), width, height);
        }

        /// <summary>Get whether the visible sprite intersects the specified coordinate. This can be an expensive test.</summary>
        /// <param name="tile">The tile to search.</param>
        /// <param name="position">The viewport-relative coordinates to search.</param>
        /// <param name="spriteArea">The approximate sprite area calculated by <see cref="GenericTarget.GetSpriteArea"/>.</param>
        public override bool SpriteIntersectsPixel(Vector2 tile, Vector2 position, Rectangle spriteArea)
        {
            return spriteArea.Contains((int)position.X, (int)position.Y);
        }
    }
}