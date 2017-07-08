using Microsoft.Xna.Framework;
using SFarmer = StardewValley.Farmer;

namespace Pathoschild.Stardew.LookupAnything.Framework.Targets
{
    /// <summary>Positional metadata about a farmer (i.e. player).</summary>
    internal class FarmerTarget : GenericTarget
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="farmer">The underlying in-game object.</param>
        public FarmerTarget(SFarmer farmer)
            : base(TargetType.Farmer, farmer, farmer.getTileLocation()) { }

        /// <summary>Get a rectangle which roughly bounds the visible sprite relative the viewport.</summary>
        public override Rectangle GetSpriteArea()
        {
            SFarmer farmer = (SFarmer)this.Value;
            return this.GetSpriteArea(farmer.GetBoundingBox(), farmer.FarmerSprite.SourceRect);
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
