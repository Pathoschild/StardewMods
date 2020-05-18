using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
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


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="config">The attachment settings.</param>
        /// <param name="modRegistry">Fetches metadata about loaded mods.</param>
        /// <param name="reflection">Simplifies access to private code.</param>
        public AxeAttachment(AxeConfig config, IModRegistry modRegistry, IReflectionHelper reflection)
            : base(modRegistry, reflection)
        {
            this.Config = config;
        }

        /// <summary>Get whether the tool is currently enabled.</summary>
        /// <param name="player">The current player.</param>
        /// <param name="tool">The tool selected by the player (if any).</param>
        /// <param name="item">The item selected by the player (if any).</param>
        /// <param name="location">The current location.</param>
        public override bool IsEnabled(Farmer player, Tool tool, Item item, GameLocation location)
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
        public override bool Apply(Vector2 tile, SObject tileObj, TerrainFeature tileFeature, Farmer player, Tool tool, Item item, GameLocation location)
        {
            // clear debris
            if (this.Config.ClearDebris && (this.IsTwig(tileObj) || this.IsWeed(tileObj)))
                return this.UseToolOnTile(tool, tile, player, location);

            // cut terrain features
            switch (tileFeature)
            {
                // cut non-fruit tree
                case Tree tree:
                    return this.ShouldCut(tree) && this.UseToolOnTile(tool, tile, player, location);

                // cut fruit tree
                case FruitTree tree:
                    return this.ShouldCut(tree) && this.UseToolOnTile(tool, tile, player, location);

                // cut bushes
                case Bush bush:
                    return this.ShouldCut(bush) && this.UseToolOnTile(tool, tile, player, location);

                // clear crops
                case HoeDirt dirt when dirt.crop != null:
                    if (this.Config.ClearDeadCrops && dirt.crop.dead.Value)
                        return this.UseToolOnTile(tool, tile, player, location);
                    if (this.Config.ClearLiveCrops && !dirt.crop.dead.Value)
                        return this.UseToolOnTile(tool, tile, player, location);
                    break;
            }

            // cut resource stumps
            if (this.Config.ClearDebris || this.Config.CutGiantCrops)
            {
                ResourceClump clump = this.GetResourceClumpCoveringTile(location, tile, player, out var applyTool);

                // giant crops
                if (this.Config.CutGiantCrops && clump is GiantCrop)
                {
                    applyTool(tool);
                    return true;
                }

                // big stumps and fallen logs
                // This needs to check if the axe upgrade level is high enough first, to avoid spamming
                // 'need to upgrade your tool' messages. Based on ResourceClump.performToolAction.
                if (this.Config.ClearDebris && clump != null && this.ResourceUpgradeLevelsNeeded.ContainsKey(clump.parentSheetIndex.Value) && tool.UpgradeLevel >= this.ResourceUpgradeLevelsNeeded[clump.parentSheetIndex.Value])
                {
                    applyTool(tool);
                    return true;
                }
            }

            // cut bushes in large terrain features
            if (this.Config.CutBushes)
            {
                foreach (Bush bush in location.largeTerrainFeatures.OfType<Bush>().Where(p => p.tilePosition.Value == tile))
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
        private bool ShouldCut(Tree tree)
        {
            var config = this.Config;

            // seed
            if (tree.growthStage.Value == Tree.seedStage)
                return config.ClearTreeSeeds;

            // sapling
            if (tree.growthStage.Value < Tree.treeStage)
                return config.ClearTreeSaplings;

            // full-grown
            return tree.tapped.Value ? config.CutTappedTrees : config.CutGrownTrees;
        }

        /// <summary>Get whether a given tree should be chopped.</summary>
        /// <param name="tree">The tree to check.</param>
        private bool ShouldCut(FruitTree tree)
        {
            var config = this.Config;

            // seed
            if (tree.growthStage.Value == Tree.seedStage)
                return config.ClearFruitTreeSeeds;

            // sapling
            if (tree.growthStage.Value < Tree.treeStage)
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
    }
}
