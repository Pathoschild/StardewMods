using Microsoft.Xna.Framework;
using Pathoschild.LookupAnything.Framework.Constants;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace Pathoschild.LookupAnything.Framework.Targets
{
    /// <summary>Positional metadata about a wild tree.</summary>
    public class TreeTarget : GenericTarget
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="obj">The underlying in-game object.</param>
        /// <param name="tilePosition">The object's tile position in the current location (if applicable).</param>
        public TreeTarget(Tree obj, Vector2? tilePosition = null)
            : base(TargetType.WildTree, obj, tilePosition) { }

        /// <summary>Get a rectangle which roughly bounds the visible sprite.</summary>
        /// <remarks>Reverse-engineered from <see cref="Tree.draw"/>.</remarks>
        public override Rectangle GetSpriteArea()
        {
            Rectangle tile = base.GetSpriteArea();
            Tree tree = (Tree)this.Value;
            WildTreeGrowthStage growth = (WildTreeGrowthStage)tree.growthStage;

            // tile
            if (tree.stump || growth == WildTreeGrowthStage.Seed || growth == WildTreeGrowthStage.Sapling)
                return tile;

            // tree
            // (Wild trees often consist of multiple sprites, but we can approximate the sprite
            // area well enough that we don't need to deal with that complication yet.)
            int height = 80 * Game1.pixelZoom;
            int width = 48 * Game1.pixelZoom;
            int x = tile.X + (tile.Width / 2) - width / 2;
            int y = tile.Y + tile.Height - height;

            return new Rectangle(x, y, width, height);
        }
    }
}