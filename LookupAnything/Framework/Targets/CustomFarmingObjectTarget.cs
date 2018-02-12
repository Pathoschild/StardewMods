using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common.Integrations.CustomFarmingRedux;
using StardewModdingAPI;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Targets
{
    /// <summary>Positional metadata about a custom world object from Custom Farming.</summary>
    internal class CustomFarmingObjectTarget : ObjectTarget
    {
        /*********
        ** Public methods
        *********/
        /// <summary>The Custom Farming Redux sprite info (if any).</summary>
        private readonly CustomSprite CustomSprite;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="obj">The underlying in-game object.</param>
        /// <param name="tilePosition">The object's tile position in the current location (if applicable).</param>
        /// <param name="reflection">Simplifies access to private game code.</param>
        /// <param name="customFarming">Handles the logic for integrating with the Custom Farming Redux mod.</param>
        public CustomFarmingObjectTarget(Object obj, Vector2? tilePosition, IReflectionHelper reflection, CustomFarmingReduxIntegration customFarming)
            : base(obj, tilePosition, reflection)
        {
            this.CustomSprite = customFarming.IsLoaded
                ? customFarming.GetTexture(obj)
                : null;
        }

        /// <summary>Get a rectangle which roughly bounds the visible sprite relative the viewport.</summary>
        public override Rectangle GetSpriteArea()
        {
            if (this.CustomSprite == null)
                return base.GetSpriteArea();

            Object obj = (Object)this.Value;
            Rectangle boundingBox = obj.getBoundingBox(this.GetTile());
            Rectangle spriteArea = this.GetSpriteArea(boundingBox, this.CustomSprite.SourceRectangle);
            return new Rectangle(
                x: spriteArea.X,
                y: spriteArea.Y - (spriteArea.Height / 2), // custom sprite areas are offset from game logic
                width: spriteArea.Width,
                height: spriteArea.Height
            );
        }

        /// <summary>Get whether the visible sprite intersects the specified coordinate. This can be an expensive test.</summary>
        /// <param name="tile">The tile to search.</param>
        /// <param name="position">The viewport-relative coordinates to search.</param>
        /// <param name="spriteArea">The approximate sprite area calculated by <see cref="GetSpriteArea"/>.</param>
        public override bool SpriteIntersectsPixel(Vector2 tile, Vector2 position, Rectangle spriteArea)
        {
            if (this.CustomSprite == null)
                return base.SpriteIntersectsPixel(tile, position, spriteArea);

            Object obj = (Object)this.Value;

            // get sprite data
            Texture2D spriteSheet = this.CustomSprite.Spritesheet;
            Rectangle sourceRectangle = this.CustomSprite.SourceRectangle;

            // check pixel from sprite sheet
            SpriteEffects spriteEffects = obj.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            return this.SpriteIntersectsPixel(tile, position, spriteArea, spriteSheet, sourceRectangle, spriteEffects);
        }
    }
}
