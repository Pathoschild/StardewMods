using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.TractorMod.Framework.Config;
using StardewModdingAPI;
using StardewValley;
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

        /// <summary>A fake pickaxe to use for clearing dead crops.</summary>
        private readonly Pickaxe FakePickaxe = new Pickaxe();

        /// <summary>A cache of is-flower checks by item ID for <see cref="ShouldHarvest"/>.</summary>
        private readonly IDictionary<int, bool> IsFlowerCache = new Dictionary<int, bool>();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="config">The mod configuration.</param>
        /// <param name="modRegistry">Fetches metadata about loaded mods.</param>
        /// <param name="reflection">Simplifies access to private code.</param>
        public ScytheAttachment(ScytheConfig config, IModRegistry modRegistry, IReflectionHelper reflection)
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
            return tool is MeleeWeapon weapon && weapon.isScythe();
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
            // spawned forage
            if (this.TryHarvestForage(tileObj, location, tile, player))
                return true;

            // crop or indoor pot
            if (this.TryGetHoeDirt(tileFeature, tileObj, out HoeDirt dirt, out bool dirtCoveredByObj, out IndoorPot pot))
            {
                // crop or spring onion (if an object like a scarecrow isn't placed on top of it)
                if (!dirtCoveredByObj && this.TryHarvestCrop(dirt, location, tile, player, tool))
                    return true;

                // indoor pot bush
                if (this.TryHarvestBush(pot?.bush.Value, location))
                    return true;
            }

            // machine
            if (this.TryHarvestMachine(tileObj))
                return true;

            // fruit tree
            if (this.TryHarvestFruitTree(tileFeature as FruitTree, location, tile))
                return true;

            // grass
            if (this.TryHarvestGrass(tileFeature as Grass, location, tile, tool))
                return true;

            // weeds
            if (this.TryHarvestWeeds(tileObj, location, tile, player, tool))
                return true;

            // bush
            Rectangle tileArea = this.GetAbsoluteTileArea(tile);
            if (this.Config.HarvestForage)
            {
                Bush bush = tileFeature as Bush ?? location.largeTerrainFeatures.FirstOrDefault(p => p.getBoundingBox(p.tilePosition.Value).Intersects(tileArea)) as Bush;
                if (this.TryHarvestBush(bush, location))
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
            int cropId = crop.indexOfHarvest.Value;
            if (!this.IsFlowerCache.TryGetValue(cropId, out bool isFlower))
            {
                try
                {
                    isFlower = new SObject(cropId, 1).Category == SObject.flowersCategory;
                }
                catch
                {
                    isFlower = false;
                }
                this.IsFlowerCache[cropId] = isFlower;
            }

            return isFlower
                ? this.Config.HarvestFlowers
                : this.Config.HarvestCrops;
        }

        /// <summary>Harvest a bush if it's ready.</summary>
        /// <param name="bush">The bush to harvest.</param>
        /// <param name="location">The location being harvested.</param>
        /// <returns>Returns whether it was harvested.</returns>
        private bool TryHarvestBush(Bush bush, GameLocation location)
        {
            // harvest if ready
            if (bush?.tileSheetOffset.Value == 1)
            {
                bool isTeaBush = bush.size.Value == Bush.greenTeaBush;
                bool isBerryBush = !isTeaBush && bush.size.Value == Bush.mediumBush && !bush.townBush.Value;
                if ((isTeaBush && this.Config.HarvestCrops) || (isBerryBush && this.Config.HarvestForage))
                {
                    bush.performUseAction(bush.tilePosition.Value, location);
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
        /// <param name="tool">The tool selected by the player (if any).</param>
        /// <returns>Returns whether it was harvested.</returns>
        private bool TryHarvestCrop(HoeDirt dirt, GameLocation location, Vector2 tile, Farmer player, Tool tool)
        {
            if (dirt?.crop == null)
                return false;

            // clear dead crop
            if (this.Config.ClearDeadCrops && dirt.crop.dead.Value)
            {
                this.UseToolOnTile(this.FakePickaxe, tile, player, location); // clear dead crop
                return true;
            }

            // harvest
            if (this.ShouldHarvest(dirt.crop) && dirt.crop.harvest((int)tile.X, (int)tile.Y, dirt))
            {
                bool isScytheCrop = dirt.crop.harvestMethod.Value == Crop.sickleHarvest;
                dirt.destroyCrop(tile, showAnimation: isScytheCrop, location);
                return true;
            }

            return false;
        }

        /// <summary>Try to harvest spawned forage.</summary>
        /// <param name="forage">The forage object.</param>
        /// <param name="location">The location being harvested.</param>
        /// <param name="tile">The tile being harvested.</param>
        /// <param name="player">The current player.</param>
        /// <returns>Returns whether it was harvested.</returns>
        private bool TryHarvestForage(SObject forage, GameLocation location, Vector2 tile, Farmer player)
        {
            if (this.Config.HarvestForage && forage?.IsSpawnedObject == true)
            {
                // pick up forage & cancel animation
                if (this.CheckTileAction(location, tile, player))
                {
                    IReflectedField<int> animationID = this.Reflection.GetField<int>(player.FarmerSprite, "currentSingleAnimation");
                    switch (animationID.GetValue())
                    {
                        case FarmerSprite.harvestItemDown:
                        case FarmerSprite.harvestItemLeft:
                        case FarmerSprite.harvestItemRight:
                        case FarmerSprite.harvestItemUp:
                            player.completelyStopAnimatingOrDoingAction();
                            player.forceCanMove();
                            break;
                    }
                }
                return true;
            }

            return false;
        }

        /// <summary>Try to harvest a fruit tree.</summary>
        /// <param name="tree">The fruit tree to harvest.</param>
        /// <param name="location">The location being harvested.</param>
        /// <param name="tile">The tile being harvested.</param>
        /// <returns>Returns whether it was harvested.</returns>
        private bool TryHarvestFruitTree(FruitTree tree, GameLocation location, Vector2 tile)
        {
            if (this.Config.HarvestFruitTrees && tree?.fruitsOnTree.Value > 0)
            {
                tree.performUseAction(tile, location);
                return true;
            }

            return false;
        }

        /// <summary>Try to harvest tall grass.</summary>
        /// <param name="grass">The grass to harvest.</param>
        /// <param name="location">The location being harvested.</param>
        /// <param name="tile">The tile being harvested.</param>
        /// <param name="tool">The tool selected by the player (if any).</param>
        /// <returns>Returns whether it was harvested.</returns>
        /// <remarks>Derived from <see cref="Grass.performToolAction"/>.</remarks>
        private bool TryHarvestGrass(Grass grass, GameLocation location, Vector2 tile, Tool tool)
        {
            if (this.Config.HarvestGrass && grass != null)
            {
                location.terrainFeatures.Remove(tile);

                Random random = Game1.IsMultiplayer
                    ? Game1.recentMultiplayerRandom
                    : new Random((int)(Game1.uniqueIDForThisGame + tile.X * 1000.0 + tile.Y * 11.0));

                if (random.NextDouble() < (tool.InitialParentTileIndex == MeleeWeapon.goldenScythe ? 0.75 : 0.5))
                {
                    if (Game1.getFarm().tryToAddHay(1) == 0) // returns number left
                        Game1.addHUDMessage(new HUDMessage("Hay", HUDMessage.achievement_type, true, Color.LightGoldenrodYellow, new SObject(178, 1)));
                }

                return true;
            }

            return false;
        }

        /// <summary>Try to harvest the output from a machine.</summary>
        /// <param name="machine">The machine to harvest.</param>
        /// <returns>Returns whether it was harvested.</returns>
        private bool TryHarvestMachine(SObject machine)
        {
            if (this.Config.HarvestMachines && machine != null && machine.readyForHarvest.Value && machine.heldObject.Value != null)
            {
                machine.checkForAction(Game1.player);
                return true;
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
        private bool TryHarvestWeeds(SObject weeds, GameLocation location, Vector2 tile, Farmer player, Tool tool)
        {
            if (this.Config.ClearWeeds && this.IsWeed(weeds))
            {
                this.UseToolOnTile(tool, tile, player, location); // doesn't do anything to the weed, but sets up for the tool action (e.g. sets last user)
                weeds.performToolAction(tool, location); // triggers weed drops, but doesn't remove weed
                location.removeObject(tile, false);
                return true;
            }

            return false;
        }
    }
}
