using System.Collections.Generic;
using Microsoft.Xna.Framework;
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
        /// <summary>The config settings for the axe attachment.</summary>
        private readonly Config.AxeConfig config;

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
        /// <param name="config">The mod configuration.</param>
        public AxeAttachment(Config.AxeConfig config)
        {
            this.config = config;
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
            if (this.config.ClearDebris && tileObj?.Name == "Twig" || tileObj?.Name.ToLower().Contains("weed") == true)
                return this.UseToolOnTile(tool, tile);

            // cut non-fruit tree
            if (tileFeature is Tree tree)
            {
                if (tree.tapped)
                    return this.config.CutTappedTrees && this.UseToolOnTile(tool, tile);
                else
                    return this.config.CutTrees && this.UseToolOnTile(tool, tile);
            }

            // cut fruit tree
            if (tileFeature is FruitTree)
                return this.config.CutFruitTrees && this.UseToolOnTile(tool, tile);

            // clear dead crops
            if (tileFeature is HoeDirt dirt && dirt.crop != null)
            {
                if (dirt.crop.dead && this.config.ClearDeadCrops)
                    return this.UseToolOnTile(tool, tile);
                else if (!dirt.crop.dead && this.config.ClearLiveCrops)
                    return this.UseToolOnTile(tool, tile);
            }

            // clear stumps
            // This needs to check if the axe upgrade level is high enough first, to avoid spamming
            // 'need to upgrade your tool' messages. Based on ResourceClump.performToolAction.
            {
                ResourceClump clump = this.GetResourceClumpCoveringTile(location, tile);
                if (this.config.ClearDebris && clump != null && this.ResourceUpgradeLevelsNeeded.ContainsKey(clump.parentSheetIndex) && tool.upgradeLevel >= this.ResourceUpgradeLevelsNeeded[clump.parentSheetIndex])
                    this.UseToolOnTile(tool, tile);
            }
            
            return false;
        }
    }
}
