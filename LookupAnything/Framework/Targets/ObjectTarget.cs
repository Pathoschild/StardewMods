using Microsoft.Xna.Framework;
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
            // get sprite source rectangle
            Object obj = (Object)this.Value;
            Rectangle sourceRectangle = obj.bigCraftable
                ? Object.getSourceRectForBigCraftable(obj.parentSheetIndex)
                : Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, obj.parentSheetIndex);

            // translate into game coordinates
            Rectangle tileRectangle = base.GetSpriteArea();
            return new Rectangle(tileRectangle.X, tileRectangle.Y - (sourceRectangle.Height * Game1.pixelZoom) + tileRectangle.Height, sourceRectangle.Width * Game1.pixelZoom, sourceRectangle.Height * Game1.pixelZoom);
        }
    }
}