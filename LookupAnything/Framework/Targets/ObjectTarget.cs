using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace Pathoschild.LookupAnything.Framework.Targets
{
    /// <summary>Positional metadata about a world object.</summary>
    public class ObjectTarget : GenericTarget
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="obj">The underlying in-game object.</param>
        /// <param name="tilePosition">The object's tile position in the current location (if applicable).</param>
        public ObjectTarget(Object obj, Vector2? tilePosition = null)
            : base(TargetType.Object, obj, tilePosition) { }

        /// <summary>Get a rectangle which roughly bounds the visible sprite.</summary>
        public override Rectangle GetSpriteArea()
        {
            Object obj = (Object)this.Value;
            Rectangle tileRectangle = base.GetSpriteArea();
            if (obj.bigCraftable)
            {
                Rectangle sourceRectangle = Object.getSourceRectForBigCraftable(obj.parentSheetIndex);
                return new Rectangle(tileRectangle.X, tileRectangle.Y - (sourceRectangle.Height * Game1.pixelZoom) + tileRectangle.Height, sourceRectangle.Width * Game1.pixelZoom, sourceRectangle.Height * Game1.pixelZoom);
            }
            else
            {
                Rectangle sourceRectangle = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, obj.parentSheetIndex);
                return new Rectangle(tileRectangle.X, tileRectangle.Y - sourceRectangle.Height + tileRectangle.Height, sourceRectangle.Width, sourceRectangle.Height);
            }
        }

        /// <summary>Get whether the visible sprite intersects the specified coordinate. This can be an expensive test.</summary>
        /// <param name="tile">The tile to search.</param>
        /// <param name="position">The viewport-relative coordinates to search.</param>
        /// <param name="spriteArea">The approximate sprite area calculated by <see cref="GetSpriteArea"/>.</param>
        public override bool SpriteIntersectsPixel(Vector2 tile, Vector2 position, Rectangle spriteArea)
        {
            Object obj = (Object)this.Value;

            // get sprite sheet
            Texture2D spriteSheet = obj.bigCraftable
                ? Game1.bigCraftableSpriteSheet
                : Game1.objectSpriteSheet;
            Rectangle sourceRectangle = obj.bigCraftable
                ? Object.getSourceRectForBigCraftable(obj.parentSheetIndex)
                : Game1.currentLocation.getSourceRectForObject(obj.ParentSheetIndex);

            // check pixel from sprite sheet
            SpriteEffects spriteEffects = obj.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            return this.SpriteIntersectsPixel(tile, position, spriteArea, spriteSheet, sourceRectangle, spriteEffects);
        }
    }
}