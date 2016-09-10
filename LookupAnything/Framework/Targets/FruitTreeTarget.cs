using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.LookupAnything.Framework.Constants;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace Pathoschild.LookupAnything.Framework.Targets
{
    /// <summary>Positional metadata about a fruit tree.</summary>
    public class FruitTreeTarget : GenericTarget
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="obj">The underlying in-game object.</param>
        /// <param name="tilePosition">The object's tile position in the current location (if applicable).</param>
        public FruitTreeTarget(FruitTree obj, Vector2? tilePosition = null)
            : base(TargetType.FruitTree, obj, tilePosition) { }

        /// <summary>Get a rectangle which roughly bounds the visible sprite.</summary>
        /// <remarks>Reverse-engineered from <see cref="FruitTree.draw"/>.</remarks>
        public override Rectangle GetSpriteArea()
        {
            FruitTree tree = (FruitTree)this.Value;
            Rectangle sprite = this.GetSourceRectangle(tree);

            // tree
            Rectangle tile = base.GetSpriteArea();
            int width = sprite.Width * Game1.pixelZoom;
            int height = sprite.Height * Game1.pixelZoom;
            int x = tile.X + (tile.Width / 2) - width / 2;
            int y = tile.Y + tile.Height - height;

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
        private Rectangle GetSourceRectangle(FruitTree tree)
        {
            FruitTreeGrowthStage growth = (FruitTreeGrowthStage)tree.growthStage;

            int row = tree.treeType * 5 * 16;
            int width = 48;
            int height = 80;

            if (tree.stump)
                return new Rectangle(384, row, width, 32);
            else if (growth == FruitTreeGrowthStage.Seed)
                return new Rectangle(0, row, width, 32);
            else if (growth == FruitTreeGrowthStage.Sprout)
                return new Rectangle(48, row, width, height);
            else if (growth == FruitTreeGrowthStage.Sapling)
                return new Rectangle(96, row, width, height);
            else if (growth == FruitTreeGrowthStage.Bush)
                return new Rectangle(144, row, width, height);
            else
                return new Rectangle((12 + (tree.greenHouseTree ? 1 : Utility.getSeasonNumber(Game1.currentSeason)) * 3) * 16, row, width, height);
        }
    }
}