using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
            if (tree.stump || growth == WildTreeGrowthStage.Seed)
                return tile;

            // tree
            int height = Tree.treeTopSourceRect.Height * Game1.pixelZoom;
            int width = Tree.treeTopSourceRect.Width * Game1.pixelZoom;
            int x = tile.X + (tile.Width / 2) - width / 2;
            int y = tile.Y + tile.Height - height;

            return new Rectangle(x, y, width, height);
        }

        /// <summary>Get whether the visible sprite intersects the specified coordinate. This can be an expensive test.</summary>
        /// <param name="tile">The tile to search.</param>
        /// <param name="position">The viewport-relative coordinates to search.</param>
        /// <param name="spriteArea">The approximate sprite area calculated by <see cref="GenericTarget.GetSpriteArea"/>.</param>
        /// <remarks>Reverse engineered from <see cref="Tree.draw"/>.</remarks>
        public override bool SpriteIntersectsPixel(Vector2 tile, Vector2 position, Rectangle spriteArea)
        {
            // get tree data
            Tree tree = (Tree)this.Value;
            Texture2D spriteSheet = GameHelper.GetPrivateField<Texture2D>(tree, "texture");
            WildTreeGrowthStage growth = (WildTreeGrowthStage)tree.growthStage;

            // check stump sprite
            Rectangle stumpSpriteArea = new Rectangle(spriteArea.Center.X - (Tree.stumpSourceRect.Width / 2 * Game1.pixelZoom), spriteArea.Y + spriteArea.Height - Tree.stumpSourceRect.Height * Game1.pixelZoom, Tree.stumpSourceRect.Width * Game1.pixelZoom, Tree.stumpSourceRect.Height * Game1.pixelZoom);
            if (stumpSpriteArea.Contains((int)position.X, (int)position.Y) && this.SpriteIntersectsPixel(tile, position, stumpSpriteArea, spriteSheet, Tree.stumpSourceRect))
                return true;

            // check treetop sprite
            if (!tree.stump && growth != WildTreeGrowthStage.Seed)
                return this.SpriteIntersectsPixel(tile, position, spriteArea, spriteSheet, Tree.treeTopSourceRect);

            return false;
        }
    }
}