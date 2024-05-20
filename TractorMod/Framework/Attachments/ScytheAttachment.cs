using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.TractorMod.Framework.Config;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Crops;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.TractorMod.Framework.Attachments
{
    /// <summary>An attachment for the scythe.</summary>
    internal class ScytheAttachment : BaseAttachment
    {
        /*********
        ** Fields
        *********/
        /// <summary>The attachment settings.</summary>
        private readonly ScytheConfig Config;

        /// <summary>A cache of is-flower checks by item ID for <see cref="ShouldHarvest"/>.</summary>
        private readonly Dictionary<string, bool> IsFlowerCache = new();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="config">The mod configuration.</param>
        /// <param name="modRegistry">Fetches metadata about loaded mods.</param>
        public ScytheAttachment(ScytheConfig config, IModRegistry modRegistry)
            : base(modRegistry)
        {
            this.Config = config;
        }

        /// <summary>Get whether the tool is currently enabled.</summary>
        /// <param name="player">The current player.</param>
        /// <param name="tool">The tool selected by the player (if any).</param>
        /// <param name="item">The item selected by the player (if any).</param>
        /// <param name="location">The current location.</param>
        public override bool IsEnabled(Farmer player, Tool? tool, Item? item, GameLocation location)
        {
            return
                tool is MeleeWeapon weapon
                && weapon.isScythe();
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

            // spawned forage
            if (this.Config.HarvestForage && tileObj?.IsSpawnedObject == true && this.CheckTileAction(location, tile, player))
            {
                this.CancelAnimation(player, FarmerSprite.harvestItemDown, FarmerSprite.harvestItemLeft, FarmerSprite.harvestItemRight, FarmerSprite.harvestItemUp);
                return true;
            }

            // crop or indoor pot
            if (this.TryGetHoeDirt(tileFeature, tileObj, out HoeDirt? dirt, out bool dirtCoveredByObj, out IndoorPot? pot))
            {
                // crop or spring onion (if an object like a scarecrow isn't placed on top of it)
                if (!dirtCoveredByObj && this.TryHarvestCrop(dirt, location, tile, player))
                    return true;

                // indoor pot bush
                if (this.TryHarvestBush(pot?.bush.Value))
                    return true;
            }

            // machine
            if (this.TryHarvestMachine(tileObj))
                return true;

            // grass
            if (tileFeature is Grass grass && this.ShouldHarvest(grass) && this.TryHarvestGrass(grass, location, tile, player, tool))
                return true;

            // tree
            if (this.TryHarvestTree(tileFeature, tile, tool))
                return true;

            // weeds
            if (this.TryHarvestWeeds(tileObj, location, tile, player, tool))
                return true;

            // bush
            Rectangle tileArea = this.GetAbsoluteTileArea(tile);
            if (this.Config.HarvestForage)
            {
                Bush? bush = tileFeature as Bush ?? location.largeTerrainFeatures.FirstOrDefault(p => p.getBoundingBox().Intersects(tileArea)) as Bush;
                if (this.TryHarvestBush(bush))
                    return true;
            }

            return false;
        }

        /// <summary>Method called when the tractor attachments have been activated for a location.</summary>
        /// <param name="location">The current tractor location.</param>
        public override void OnActivated(GameLocation location)
        {
            base.OnActivated(location);
            this.IsFlowerCache.Clear();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get whether a crop should be harvested.</summary>
        /// <param name="crop">The crop to check.</param>
        private bool ShouldHarvest(Crop crop)
        {
            // flower
            if (this.IsFlower(crop))
                return this.Config.HarvestFlowers;

            // forage
            if (CommonHelper.IsItemId(crop.whichForageCrop.Value, allowZero: false))
                return this.Config.HarvestForage;

            // crop
            return this.Config.HarvestCrops;
        }

        /// <summary>Get whether a grass should be harvested.</summary>
        /// <param name="grass">The grass to check.</param>
        private bool ShouldHarvest(Grass grass)
        {
            return grass.grassType.Value switch
            {
                Grass.blueGrass => this.Config.HarvestBlueGrass,
                _ => this.Config.HarvestNonBlueGrass
            };
        }

        /// <summary>Get whether a crop counts as a flower.</summary>
        /// <param name="crop">The crop to check.</param>
        private bool IsFlower([NotNullWhen(true)] Crop? crop)
        {
            if (crop == null)
                return false;

            string cropId = crop.indexOfHarvest.Value;
            if (string.IsNullOrWhiteSpace(cropId))
                return false;

            if (!this.IsFlowerCache.TryGetValue(cropId, out bool isFlower))
            {
                try
                {
                    isFlower = ItemRegistry.GetData(cropId)?.Category == SObject.flowersCategory;
                }
                catch
                {
                    isFlower = false;
                }
                this.IsFlowerCache[cropId] = isFlower;
            }

            return isFlower;
        }

        /// <summary>Harvest a bush if it's ready.</summary>
        /// <param name="bush">The bush to harvest.</param>
        /// <returns>Returns whether it was harvested.</returns>
        private bool TryHarvestBush([NotNullWhen(true)] Bush? bush)
        {
            // harvest if ready
            if (bush?.tileSheetOffset.Value == 1)
            {
                bool isTeaBush = bush.size.Value == Bush.greenTeaBush;
                bool isBerryBush = !isTeaBush && bush.size.Value == Bush.mediumBush && !bush.townBush.Value;
                if ((isTeaBush && this.Config.HarvestCrops) || (isBerryBush && this.Config.HarvestForage))
                {
                    bush.performUseAction(bush.Tile);
                    return true;
                }
            }

            return false;
        }

        /// <summary>Try to harvest the crop on a hoed dirt tile.</summary>
        /// <param name="dirt">The hoed dirt tile.</param>
        /// <param name="location">The location being harvested.</param>
        /// <param name="tile">The tile being harvested.</param>
        /// <param name="player">The current player.</param>
        /// <returns>Returns whether it was harvested.</returns>
        /// <remarks>Derived from <see cref="HoeDirt.performUseAction"/> and <see cref="HoeDirt.performToolAction"/>.</remarks>
        private bool TryHarvestCrop([NotNullWhen(true)] HoeDirt? dirt, GameLocation location, Vector2 tile, Farmer player)
        {
            if (dirt?.crop == null)
                return false;

            // clear dead crop
            if (this.Config.ClearDeadCrops && this.TryClearDeadCrop(location, tile, dirt, player))
                return true;

            // harvest
            if (this.ShouldHarvest(dirt.crop))
            {
                CropData? data = dirt.crop.GetData();
                HarvestMethod? wasHarvestMethod = data?.HarvestMethod;

                try
                {
                    if (data != null)
                        data.HarvestMethod = HarvestMethod.Scythe; // prevent player from visually stooping off of tractor to grab crop

                    // scythe or pick crops
                    if (dirt.crop.harvest((int)tile.X, (int)tile.Y, dirt))
                    {
                        bool isScytheCrop = dirt.crop.GetHarvestMethod() == HarvestMethod.Scythe;

                        dirt.destroyCrop(showAnimation: isScytheCrop);
                        if (!isScytheCrop && location is IslandLocation && Game1.random.NextDouble() < 0.05)
                            Game1.player.team.RequestLimitedNutDrops("IslandFarming", location, (int)tile.X * 64, (int)tile.Y * 64, 5);

                        return true;
                    }

                    // hoe crops (e.g. ginger)
                    if (dirt.crop.hitWithHoe((int)tile.X, (int)tile.Y, location, dirt))
                    {
                        dirt.destroyCrop(showAnimation: false);
                        return true;
                    }
                }
                finally
                {
                    if (data != null)
                        data.HarvestMethod = wasHarvestMethod!.Value;
                }
            }

            return false;
        }

        /// <summary>Try to harvest the output from a machine.</summary>
        /// <param name="machine">The machine to harvest.</param>
        /// <returns>Returns whether it was harvested.</returns>
        private bool TryHarvestMachine([NotNullWhen(true)] SObject? machine)
        {
            if (this.Config.HarvestMachines && machine != null && machine.readyForHarvest.Value && machine.heldObject.Value != null)
            {
                machine.checkForAction(Game1.player);
                return true;
            }

            return false;
        }

        /// <summary>Try to harvest a tree.</summary>
        /// <param name="terrainFeature">The tree to harvest.</param>
        /// <param name="tile">The tile being harvested.</param>
        /// <param name="scythe">The scythe being used.</param>
        /// <returns>Returns whether it was harvested.</returns>
        private bool TryHarvestTree([NotNullWhen(true)] TerrainFeature? terrainFeature, Vector2 tile, Tool scythe)
        {
            switch (terrainFeature)
            {
                case FruitTree tree:
                    if (this.Config.HarvestFruitTrees && tree.fruit.Count > 0)
                    {
                        tree.performUseAction(tile);
                        return true;
                    }
                    break;

                case Tree tree:
                    if (tree.hasSeed.Value && !tree.tapped.Value)
                    {
                        bool shouldHarvest = tree.treeType.Value is (Tree.palmTree or Tree.palmTree2)
                            ? this.Config.HarvestFruitTrees
                            : this.Config.HarvestTreeSeeds;

                        if (shouldHarvest && tree.performUseAction(tile))
                            return true;
                    }

                    if (tree.hasMoss.Value && this.Config.HarvestTreeMoss)
                    {
                        if (tree.performToolAction(scythe, 0, tile))
                            return true;
                    }
                    break;
            }

            return false;
        }

        /// <summary>Try to harvest weeds.</summary>
        /// <param name="weeds">The weeds to harvest.</param>
        /// <param name="location">The location being harvested.</param>
        /// <param name="tile">The tile being harvested.</param>
        /// <param name="player">The current player.</param>
        /// <param name="tool">The tool selected by the player (if any).</param>
        /// <returns>Returns whether it was harvested.</returns>
        private bool TryHarvestWeeds([NotNullWhen(true)] SObject? weeds, GameLocation location, Vector2 tile, Farmer player, Tool tool)
        {
            if (this.Config.ClearWeeds && weeds?.IsWeeds() == true)
            {
                this.UseToolOnTile(tool, tile, player, location); // doesn't do anything to the weed, but sets up for the tool action (e.g. sets last user)
                weeds.performToolAction(tool); // triggers weed drops, but doesn't remove weed
                location.removeObject(tile, false);
                return true;
            }

            return false;
        }
    }
}
