using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace Pathoschild.LookupAnything.Framework.Targets
{
    /// <summary>Positional metadata about a crop.</summary>
    internal class CropTarget : GenericTarget
    {
        /*********
        ** Properties
        *********/
        /// <summary>The underlying crop.</summary>
        private readonly Crop Crop;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="obj">The underlying in-game object.</param>
        /// <param name="tilePosition">The object's tile position in the current location (if applicable).</param>
        public CropTarget(TerrainFeature obj, Vector2? tilePosition = null)
            : base(TargetType.Crop, obj, tilePosition)
        {
            this.Crop = ((HoeDirt)obj).crop;
        }

        /// <summary>Get a rectangle which roughly bounds the visible sprite.</summary>
        public override Rectangle GetSpriteArea()
        {
            Crop crop = this.Crop;

            // get source rectangle
            Rectangle sourceRectangle = this.GetSourceRectangle(crop);

            // derive sprite area
            Rectangle tile = base.GetSpriteArea();
            int height = sourceRectangle.Height * Game1.pixelZoom;
            int width = sourceRectangle.Width * Game1.pixelZoom;
            int x = tile.X; // crops don't extend horizontally
            int y = tile.Y + tile.Height - height;
            return new Rectangle(x, y, width, height);
        }

        /// <summary>Get whether the visible sprite intersects the specified coordinate. This can be an expensive test.</summary>
        /// <param name="tile">The tile to search.</param>
        /// <param name="position">The viewport-relative coordinates to search.</param>
        /// <param name="spriteArea">The approximate sprite area calculated by <see cref="GenericTarget.GetSpriteArea"/>.</param>
        /// <remarks>Derived from <see cref="StardewValley.Crop.draw"/>.</remarks>
        public override bool SpriteIntersectsPixel(Vector2 tile, Vector2 position, Rectangle spriteArea)
        {
            Crop crop = this.Crop;
            SpriteEffects spriteEffects = crop.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // base crop
            if (this.SpriteIntersectsPixel(tile, position, spriteArea, Game1.cropSpriteSheet, this.GetSourceRectangle(crop), spriteEffects))
                return true;

            // crop in last phase (may have fruit, be identical to base crop, or be blank)
            if (!crop.tintColor.Equals(Color.White) && crop.currentPhase == crop.phaseDays.Count - 1 && !crop.dead)
            {
                var sourceRectangle = new Rectangle(
                    x: (crop.fullyGrown ? (crop.dayOfCurrentPhase <= 0 ? 6 : 7) : crop.currentPhase + 1 + 1) * 16 + (crop.rowInSpriteSheet % 2 != 0 ? 128 : 0),
                    y: crop.rowInSpriteSheet / 2 * 16 * 2,
                    width: 16,
                    height: 32
                );
                return this.SpriteIntersectsPixel(tile, position, spriteArea, Game1.cropSpriteSheet, sourceRectangle, spriteEffects);
            }

            return false;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the crop's source rectangle in the sprite sheet.</summary>
        /// <param name="crop">The crop.</param>
        private Rectangle GetSourceRectangle(Crop crop)
        {
            MethodInfo method = GameHelper.GetPrivateMethod(crop, "getSourceRect");
            return (Rectangle)method.Invoke(crop, new object[] { crop.rowInSpriteSheet });
        }
    }
}