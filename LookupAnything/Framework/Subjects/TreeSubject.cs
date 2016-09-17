using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.LookupAnything.Framework.Constants;
using Pathoschild.LookupAnything.Framework.Fields;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace Pathoschild.LookupAnything.Framework.Subjects
{
    /// <summary>Describes a non-fruit tree.</summary>
    internal class TreeSubject : BaseSubject
    {
        /*********
        ** Properties
        *********/
        /// <summary>The underlying target.</summary>
        private readonly Tree Target;

        /// <summary>The tree's tile position.</summary>
        private readonly Vector2 Tile;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="tree">The lookup target.</param>
        /// <param name="tile">The tree's tile position.</param>
        public TreeSubject(Tree tree, Vector2 tile)
            : base(TreeSubject.GetName(tree), null, "Tree")
        {
            this.Target = tree;
            this.Tile = tile;
        }

        /// <summary>Get the data to display for this subject.</summary>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        /// <remarks>Tree growth algorithm reverse engineered from <see cref="StardewValley.TerrainFeatures.Tree.dayUpdate"/>.</remarks>
        public override IEnumerable<ICustomField> GetData(Metadata metadata)
        {
            Tree tree = this.Target;

            // get growth stage
            WildTreeGrowthStage stage = (WildTreeGrowthStage)Math.Min(tree.growthStage, (int)WildTreeGrowthStage.Tree);
            bool isFullyGrown = stage == WildTreeGrowthStage.Tree;
            yield return isFullyGrown
                ? new GenericField("Growth stage", "fully grown")
                : new GenericField("Growth stage", $"{stage} ({(int)stage} of {(int)WildTreeGrowthStage.Tree})");

            // get growth scheduler
            if (!isFullyGrown)
            {
                if (Game1.IsWinter && Game1.currentLocation.Name != Constant.LocationNames.Greenhouse)
                    yield return new GenericField("Next growth", "can't grow in winter outside greenhouse");
                else if (stage == WildTreeGrowthStage.SmallTree && this.HasAdjacentTrees(this.Tile))
                    yield return new GenericField("Next growth", "can't grow because other trees are too close");
                else
                    yield return new GenericField("Next growth", $"20% chance to grow into {stage + 1} tomorrow");
            }

            // get seed
            if (isFullyGrown)
                yield return new GenericField("Has seed", tree.hasSeed);
        }

        /// <summary>Draw the subject portrait (if available).</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="size">The size of the portrait to draw.</param>
        /// <returns>Returns <c>true</c> if a portrait was drawn, else <c>false</c>.</returns>
        public override bool DrawPortrait(SpriteBatch spriteBatch, Vector2 position, Vector2 size)
        {
            this.Target.drawInMenu(spriteBatch, position, Vector2.Zero, 1, 1);
            return true;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get a display name for the tree.</summary>
        /// <param name="tree">The tree object.</param>
        private static string GetName(Tree tree)
        {
            TreeType type = (TreeType)tree.treeType;

            switch (type)
            {
                case TreeType.Maple:
                    return "Maple Tree";
                case TreeType.Oak:
                    return "Oak Tree";
                case TreeType.Pine:
                    return "Pine Tree";
                case TreeType.Palm:
                    return "Palm Tree";
                case TreeType.BigMushroom:
                    return "Big Mushroom";
                default:
                    return "(unknown tree)";
            }
        }

        /// <summary>Whether there are adjacent trees that prevent growth.</summary>
        /// <param name="position">The tree's position in the current location.</param>
        private bool HasAdjacentTrees(Vector2 position)
        {
            GameLocation location = Game1.currentLocation;
            return (
                from adjacentTile in Utility.getSurroundingTileLocationsArray(position)
                let otherTree = location.terrainFeatures.ContainsKey(adjacentTile)
                    ? location.terrainFeatures[adjacentTile] as Tree
                    : null
                select otherTree != null && otherTree.growthStage >= (int)WildTreeGrowthStage.SmallTree
            ).Any(p => p);
        }
    }
}