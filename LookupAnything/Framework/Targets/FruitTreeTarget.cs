using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common.Integrations.JsonAssets;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.LookupAnything.Framework.Targets
{
    /// <summary>Positional metadata about a fruit tree.</summary>
    internal class FruitTreeTarget : GenericTarget<FruitTree>
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying tree texture.</summary>
        private readonly Texture2D Texture;

        /// <summary>The source rectangle containing the tree sprites in the <see cref="Texture"/>.</summary>
        private readonly Rectangle SourceRect;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="value">The underlying in-game entity.</param>
        /// <param name="jsonAssets">The Json Assets API.</param>
        /// <param name="tilePosition">The object's tile position in the current location (if applicable).</param>
        public FruitTreeTarget(GameHelper gameHelper, FruitTree value, JsonAssetsIntegration jsonAssets, Vector2? tilePosition = null)
            : base(gameHelper, SubjectType.FruitTree, value, tilePosition)
        {
            this.GetSpriteSheet(value, jsonAssets, out this.Texture, out this.SourceRect);
        }

        /// <summary>Get the sprite's source rectangle within its texture.</summary>
        public override Rectangle GetSpritesheetArea()
        {
            FruitTree tree = this.Value;

            // stump
            if (tree.stump.Value)
                return new Rectangle(this.SourceRect.X + 384, this.SourceRect.Y + 48, 48, 32);

            // growing tree
            if (tree.growthStage.Value < 4)
            {
                switch (tree.growthStage.Value)
                {
                    case 0:
                    case 1:
                    case 2:
                        return new Rectangle(this.SourceRect.X + (tree.growthStage.Value * 48), this.SourceRect.Y, 48, 80);

                    default:
                        return new Rectangle(this.SourceRect.X + 144, this.SourceRect.Y, 48, 80);
                }
            }

            // grown tree
            return new Rectangle(this.SourceRect.X + ((12 + (tree.GreenHouseTree ? 1 : Utility.getSeasonNumber(Game1.currentSeason)) * 3) * 16), this.SourceRect.Y, 48, 16 + 64);
        }

        /// <summary>Get a rectangle which roughly bounds the visible sprite relative the viewport.</summary>
        /// <remarks>Reverse-engineered from <see cref="FruitTree.draw"/>.</remarks>
        public override Rectangle GetWorldArea()
        {
            FruitTree tree = this.Value;
            Rectangle sprite = this.GetSpritesheetArea();

            int width = sprite.Width * Game1.pixelZoom;
            int height = sprite.Height * Game1.pixelZoom;
            int x, y;
            if (tree.growthStage.Value < 4)
            {
                // apply crazy offset logic for growing fruit trees
                Vector2 tile = this.GetTile();
                Vector2 offset = new Vector2((float)Math.Max(-8.0, Math.Min(Game1.tileSize, Math.Sin(tile.X * 200.0 / (2.0 * Math.PI)) * -16.0)), (float)Math.Max(-8.0, Math.Min(Game1.tileSize, Math.Sin(tile.X * 200.0 / (2.0 * Math.PI)) * -16.0)));
                Vector2 centerBottom = new Vector2(tile.X * Game1.tileSize + Game1.tileSize / 2 + offset.X, tile.Y * Game1.tileSize - sprite.Height + Game1.tileSize * 2 + offset.Y) - new Vector2(Game1.viewport.X, Game1.viewport.Y);
                x = (int)centerBottom.X - width / 2;
                y = (int)centerBottom.Y - height;
            }
            else
            {
                // grown trees are centered on tile
                Rectangle tileArea = base.GetWorldArea();
                x = tileArea.Center.X - width / 2;
                y = tileArea.Bottom - height;
            }

            return new Rectangle(x, y, width, height);
        }

        /// <summary>Get whether the visible sprite intersects the specified coordinate. This can be an expensive test.</summary>
        /// <param name="tile">The tile to search.</param>
        /// <param name="position">The viewport-relative coordinates to search.</param>
        /// <param name="spriteArea">The approximate sprite area calculated by <see cref="GetWorldArea"/>.</param>
        /// <remarks>Reverse engineered from <see cref="FruitTree.draw"/>.</remarks>
        public override bool SpriteIntersectsPixel(Vector2 tile, Vector2 position, Rectangle spriteArea)
        {
            SpriteEffects spriteEffects = this.Value.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            return this.SpriteIntersectsPixel(tile, position, spriteArea, this.Texture, this.GetSpritesheetArea(), spriteEffects);
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
        public void GetSpriteSheet(FruitTree target, JsonAssetsIntegration jsonAssets, out Texture2D texture, out Rectangle sourceRect)
        {
            // get from Json Assets
            if (jsonAssets.IsLoaded && jsonAssets.TryGetCustomSpriteSheet(target, out texture, out sourceRect))
                return;

            // use vanilla logic
            texture = FruitTree.texture;
            sourceRect = new Rectangle(x: 0, y: target.treeType.Value * 5 * 16, width: 432, height: 80);
        }
    }
}
