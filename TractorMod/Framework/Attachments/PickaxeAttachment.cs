using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using SFarmer = StardewValley.Farmer;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.TractorMod.Framework.Attachments
{
    /// <summary>An attachment for the pickaxe.</summary>
    internal class PickaxeAttachment : BaseAttachment
    {
        /*********
        ** Properties
        *********/
        /// <summary>The config settings for the pickaxe attachment.</summary>
        private readonly Config.PickAxeConfig config;

        /// <summary>The axe upgrade levels needed to break supported resource clumps.</summary>
        /// <remarks>Derived from <see cref="ResourceClump.performToolAction"/>.</remarks>
        private readonly IDictionary<int, int> ResourceUpgradeLevelsNeeded = new Dictionary<int, int>
        {
            [ResourceClump.meteoriteIndex] = Tool.gold,
            [ResourceClump.boulderIndex] = Tool.steel
        };


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="config">The mod configuration.</param>
        public PickaxeAttachment(Config.PickAxeConfig config)
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
            return tool is Pickaxe;
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
            // break stones
            if (tileObj?.Name == "Stone")
                return this.config.ClearDebris && this.UseToolOnTile(tool, tile);

            // break flooring & paths
            if (tileFeature is Flooring)
                return this.config.ClearFlooring && this.UseToolOnTile(tool, tile);

            // handle dirt
            if (tileFeature is HoeDirt dirt)
            {
                // clear tilled dirt
                if (dirt.crop == null)
                    return this.config.ClearDirt && this.UseToolOnTile(tool, tile);

                // clear dead crops
                if (dirt.crop?.dead == true)
                    return this.config.ClearDeadCrops && this.UseToolOnTile(tool, tile);
            }

            // clear boulders / meteorites
            // This needs to check if the axe upgrade level is high enough first, to avoid spamming
            // 'need to upgrade your tool' messages. Based on ResourceClump.performToolAction.
            {
                ResourceClump clump = this.GetResourceClumpCoveringTile(location, tile);
                if (this.config.ClearBoulders && clump != null && this.ResourceUpgradeLevelsNeeded.ContainsKey(clump.parentSheetIndex) && tool.upgradeLevel >= this.ResourceUpgradeLevelsNeeded[clump.parentSheetIndex])
                    this.UseToolOnTile(tool, tile);
            }

            return false;
        }
    }
}
