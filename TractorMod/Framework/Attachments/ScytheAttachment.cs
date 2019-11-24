using System;
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


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="config">The mod configuration.</param>
        /// <param name="reflection">Simplifies access to private code.</param>
        public ScytheAttachment(ScytheConfig config, IReflectionHelper reflection)
            : base(reflection)
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
            if (this.Config.HarvestForage && tileObj?.IsSpawnedObject == true)
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

            // crop or spring onion (if an object like a scarecrow isn't placed on top of it)
            if (this.TryGetHoeDirt(tileFeature, tileObj, out HoeDirt dirt, out bool dirtCoveredByObj))
            {
                if (dirtCoveredByObj || dirt.crop == null)
                    return false;

                if (this.Config.ClearDeadCrops && dirt.crop.dead.Value)
                {
                    this.UseToolOnTile(this.FakePickaxe, tile, player, location); // clear dead crop
                    return true;
                }

                bool shouldHarvest = dirt.crop.programColored.Value // from Utility.findCloseFlower
                    ? this.Config.HarvestFlowers
                    : this.Config.HarvestCrops;
                if (shouldHarvest)
                {
                    return dirt.crop.harvestMethod.Value == Crop.sickleHarvest
                        ? dirt.performToolAction(tool, 0, tile, location)
                        : dirt.performUseAction(tile, location);
                }

                return true;
            }

            // machines
            if (this.Config.HarvestMachines && tileObj != null && tileObj.readyForHarvest.Value && tileObj.heldObject.Value != null)
            {
                tileObj.checkForAction(Game1.player);
                return true;
            }

            // fruit tree
            if (this.Config.HarvestFruitTrees && tileFeature is FruitTree tree && tree.fruitsOnTree.Value > 0)
            {
                tree.performUseAction(tile, location);
                return true;
            }

            // grass
            // (see Grass.performToolAction)
            if (this.Config.HarvestGrass && tileFeature is Grass)
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

            // weeds
            if (this.Config.ClearWeeds && this.IsWeed(tileObj))
            {
                this.UseToolOnTile(tool, tile, player, location); // doesn't do anything to the weed, but sets up for the tool action (e.g. sets last user)
                tileObj.performToolAction(tool, location); // triggers weed drops, but doesn't remove weed
                location.removeObject(tile, false);
                return true;
            }

            // bush
            Rectangle tileArea = this.GetAbsoluteTileArea(tile);
            if (this.Config.HarvestForage)
            {
                Bush bush = tileFeature as Bush ?? location.largeTerrainFeatures.FirstOrDefault(p => p.getBoundingBox(p.tilePosition.Value).Intersects(tileArea)) as Bush;
                if (bush?.tileSheetOffset.Value == 1 && (bush.size.Value == Bush.greenTeaBush || (bush.size.Value == Bush.mediumBush && !bush.townBush.Value)))
                {
                    bush.performUseAction(bush.tilePosition.Value, location);
                    return true;
                }
            }

            return false;
        }
    }
}
