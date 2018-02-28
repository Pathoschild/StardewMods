using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using SFarmer = StardewValley.Farmer;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.TractorMod.Framework.Attachments
{
    /// <summary>An attachment for the scythe.</summary>
    internal class ScytheAttachment : BaseAttachment
    {
        /*********
        ** Properties
        *********/
        /// <summary>Whether to harvest forage.</summary>
        private readonly bool HarvestForage;

        /// <summary>Whether to harvest crops.</summary>
        private readonly bool HarvestCrops;

        /// <summary>Whether to harvest flowers.</summary>
        private readonly bool HarvestFlowers;

        /// <summary>Whether to harvest fruit trees.</summary>
        private readonly bool HarvestFruitTrees;

        /// <summary>Whether to harvest grass.</summary>
        private readonly bool HarvestGrass;

        /// <summary>Whether to harvest dead crops.</summary>
        private readonly bool ClearDeadCrops;

        /// <summary>Whether to harvest debris.</summary>
        private readonly bool ClearDebris;

        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="config">The mod configuration.</param>
        public ScytheAttachment(ModConfig config)
        {
            this.HarvestForage = config.StandardAttachments.Scythe.HarvestForage;
            this.HarvestCrops = config.StandardAttachments.Scythe.HarvestCrops;
            this.HarvestFlowers = config.StandardAttachments.Scythe.HarvestFlowers;
            this.HarvestFruitTrees = config.StandardAttachments.Scythe.HarvestFruitTrees;
            this.HarvestGrass = config.StandardAttachments.Scythe.HarvestGrass;
            this.ClearDeadCrops = config.StandardAttachments.Scythe.ClearDeadCrops;
            this.ClearDebris = config.StandardAttachments.Scythe.ClearWeeds;
        }

        /// <summary>Get whether the tool is currently enabled.</summary>
        /// <param name="player">The current player.</param>
        /// <param name="tool">The tool selected by the player (if any).</param>
        /// <param name="item">The item selected by the player (if any).</param>
        /// <param name="location">The current location.</param>
        public override bool IsEnabled(SFarmer player, Tool tool, Item item, GameLocation location)
        {
            return tool is MeleeWeapon && tool.name.ToLower().Contains("scythe");
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
            // spawned forage
            if (tileObj?.isSpawnedObject == true && this.HarvestForage)
            {
                this.CheckTileAction(location, tile, player);
                return true;
            }

            // crop or spring onion
            if (tileFeature is HoeDirt dirt)
            {
                if (dirt.crop == null)
                    return false;

                if (dirt.crop.dead && this.ClearDeadCrops)
                {
                    this.UseToolOnTile(new Pickaxe(), tile); // clear dead crop
                    return true;
                }

                List<int> flowers = new List<int> { 376, 591, 593, 595, 597, 421 };
                if (dirt.crop.harvestMethod == Crop.sickleHarvest)
                {
                    if (flowers.Contains(dirt.crop.indexOfHarvest))
                        return this.HarvestFlowers && dirt.performToolAction(tool, 0, tile, location);
                    else
                        return this.HarvestCrops && dirt.performToolAction(tool, 0, tile, location);
                }
                else
                    this.CheckTileAction(location, tile, player);

                /*
                if (dirt.crop.harvestMethod == Crop.sickleHarvest && flowers.Contains(dirt.crop.indexOfHarvest) && this.HarvestFlowers)
                    dirt.performToolAction(tool, 0, tile, location);

                else if (dirt.crop.harvestMethod == Crop.sickleHarvest && !flowers.Contains(dirt.crop.indexOfHarvest) && this.HarvestCrops)
                    dirt.performToolAction(tool, 0, tile, location);

                else
                    this.CheckTileAction(location, tile, player);
                */

                return true;
            }

            // fruit tree
            if (tileFeature is FruitTree tree && this.HarvestFruitTrees)
            {
                tree.performUseAction(tile);
                return true;
            }

            // grass
            if (tileFeature is Grass _ && this.HarvestGrass)
            {
                location.terrainFeatures.Remove(tile);
                if (Game1.getFarm().tryToAddHay(1) == 0) // returns number left
                    Game1.addHUDMessage(new HUDMessage("Hay", HUDMessage.achievement_type, true, Color.LightGoldenrodYellow, new SObject(178, 1)));
                return true;
            }

            // weeds
            if (tileObj?.Name.ToLower().Contains("weed") == true && this.ClearDebris)
            {
                this.UseToolOnTile(tool, tile); // doesn't do anything to the weed, but sets up for the tool action (e.g. sets last user)
                tileObj.performToolAction(tool);    // triggers weed drops, but doesn't remove weed
                location.removeObject(tile, false);
                return true;
            }

            return false;
        }
    }
}
