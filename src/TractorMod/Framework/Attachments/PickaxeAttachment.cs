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
    /// <summary>An attachment for the pickaxe.</summary>
    internal class PickaxeAttachment : BaseAttachment
    {
        /*********
        ** Properties
        *********/
        /// <summary>The attachment settings.</summary>
        private readonly PickAxeConfig Config;

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
        /// <param name="config">The attachment settings.</param>
        public PickaxeAttachment(PickAxeConfig config)
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
            if (this.Config.ClearDebris && tileObj?.Name == "Stone")
                return this.UseToolOnTile(tool, tile);

            // break flooring & paths
            if (this.Config.ClearFlooring && tileFeature is Flooring)
                return this.UseToolOnTile(tool, tile);

            // clear twigs & weeds
            if (this.Config.ClearWeeds && this.IsWeed(tileObj))
                return this.UseToolOnTile(tool, tile);

            // handle dirt
            if (tileFeature is HoeDirt dirt)
            {
                // clear tilled dirt
                if (this.Config.ClearDirt && dirt.crop == null)
                    return this.UseToolOnTile(tool, tile);

                // clear dead crops
                if (this.Config.ClearDeadCrops && dirt.crop != null && dirt.crop.dead.Value)
                    return this.UseToolOnTile(tool, tile);
            }

            // clear boulders / meteorites
            // This needs to check if the axe upgrade level is high enough first, to avoid spamming
            // 'need to upgrade your tool' messages. Based on ResourceClump.performToolAction.
            if (this.Config.ClearBouldersAndMeteorites)
            {
                ResourceClump clump = this.GetResourceClumpCoveringTile(location, tile);
                if (clump != null && (!this.ResourceUpgradeLevelsNeeded.TryGetValue(clump.parentSheetIndex.Value, out int requiredUpgradeLevel) || tool.UpgradeLevel >= requiredUpgradeLevel))
                    return this.UseToolOnTile(tool, tile);
            }

            return false;
        }
    }
}
