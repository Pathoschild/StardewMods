using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.TractorMod.Framework.Config;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using SFarmer = StardewValley.Farmer;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.TractorMod.Framework.Attachments
{
    /// <summary>An attachment for the axe.</summary>
    internal class AxeAttachment : BaseAttachment
    {
        /*********
        ** Properties
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
        public AxeAttachment(AxeConfig config)
        {
            this.Config = config;
        }

        /// <summary>Get whether the tool is currently enabled.</summary>
        /// <param name="player">The current player.</param>
        /// <param name="tool">The tool selected by the player (if any).</param>
        /// <param name="item">The item selected by the player (if any).</param>
        /// <param name="location">The current location.</param>
        public override bool IsEnabled(SFarmer player, Tool tool, Item item, GameLocation location)
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
        public override bool Apply(Vector2 tile, SObject tileObj, TerrainFeature tileFeature, SFarmer player, Tool tool, Item item, GameLocation location)
        {
            // clear twigs & weeds
            if (this.Config.ClearDebris && (tileObj?.Name == "Twig" || this.IsWeed(tileObj)))
                return this.UseToolOnTile(tool, tile);

            // check terrain feature
            switch (tileFeature)
            {
                // cut non-fruit tree
                case Tree tree:
                    if (tree.tapped.Value ? this.Config.CutTappedTrees : this.Config.CutTrees)
                        return this.UseToolOnTile(tool, tile);
                    break;

                // cut fruit tree
                case FruitTree _:
                    if (this.Config.CutFruitTrees)
                        return this.UseToolOnTile(tool, tile);
                    break;

                // clear crops
                case HoeDirt dirt when dirt.crop != null:
                    if (this.Config.ClearDeadCrops && dirt.crop.dead.Value)
                        return this.UseToolOnTile(tool, tile);
                    if (this.Config.ClearLiveCrops && !dirt.crop.dead.Value)
                        return this.UseToolOnTile(tool, tile);
                    break;
            }

            // clear stumps
            // This needs to check if the axe upgrade level is high enough first, to avoid spamming
            // 'need to upgrade your tool' messages. Based on ResourceClump.performToolAction.
            if (this.Config.ClearDebris)
            {
                ResourceClump clump = this.GetResourceClumpCoveringTile(location, tile);
                if (clump != null && this.ResourceUpgradeLevelsNeeded.ContainsKey(clump.parentSheetIndex.Value) && tool.UpgradeLevel >= this.ResourceUpgradeLevelsNeeded[clump.parentSheetIndex.Value])
                    this.UseToolOnTile(tool, tile);
            }

            return false;
        }
    }
}
