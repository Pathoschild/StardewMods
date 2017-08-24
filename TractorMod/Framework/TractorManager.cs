using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using SFarmer = StardewValley.Farmer;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.TractorMod.Framework
{
    /// <summary>Manages a spawned tractor.</summary>
    internal class TractorManager
    {
        /*********
        ** Properties
        *********/
        /// <summary>The unique buff ID for the tractor speed.</summary>
        private readonly int BuffUniqueID = 58012397;

        /// <summary>The number of ticks between each tractor action check.</summary>
        private readonly int TicksPerAction = 12; // roughly five times per second

        /// <summary>Provides translations from the mod's i18n folder.</summary>
        private readonly ITranslationHelper Translation;

        /// <summary>The mod settings.</summary>
        private readonly ModConfig Config;

        /// <summary>The number of ticks since the tractor last checked for an action to perform.</summary>
        private int SkippedActionTicks;


        /*********
        ** Accessors
        *********/
        /// <summary>A static tractor NPC for display in the world.</summary>
        /// <remarks>This is deliberately separate to avoid conflicting with logic for summoning or managing the player horse.</remarks>
        public TractorStatic Static { get; }

        /// <summary>A tractor horse for riding.</summary>
        public TractorMount Mount { get; }

        /// <summary>The currently active tractor instance.</summary>
        public NPC Current => this.IsRiding ? (NPC)this.Mount : this.Static;

        /// <summary>Whether the player is currently riding the tractor.</summary>
        public bool IsRiding => this.Mount?.rider == Game1.player;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="tileX">The initial tile X position.</param>
        /// <param name="tileY">The initial tile Y position.</param>
        /// <param name="config">The mod settings.</param>
        /// <param name="content">The content helper with which to load the tractor sprite.</param>
        /// <param name="translation">Provides translations from the mod's i18n folder.</param>
        public TractorManager(int tileX, int tileY, ModConfig config, IContentHelper content, ITranslationHelper translation)
        {
            AnimatedSprite sprite = new AnimatedSprite(content.Load<Texture2D>(@"assets\tractor.png"), 0, 32, 32)
            {
                textureUsesFlippedRightForLeft = true,
                loop = true
            };

            this.Static = new TractorStatic(typeof(TractorStatic).Name, tileX, tileY, sprite, () => this.SetMounted(true));
            this.Mount = new TractorMount(typeof(TractorMount).Name, tileX, tileY, sprite, () => this.SetMounted(false));
            this.Config = config;
            this.Translation = translation;
        }

        /// <summary>Move the tractor to the given location.</summary>
        /// <param name="location">The game location.</param>
        /// <param name="tile">The tile coordinate in the given location.</param>
        public void SetLocation(GameLocation location, Vector2 tile)
        {
            Game1.warpCharacter(this.Current, location.name, tile, false, true);
        }

        /// <summary>Move the tractor to a specific pixel position within its current location.</summary>
        /// <param name="position">The pixel coordinate in the current location.</param>
        public void SetPixelPosition(Vector2 position)
        {
            this.Current.Position = position;
        }

        /// <summary>Remove all tractors from the game.</summary>
        public void RemoveTractors()
        {
            // find all locations
            IEnumerable<GameLocation> locations = Game1.locations
                .Union(
                    from location in Game1.locations.OfType<BuildableGameLocation>()
                    from building in location.buildings
                    where building.indoors != null
                    select building.indoors
                );

            // remove tractors
            foreach (GameLocation location in locations)
                location.characters.RemoveAll(p => p is TractorStatic || p is TractorMount);
        }

        /// <summary>Update tractor effects and actions in the game.</summary>
        /// <param name="farm">The player's farm instance (not necessarily the current location).</param>
        public void Update(Farm farm)
        {
            if (!this.IsRiding || Game1.activeClickableMenu != null)
                return; // tractor isn't enabled

            // apply tractor speed buff
            Buff speedBuff = Game1.buffsDisplay.otherBuffs.FirstOrDefault(p => p.which == this.BuffUniqueID);
            if (speedBuff == null)
            {
                speedBuff = new Buff(0, 0, 0, 0, 0, 0, 0, 0, 0, this.Config.TractorSpeed, 0, 0, 1, "Tractor Power", this.Translation.Get("buff.name")) { which = this.BuffUniqueID };
                Game1.buffsDisplay.addOtherBuff(speedBuff);
            }
            speedBuff.millisecondsDuration = 100;

            // apply action cooldown
            this.SkippedActionTicks++;
            if (this.SkippedActionTicks % this.TicksPerAction != 0)
                return;
            this.SkippedActionTicks = 0;

            // perform tractor action
            Tool tool = Game1.player.CurrentTool;
            Item item = Game1.player.CurrentItem;
            Vector2[] grid = this.GetTileGrid(Game1.player.getTileLocation(), this.Config.Distance).ToArray();
            if (tool is MeleeWeapon && tool.name.ToLower().Contains("scythe"))
                this.HarvestTiles(farm, grid);
            else if (tool != null)
                this.ApplyTool(tool, grid);
            else if (item != null)
                this.ApplyItem(item, grid);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Apply an item stack to the given tiles.</summary>
        /// <param name="item">The item stack to apply.</param>
        /// <param name="tiles">The tiles to affect.</param>
        private void ApplyItem(Item item, Vector2[] tiles)
        {
            // validate category
            string category = item.getCategoryName().ToLower();
            if (category != "seed" && category != "fertilizer")
                return;

            // act on affected tiles
            foreach (Vector2 tile in tiles)
            {
                // get tilled dirt
                if (!Game1.currentLocation.terrainFeatures.TryGetValue(tile, out TerrainFeature terrainTile) || !(terrainTile is HoeDirt dirt))
                    continue;

                // apply item
                bool applied = false;
                switch (category)
                {
                    case "seed":
                        if (dirt.crop == null && dirt.plant(Game1.player.CurrentItem.parentSheetIndex, (int)tile.X, (int)tile.Y, Game1.player))
                            applied = true;
                        break;

                    case "fertilizer":
                        if (dirt.fertilizer == 0)
                        {
                            dirt.fertilizer = Game1.player.CurrentItem.parentSheetIndex;
                            applied = true;
                        }
                        break;

                    default:
                        throw new NotSupportedException($"Unknown category '{category}'.");
                }

                // deduct from inventory
                if (applied)
                {
                    Game1.player.CurrentItem.Stack -= 1;
                    if (Game1.player.CurrentItem.Stack <= 0)
                    {
                        Game1.player.removeItemFromInventory(Game1.player.CurrentItem);
                        return;
                    }
                }
            }
        }

        /// <summary>Harvest the affected tiles.</summary>
        /// <param name="farm">The player's farm instance (not necessarily the current location).</param>
        /// <param name="tiles">The tiles to harvest.</param>
        private void HarvestTiles(Farm farm, Vector2[] tiles)
        {
            if (!this.Config.ScytheHarvests)
                return;

            foreach (Vector2 tile in tiles)
            {
                // get feature/object on tile
                object target;
                {
                    if (Game1.currentLocation.terrainFeatures.TryGetValue(tile, out TerrainFeature feature))
                        target = feature;
                    else if (Game1.currentLocation.objects.TryGetValue(tile, out SObject obj))
                        target = obj;
                    else
                        continue;
                }

                // harvest target
                switch (target)
                {
                    // crop or spring onion
                    case HoeDirt dirt when dirt.crop != null:
                        {
                            // make item scythe-harvestable
                            int oldHarvestMethod = dirt.crop.harvestMethod;
                            dirt.crop.harvestMethod = Crop.sickleHarvest;

                            // harvest spring onion
                            if (dirt.crop.whichForageCrop == Crop.forageCrop_springOnion)
                            {
                                SObject onion = new SObject(399, 1);
                                bool gatherer = Game1.player.professions.Contains(SFarmer.gatherer);
                                bool botanist = Game1.player.professions.Contains(SFarmer.botanist);
                                if (botanist)
                                    onion.quality = SObject.bestQuality;
                                if (gatherer)
                                {
                                    if (new Random().Next(0, 10) < 2)
                                        onion.stack *= 2;
                                }
                                for (int i = 0; i < onion.stack; i++)
                                    Game1.currentLocation.debris.Add(new Debris(onion, new Vector2(tile.X * Game1.tileSize, tile.Y * Game1.tileSize)));

                                dirt.destroyCrop(tile);
                                continue;
                            }

                            // harvest crop
                            if (dirt.crop.harvest((int)tile.X, (int)tile.Y, dirt))
                            {
                                if (dirt.crop.indexOfHarvest == 421) // sun flower
                                {
                                    int seedDrop = new Random().Next(1, 4);
                                    for (int i = 0; i < seedDrop; i++)
                                        Game1.createObjectDebris(431, (int)tile.X, (int)tile.Y, -1, 0, 1f, Game1.currentLocation); // spawn sunflower seeds
                                }

                                if (dirt.crop.regrowAfterHarvest == -1)
                                    dirt.destroyCrop(tile);
                            }

                            // restore item harvest type
                            if (dirt.crop != null)
                                dirt.crop.harvestMethod = oldHarvestMethod;
                            break;
                        }

                    // fruit tree
                    case FruitTree tree:
                        tree.shake(tile, false);
                        break;

                    // grass
                    case Grass _:
                        Game1.currentLocation.terrainFeatures.Remove(tile);
                        farm.tryToAddHay(2);
                        break;

                    // spawned object
                    case SObject obj when obj.isSpawnedObject:
                        // get output
                        if (obj.isForage(Game1.currentLocation))
                        {
                            bool gatherer = Game1.player.professions.Contains(SFarmer.gatherer);
                            bool botanist = Game1.player.professions.Contains(SFarmer.botanist);
                            if (botanist)
                                obj.quality = SObject.bestQuality;
                            if (gatherer)
                            {
                                int num = new Random().Next(0, 100);
                                if (num < 20)
                                    obj.stack *= 2;
                            }
                        }

                        // spawn output
                        for (int i = 0; i < obj.stack; i++)
                            Game1.currentLocation.debris.Add(new Debris(obj, new Vector2(tile.X * Game1.tileSize, tile.Y * Game1.tileSize)));

                        // remove harvested object
                        Game1.currentLocation.removeObject(tile, false);
                        break;

                    // weed
                    case SObject obj when obj.name.ToLower().Contains("weed"):
                        Game1.createObjectDebris(771, (int)tile.X, (int)tile.Y, -1, 0, 1f, Game1.currentLocation); // fiber
                        if (new Random().Next(0, 10) < 1)
                            Game1.createObjectDebris(770, (int)tile.X, (int)tile.Y, -1, 0, 1f, Game1.currentLocation); // 10% mixed seeds
                        Game1.currentLocation.removeObject(tile, false);
                        break;
                }
            }
        }

        /// <summary>Use a tool on the given tiles.</summary>
        /// <param name="tool">The tool to use.</param>
        /// <param name="tiles">The tiles to affect.</param>
        private void ApplyTool(Tool tool, Vector2[] tiles)
        {
            // check if tool is enabled
            if (!this.Config.CustomTools.Contains(tool.name))
            {
                switch (tool)
                {
                    case WateringCan _:
                        if (!this.Config.WateringCanWaters)
                            return;
                        break;

                    case Hoe _:
                        if (!this.Config.HoeTillsDirt)
                            return;
                        break;

                    case Pickaxe _:
                        if (!this.Config.PickaxeClearsDirt && !this.Config.PickaxeBreaksRocks && !this.Config.PickaxeBreaksFlooring)
                            return; // nothing to do
                        break;

                    default:
                        return;
                }
            }

            // track things that shouldn't decrease
            WateringCan wateringCan = tool as WateringCan;
            int waterInCan = wateringCan?.WaterLeft ?? 0;
            float stamina = Game1.player.stamina;
            int toolUpgrade = tool.upgradeLevel;
            Vector2 mountPosition = this.Current.position;

            // use tools
            this.Current.position = new Vector2(0, 0);
            if (wateringCan != null)
                wateringCan.WaterLeft = wateringCan.waterCanMax;
            tool.upgradeLevel = Tool.iridium;
            Game1.player.toolPower = 0;
            foreach (Vector2 tile in tiles)
            {
                Game1.currentLocation.objects.TryGetValue(tile, out SObject tileObj);
                Game1.currentLocation.terrainFeatures.TryGetValue(tile, out TerrainFeature tileFeature);

                // prevent tools from destroying placed objects
                if (tileObj != null && tileObj.Name != "Stone")
                {
                    if (tool is Hoe || tool is Pickaxe)
                        continue;
                }

                // prevent pickaxe from destroying
                if (tool is Pickaxe)
                {
                    // never destroy live crops
                    if (tileFeature is HoeDirt dirt && dirt.crop != null && !dirt.crop.dead)
                        continue;

                    // don't destroy other things unless configured
                    if (!this.Config.PickaxeBreaksFlooring && tileFeature is Flooring)
                        continue;
                    if (!this.Config.PickaxeClearsDirt && tileFeature is HoeDirt)
                        continue;
                    if (!this.Config.PickaxeBreaksRocks && tileObj?.Name == "Stone")
                        continue;
                }

                // use tool on center of tile
                Vector2 useAt = (tile * Game1.tileSize) + new Vector2(Game1.tileSize / 2f);
                tool.DoFunction(Game1.currentLocation, (int)useAt.X, (int)useAt.Y, 0, Game1.player);
            }

            // reset tools
            this.Current.position = mountPosition;
            if (wateringCan != null)
                wateringCan.WaterLeft = waterInCan;
            tool.upgradeLevel = toolUpgrade;
            Game1.player.stamina = stamina;
        }

        /// <summary>Get a grid of tiles.</summary>
        /// <param name="origin">The center of the grid.</param>
        /// <param name="distance">The number of tiles in each direction to include.</param>
        private IEnumerable<Vector2> GetTileGrid(Vector2 origin, int distance)
        {
            for (int x = -distance; x <= distance; x++)
            {
                for (int y = -distance; y <= distance; y++)
                    yield return new Vector2(origin.X + x, origin.Y + y);
            }
        }

        /// <summary>Set whether the player should be riding the tractor.</summary>
        /// <param name="mount">Whether the player should be riding the tractor.</param>
        private void SetMounted(bool mount)
        {
            // swap tractors
            NPC newTractor = mount ? (NPC)this.Mount : this.Static;
            NPC oldTractor = mount ? (NPC)this.Static : this.Mount;

            Game1.removeCharacterFromItsLocation(oldTractor.name);
            Game1.removeCharacterFromItsLocation(newTractor.name);
            Game1.currentLocation.addCharacter(newTractor);

            newTractor.position = oldTractor.position;
            newTractor.facingDirection = oldTractor.facingDirection;

            // mount
            if (mount)
                this.Mount.checkAction(Game1.player, Game1.currentLocation);
        }
    }
}
