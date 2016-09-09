using Microsoft.Xna.Framework;
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
            FruitTreeGrowthStage growth = (FruitTreeGrowthStage)tree.growthStage;

            // tile
            if (tree.stump || growth == FruitTreeGrowthStage.Seed)
                return base.GetSpriteArea();

            // tree
            Rectangle tile = base.GetSpriteArea();
            int height = 80 * Game1.pixelZoom;
            int width = 48 * Game1.pixelZoom;
            int x = tile.X + (tile.Width / 2) - width / 2;
            int y = tile.Y + tile.Height - height;

            return new Rectangle(x, y, width, height);
        }
    }
}