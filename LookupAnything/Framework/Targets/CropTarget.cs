using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common.Integrations.JsonAssets;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.LookupAnything.Framework.Targets
{
    /// <summary>Positional metadata about a crop.</summary>
    internal class CropTarget : GenericTarget<HoeDirt>
    {
        /*********
        ** Fields
        *********/
        /// <summary>Simplifies access to private game code.</summary>
        private readonly IReflectionHelper Reflection;

        /// <summary>The underlying tree texture.</summary>
        private readonly Texture2D Texture;

        /// <summary>The source rectangle containing the current crop sprite in the <see cref="Texture"/>.</summary>
        private readonly Rectangle SourceRect;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="value">The underlying in-game entity.</param>
        /// <param name="tilePosition">The object's tile position in the current location (if applicable).</param>
        /// <param name="reflectionHelper">Simplifies access to private game code.</param>
        /// <param name="jsonAssets">The Json Assets API.</param>
        public CropTarget(GameHelper gameHelper, HoeDirt value, Vector2? tilePosition, IReflectionHelper reflectionHelper, JsonAssetsIntegration jsonAssets)
            : base(gameHelper, SubjectType.Crop, value, tilePosition)
        {
            this.Reflection = reflectionHelper;

            this.GetSpriteSheet(value.crop, jsonAssets, out this.Texture, out this.SourceRect);
        }

        /// <summary>Get the sprite's source rectangle within its texture.</summary>
        public override Rectangle GetSpritesheetArea()
        {
            return this.SourceRect;
        }

        /// <summary>Get a rectangle which roughly bounds the visible sprite relative the viewport.</summary>
        public override Rectangle GetWorldArea()
        {
            return this.GetSpriteArea(this.Value.getBoundingBox(this.GetTile()), this.GetSpritesheetArea());
        }

        /// <summary>Get whether the visible sprite intersects the specified coordinate. This can be an expensive test.</summary>
        /// <param name="tile">The tile to search.</param>
        /// <param name="position">The viewport-relative coordinates to search.</param>
        /// <param name="spriteArea">The approximate sprite area calculated by <see cref="GetWorldArea"/>.</param>
        /// <remarks>Derived from <see cref="StardewValley.Crop.draw"/>.</remarks>
        public override bool SpriteIntersectsPixel(Vector2 tile, Vector2 position, Rectangle spriteArea)
        {
            Crop crop = this.Value.crop;
            SpriteEffects spriteEffects = crop.flip.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // base crop
            if (this.SpriteIntersectsPixel(tile, position, spriteArea, this.Texture, this.GetSpritesheetArea(), spriteEffects))
                return true;

            // crop in last phase (may have fruit, be identical to base crop, or be blank)
            if (crop.tintColor.Value != Color.White && crop.currentPhase.Value == crop.phaseDays.Count - 1 && !crop.dead.Value)
                return this.SpriteIntersectsPixel(tile, position, spriteArea, this.Texture, this.SourceRect, spriteEffects);

            return false;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the in-world sprite sheet for a target.</summary>
        /// <param name="target">The target whose texture to get.</param>
        /// <param name="jsonAssets">The Json Assets API.</param>
        /// <param name="texture">The custom sprite texture.</param>
        /// <param name="sourceRect">The custom area within the texture. </param>
        /// <returns>Returns true if the entity has a custom sprite, else false.</returns>
        private void GetSpriteSheet(Crop target, JsonAssetsIntegration jsonAssets, out Texture2D texture, out Rectangle sourceRect)
        {
            // get from Json Assets
            if (jsonAssets.IsLoaded && jsonAssets.TryGetCustomSpriteSheet(target, out texture, out sourceRect, currentSpriteOnly: true))
                return;

            // use vanilla logic
            texture = Game1.cropSpriteSheet;
            sourceRect = this.Reflection.GetMethod(target, "getSourceRect").Invoke<Rectangle>(target.rowInSpriteSheet.Value);
        }
    }
}
