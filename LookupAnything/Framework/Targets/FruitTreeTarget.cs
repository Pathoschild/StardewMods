using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.LookupAnything.Framework.Targets
{
    /// <summary>Positional metadata about a fruit tree.</summary>
    internal class FruitTreeTarget : GenericTarget
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="obj">The underlying in-game object.</param>
        /// <param name="tilePosition">The object's tile position in the current location (if applicable).</param>
        public FruitTreeTarget(FruitTree obj, Vector2? tilePosition = null)
            : base(TargetType.FruitTree, obj, tilePosition) { }

        /// <summary>Get a rectangle which roughly bounds the visible sprite relative the viewport.</summary>
        /// <remarks>Reverse-engineered from <see cref="FruitTree.draw"/>.</remarks>
        public override Rectangle GetSpriteArea()
        {
            FruitTree tree = (FruitTree)this.Value;
            Rectangle sprite = this.GetSourceRectangle(tree);

            int width = sprite.Width * Game1.pixelZoom;
            int height = sprite.Height * Game1.pixelZoom;
            int x, y;
            if (tree.growthStage < 4)
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
                Rectangle tileArea = base.GetSpriteArea();
                x = tileArea.Center.X - width / 2;
                y = tileArea.Bottom - height;
            }

            return new Rectangle(x, y, width, height);
        }

        /// <summary>Get whether the visible sprite intersects the specified coordinate. This can be an expensive test.</summary>
        /// <param name="tile">The tile to search.</param>
        /// <param name="position">The viewport-relative coordinates to search.</param>
        /// <param name="spriteArea">The approximate sprite area calculated by <see cref="GenericTarget.GetSpriteArea"/>.</param>
        /// <remarks>Reverse engineered from <see cref="FruitTree.draw"/>.</remarks>
        public override bool SpriteIntersectsPixel(Vector2 tile, Vector2 position, Rectangle spriteArea)
        {
            // get tree data
            FruitTree tree = (FruitTree)this.Value;
            Texture2D spriteSheet = FruitTree.texture;
            Rectangle sourceRectangle = this.GetSourceRectangle(tree);

            // check sprite
            SpriteEffects spriteEffects = tree.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            return this.SpriteIntersectsPixel(tile, position, spriteArea, spriteSheet, sourceRectangle, spriteEffects);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the sprite sheet's source rectangle for the displayed sprite.</summary>
        /// <remarks>Reverse-engineered from <see cref="FruitTree.draw"/>.</remarks>
        private Rectangle GetSourceRectangle(FruitTree tree)
        {
            // stump
            if (tree.stump)
                return new Rectangle(384, tree.treeType * 5 * 16 + 48, 48, 32);

            // growing tree
            if (tree.growthStage < 4)
            {
                switch (tree.growthStage)
                {
                    case 0:
                        return new Rectangle(0, tree.treeType * 5 * 16, 48, 80);
                    case 1:
                        return new Rectangle(48, tree.treeType * 5 * 16, 48, 80);
                    case 2:
                        return new Rectangle(96, tree.treeType * 5 * 16, 48, 80);
                    default:
                        return new Rectangle(144, tree.treeType * 5 * 16, 48, 80);
                }
            }

            // grown tree
            return new Rectangle((12 + (tree.greenHouseTree ? 1 : Utility.getSeasonNumber(Game1.currentSeason)) * 3) * 16, tree.treeType * 5 * 16, 48, 16 + 64);
        }
    }
}
