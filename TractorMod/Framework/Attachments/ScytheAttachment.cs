using Microsoft.Xna.Framework;
using Pathoschild.Stardew.TractorMod.Framework.Config;
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
        /// <summary>The attachment settings.</summary>
        private readonly ScytheConfig Config;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="config">The mod configuration.</param>
        public ScytheAttachment(ScytheConfig config)
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
            return tool is MeleeWeapon && tool.Name.ToLower().Contains("scythe");
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
            if (this.Config.HarvestForage && tileObj?.IsSpawnedObject == true)
            {
                this.CheckTileAction(location, tile, player);
                return true;
            }

            // crop or spring onion
            if (tileFeature is HoeDirt dirt)
            {
                if (dirt.crop == null)
                    return false;

                if (this.Config.ClearDeadCrops && dirt.crop.dead.Value)
                {
                    this.UseToolOnTile(new Pickaxe(), tile); // clear dead crop
                    return true;
                }

                if (this.Config.HarvestCrops)
                {
                    if (dirt.crop.harvestMethod.Value == Crop.sickleHarvest)
                        return dirt.performToolAction(tool, 0, tile, location);
                    else
                        this.CheckTileAction(location, tile, player);
                }

                return true;
            }

            // fruit tree
            if (this.Config.HarvestFruitTrees && tileFeature is FruitTree tree)
            {
                tree.performUseAction(tile, location);
                return true;
            }

            // grass
            if (this.Config.HarvestGrass && tileFeature is Grass)
            {
                location.terrainFeatures.Remove(tile);
                if (Game1.getFarm().tryToAddHay(1) == 0) // returns number left
                    Game1.addHUDMessage(new HUDMessage("Hay", HUDMessage.achievement_type, true, Color.LightGoldenrodYellow, new SObject(178, 1)));
                return true;
            }

            // weeds
            if (this.Config.ClearWeeds && this.IsWeed(tileObj))
            {
                this.UseToolOnTile(tool, tile); // doesn't do anything to the weed, but sets up for the tool action (e.g. sets last user)
                tileObj.performToolAction(tool, location); // triggers weed drops, but doesn't remove weed
                location.removeObject(tile, false);
                return true;
            }

            return false;
        }
    }
}
