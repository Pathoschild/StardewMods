using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.TractorMod.Framework.Config;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.TractorMod.Framework.Attachments
{
    /// <summary>An attachment for the axe.</summary>
    internal class AxeAttachment : BaseAttachment
    {
        /*********
        ** Fields
        *********/
        /// <summary>The attachment settings.</summary>
        private readonly AxeConfig Config;

        /// <summary>The axe upgrade levels needed to break supported resource clumps.</summary>
        /// <remarks>Derived from <see cref="ResourceClump.performToolAction"/>.</remarks>
        private readonly IDictionary<int, int> ResourceUpgradeLevelsNeeded = new Dictionary<int, int>
        {
            [ResourceClump.stumpIndex] = Tool.copper,
            [ResourceClump.hollowLogIndex] = Tool.steel
        };

        /// <summary>Simplifies access to private code.</summary>
        private readonly IReflectionHelper Reflection;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="config">The attachment settings.</param>
        /// <param name="modRegistry">Fetches metadata about loaded mods.</param>
        /// <param name="reflection">Simplifies access to private code.</param>
        public AxeAttachment(AxeConfig config, IModRegistry modRegistry, IReflectionHelper reflection)
            : base(modRegistry)
        {
            this.Config = config;
            this.Reflection = reflection;
        }

        /// <summary>Get whether the tool is currently enabled.</summary>
        /// <param name="player">The current player.</param>
        /// <param name="tool">The tool selected by the player (if any).</param>
        /// <param name="item">The item selected by the player (if any).</param>
        /// <param name="location">The current location.</param>
        public override bool IsEnabled(Farmer player, Tool? tool, Item? item, GameLocation location)
        {
            return tool is Axe;
        }

        /// <summary>Apply the tool to the given tile.</summary>
        /// <param name="tile">The tile to modify.</param>
        /// <param name="tileObj">The object on the tile.</param>
        /// <param name="tileFeature">The feature on the tile.</param>
        /// <param name="player">The current player.</param>
        /// <param name="tool">The tool selected by the player (if any).</param>
        /// <param name="item">The item selected by the player (if any).</param>
        /// <param name="location">The current location.</param>
        public override bool Apply(Vector2 tile, SObject? tileObj, TerrainFeature? tileFeature, Farmer player, Tool? tool, Item? item, GameLocation location)
        {
            tool = tool.AssertNotNull();

            // clear debris
            if (this.Config.ClearDebris && tileObj != null && (tileObj.IsTwig() || tileObj.IsWeeds()))
                return this.UseToolOnTile(tool, tile, player, location);

            // cut terrain features
            switch (tileFeature)
            {
                // cut non-fruit tree
                case Tree tree:
                    return this.ShouldCut(tree, tile, location) && this.UseToolOnTile(tool, tile, player, location);

                // cut fruit tree
                case FruitTree tree:
                    return this.ShouldCut(tree) && this.UseToolOnTile(tool, tile, player, location);

                // cut bushes
                case Bush bush:
                    return this.ShouldCut(bush) && this.UseToolOnTile(tool, tile, player, location);

                // clear crops
                case HoeDirt { crop: not null } dirt:
                    if (this.Config.ClearDeadCrops && dirt.crop.dead.Value)
                        return this.UseToolOnTile(tool, tile, player, location);
                    if (this.Config.ClearLiveCrops && !dirt.crop.dead.Value)
                        return this.UseToolOnTile(tool, tile, player, location);
                    break;
            }

            // cut resource stumps
            if (this.Config.ClearDebris || this.Config.CutGiantCrops)
            {
                if (this.TryGetResourceClumpCoveringTile(location, tile, player, this.Reflection, out ResourceClump? clump, out Func<Tool, bool>? applyTool))
                {
                    // giant crops
                    if (this.Config.CutGiantCrops && clump is GiantCrop)
                    {
                        applyTool(tool);
                        return true;
                    }

                    // big stumps and fallen logs
                    // This needs to check if the axe upgrade level is high enough first, to avoid spamming
                    // 'need to upgrade your tool' messages. Based on ResourceClump.performToolAction.
                    if (this.Config.ClearDebris && this.ResourceUpgradeLevelsNeeded.ContainsKey(clump.parentSheetIndex.Value) && tool.UpgradeLevel >= this.ResourceUpgradeLevelsNeeded[clump.parentSheetIndex.Value])
                    {
                        applyTool(tool);
                        return true;
                    }
                }
            }

            // cut bushes in large terrain features
            if (this.Config.CutBushes)
            {
                foreach (Bush bush in location.largeTerrainFeatures.OfType<Bush>().Where(p => p.Tile == tile))
                {
                    if (this.ShouldCut(bush) && this.UseToolOnTile(tool, tile, player, location))
                        return true;
                }
            }

            return false;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get whether a given tree should be chopped.</summary>
        /// <param name="tree">The tree to check.</param>
        /// <param name="tile">The tile containing the tree.</param>
        /// <param name="location">The location containing the tree.</param>
        private bool ShouldCut(Tree tree, Vector2 tile, GameLocation location)
        {
            var config = this.Config;

            // stump
            if (tree.stump.Value)
                return config.CutTreeStumps;

            // seed
            if (tree.growthStage.Value == Tree.seedStage)
                return config.ClearTreeSeeds;

            // sapling
            if (tree.growthStage.Value < Tree.treeStage)
                return config.ClearTreeSaplings;

            // full-grown
            if (config.CutGrownTrees == config.CutTappedTrees)
                return config.CutGrownTrees;
            return this.IsTapped(tree, tile, location)
                ? config.CutTappedTrees
                : config.CutGrownTrees;
        }

        /// <summary>Get whether a given tree should be chopped.</summary>
        /// <param name="tree">The tree to check.</param>
        private bool ShouldCut(FruitTree tree)
        {
            var config = this.Config;

            // seed
            if (tree.growthStage.Value == FruitTree.seedStage)
                return config.ClearFruitTreeSeeds;

            // sapling
            if (tree.growthStage.Value < FruitTree.treeStage)
                return config.ClearFruitTreeSaplings;

            // full-grown
            return config.CutGrownFruitTrees;
        }

        /// <summary>Get whether a given bush should be chopped.</summary>
        /// <param name="bush">The bush to check.</param>
        private bool ShouldCut(Bush bush)
        {
            var config = this.Config;

            return config.CutBushes;
        }

        /// <summary>Get whether a given tree has a tapper attached.</summary>
        /// <param name="tree">The tree to check.</param>
        /// <param name="tile">The tile containing the tree.</param>
        /// <param name="location">The location containing the tree.</param>
        private bool IsTapped(Tree tree, Vector2 tile, GameLocation location)
        {
            return
                // tree is known tapped
                tree.tapped.Value

                // the game doesn't reliably track heavy tappers, so we need to check manually
                || (
                    location.objects.TryGetValue(tile, out SObject obj)
                    && obj.IsTapper()
                );
        }
    }
}
