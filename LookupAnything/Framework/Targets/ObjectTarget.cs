using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace Pathoschild.LookupAnything.Framework.Targets
{
    /// <summary>Positional metadata about a world object.</summary>
    internal class ObjectTarget : GenericTarget
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="obj">The underlying in-game object.</param>
        /// <param name="tilePosition">The object's tile position in the current location (if applicable).</param>
        public ObjectTarget(Object obj, Vector2? tilePosition = null)
            : base(TargetType.Object, obj, tilePosition) { }

        /// <summary>Get a rectangle which roughly bounds the visible sprite relative the viewport.</summary>
        public override Rectangle GetSpriteArea()
        {
            Object obj = (Object)this.Value;
            Rectangle boundingBox = obj.getBoundingBox(this.GetTile());
            if (obj.bigCraftable)
                return this.GetSpriteArea(boundingBox, Object.getSourceRectForBigCraftable(obj.parentSheetIndex));
            if (obj is Fence)
                return this.GetSpriteArea(boundingBox, this.GetSourceRectangle((Fence) obj, Game1.currentLocation));
            else
                return this.GetSpriteArea(boundingBox, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, obj.parentSheetIndex, Object.spriteSheetTileSize, Object.spriteSheetTileSize));
        }

        /// <summary>Get whether the visible sprite intersects the specified coordinate. This can be an expensive test.</summary>
        /// <param name="tile">The tile to search.</param>
        /// <param name="position">The viewport-relative coordinates to search.</param>
        /// <param name="spriteArea">The approximate sprite area calculated by <see cref="GetSpriteArea"/>.</param>
        public override bool SpriteIntersectsPixel(Vector2 tile, Vector2 position, Rectangle spriteArea)
        {
            Object obj = (Object)this.Value;

            // get sprite data
            Texture2D spriteSheet;
            Rectangle sourceRectangle;
            if (obj.bigCraftable)
            {
                spriteSheet = Game1.bigCraftableSpriteSheet;
                sourceRectangle = Object.getSourceRectForBigCraftable(obj.parentSheetIndex);
            }
            else if (obj is Fence)
            {
                spriteSheet = GameHelper.GetPrivateField<Texture2D>(obj, "fenceTexture");
                sourceRectangle = this.GetSourceRectangle((Fence)obj, Game1.currentLocation);
            }
            else
            {
                spriteSheet = Game1.objectSpriteSheet;
                sourceRectangle = Game1.currentLocation.getSourceRectForObject(obj.ParentSheetIndex);
            }

            // check pixel from sprite sheet
            SpriteEffects spriteEffects = obj.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            return this.SpriteIntersectsPixel(tile, position, spriteArea, spriteSheet, sourceRectangle, spriteEffects);
        }

        /// <summary>Get the source rectangle for a fence texture.</summary>
        /// <param name="fence">The fence object.</param>
        /// <param name="location">The location containing the fence target.</param>
        /// <remarks>Reverse-engineered from <see cref="Fence.draw(SpriteBatch,int,int,float)"/>.</remarks>
        private Rectangle GetSourceRectangle(Fence fence, GameLocation location)
        {
            int spriteID = 1;
            if (fence.health > 1.0)
            {
                int index = 0;
                Vector2 tile = fence.tileLocation;

                // connected to right fence
                tile.X += 1;
                if (location.objects.ContainsKey(tile) && location.objects[tile].GetType() == typeof(Fence) && ((Fence)location.objects[tile]).countsForDrawing(fence.whichType))
                    index += 100;

                // connected to left fence
                tile.X -= 2;
                if (location.objects.ContainsKey(tile) && location.objects[tile].GetType() == typeof(Fence) && ((Fence)location.objects[tile]).countsForDrawing(fence.whichType))
                    index += 10;

                // connected to top fence
                tile.X += 1;
                tile.Y += 1;
                if (location.objects.ContainsKey(tile) && location.objects[tile].GetType() == typeof(Fence) && ((Fence)location.objects[tile]).countsForDrawing(fence.whichType))
                    index += 500;

                // connected to bottom fence
                tile.Y -= 2;
                if (location.objects.ContainsKey(tile) && location.objects[tile].GetType() == typeof(Fence) && ((Fence)location.objects[tile]).countsForDrawing(fence.whichType))
                    index += 1000;
                if (fence.isGate)
                {
                    if (index == 110)
                        return new Rectangle(fence.gatePosition == Fence.gateOpenedPosition ? 24 : 0, 128, 24, 32);
                    if (index == 1500)
                        return new Rectangle(fence.gatePosition == Fence.gateClosedPosition ? 16 : 0, 160, 16, 16);
                    spriteID = Fence.sourceRectForSoloGate;
                }
                else
                    spriteID = Fence.fenceDrawGuide[index];
            }

            Texture2D texture = GameHelper.GetPrivateField<Texture2D>(fence, "fenceTexture");
            return new Rectangle(spriteID * Fence.fencePieceWidth % texture.Bounds.Width, spriteID * Fence.fencePieceWidth / texture.Bounds.Width * Fence.fencePieceHeight, Fence.fencePieceWidth, Fence.fencePieceHeight);
        }
    }
}