using System;
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
    public class TreeSubject : BaseSubject
    {
        /*********
        ** Properties
        *********/
        /// <summary>The underlying tree.</summary>
        private readonly Tree Tree;

        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="tree">The underlying tree.</param>
        /// <param name="position">The tree's tile position within the current location.</param>
        /// <remarks>Tree growth algorithm reverse engineered from <see cref="StardewValley.TerrainFeatures.Tree.dayUpdate"/>.</remarks>
        public TreeSubject(Tree tree, Vector2 position)
            : base(TreeSubject.GetName(tree), null, "Tree")
        {
            this.Tree = tree;

            // get growth stage
            TreeGrowthStage stage = (TreeGrowthStage)Math.Min(tree.growthStage, (int)TreeGrowthStage.Tree);
            bool isFullyGrown = stage == TreeGrowthStage.Tree;
            this.AddCustomFields(isFullyGrown
                ? new GenericField("Growth stage", "fully grown")
                : new GenericField("Growth stage", $"{stage} ({(int)stage} of {(int)TreeGrowthStage.Tree})")
            );

            // get growth scheduler
            if (!isFullyGrown)
            {
                if (Game1.IsWinter && Game1.currentLocation.Name != StandardLocation.Greenhouse)
                    this.AddCustomFields(new GenericField("Next growth", "can't grow in winter outside greenhouse"));
                else if (stage == TreeGrowthStage.SmallTree && this.HasAdjacentTrees(position))
                    this.AddCustomFields(new GenericField("Next growth", "can't grow because other trees are too close"));
                else
                    this.AddCustomFields(new GenericField("Next growth", $"20% chance to grow into {stage + 1} tomorrow"));
            }

            // get seed
            if (isFullyGrown)
                this.AddCustomFields(new GenericField("Has seed", tree.hasSeed));

            /*
                tree.health;
                tree.tapped;
                */
        }

        /// <summary>Draw the subject portrait (if available).</summary>
        /// <param name="sprites">The sprite batch in which to draw.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="size">The size of the portrait to draw.</param>
        /// <returns>Returns <c>true</c> if a portrait was drawn, else <c>false</c>.</returns>
        public override bool DrawPortrait(SpriteBatch sprites, Vector2 position, Vector2 size)
        {
            this.Tree.drawInMenu(sprites, position, Vector2.Zero, 1, 1);
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
                select otherTree != null && otherTree.growthStage >= (int)TreeGrowthStage.SmallTree
            ).Any(p => p);
        }
    }
}