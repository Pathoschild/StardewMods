using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using TractorMod.Framework;
using SFarmer = StardewValley.Farmer;
using SObject = StardewValley.Object;

namespace TractorMod
{
    public class TractorMod : Mod
    {
        /*********
        ** Properties
        *********/
        /****
        ** Constants
        ****/
        /// <summary>The tractor garage's building type.</summary>
        private readonly string GarageBuildingType = "TractorGarage";

        /// <summary>The tractor's NPC name.</summary>
        private readonly string TractorName = "Tractor";

        /// <summary>The unique buff ID for the tractor speed.</summary>
        private readonly int BuffUniqueID = 58012397;

        /// <summary>The number of ticks between each tool use for a given tool.</summary>
        private readonly int TicksBetweenToolUses = 12;

        /****
        ** State
        ****/
        /// <summary>The mod settings.</summary>
        private TractorConfig ModConfig;

        /// <summary>The current player's farm.</summary>
        private Farm Farm;

        /// <summary>The currently spawned tractor.</summary>
        private Tractor Tractor;

        /// <summary>Whether any custom data has been restored for the new day.</summary>
        private bool IsDataRestored;

        /// <summary>Tracks the number of active tool ticks since a tool was last used.</summary>
        private int ActiveToolTicks;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // read config
            this.MigrateLegacySaveData(helper);
            this.ModConfig = helper.ReadConfig<TractorConfig>();

            // spawn tractors & garages
            TimeEvents.AfterDayStarted += this.TimeEvents_AfterDayStarted;
            LocationEvents.CurrentLocationChanged += this.LocationEvents_CurrentLocationChanged;

            // remove tractors & garages before save
            SaveEvents.BeforeSave += this.SaveEvents_BeforeSave;

            // handle player interaction
            ControlEvents.KeyPressed += ControlEvents_KeyPressed;

            // so that weird shit wouldnt happen
            MenuEvents.MenuChanged += this.MenuEvents_MenuChanged;

            // handle player interaction
            GameEvents.UpdateTick += this.GameEvents_UpdateTick;
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Event handlers
        ****/
        /// <summary>The event called when a new day begins.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void TimeEvents_AfterDayStarted(object sender, EventArgs e)
        {
            // set up for new day
            this.IsDataRestored = false;
            this.Tractor = null;
            this.Farm = Game1.getFarm();
        }

        /// <summary>The event called when the player changes location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void LocationEvents_CurrentLocationChanged(object sender, EventArgsCurrentLocationChanged e)
        {
            // spawn tractor house & tractor
            if (!this.IsDataRestored && e.NewLocation == this.Farm)
            {
                this.RestoreCustomData();
                this.IsDataRestored = true;
            }
        }

        /// <summary>The event called before the game starts saving.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void SaveEvents_BeforeSave(object sender, EventArgs e)
        {
            this.StashCustomData();
        }

        /// <summary>The event called after a new menu is opened.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void MenuEvents_MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            // add blueprint to carpenter menu
            if (e.NewMenu is CarpenterMenu menu)
            {
                this.Helper.Reflection
                    .GetPrivateValue<List<BluePrint>>(menu, "blueprints")
                    .Add(this.GetBlueprint());
            }
        }

        /// <summary>The event called when the player presses a keyboard button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ControlEvents_KeyPressed(object sender, EventArgsKeyPressed e)
        {
            // summon tractor
            if (e.KeyPressed == this.ModConfig.TractorKey)
            {
                if (this.Tractor != null)
                    Game1.warpCharacter(this.Tractor, Game1.currentLocation.name, Game1.player.getTileLocation(), false, true);
            }

            // reload config
            else if (e.KeyPressed == this.ModConfig.UpdateConfig)
                this.ModConfig = this.Helper.ReadConfig<TractorConfig>();
        }

        /// <summary>The event called when the game updates (roughly sixty times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void GameEvents_UpdateTick(object sender, EventArgs e)
        {
            if (Game1.currentLocation != null)
                this.Update();
        }

        /****
        ** Save methods
        ****/
        /// <summary>Get the mod-relative path for custom save data.</summary>
        /// <param name="saveID">The save ID.</param>
        private string GetDataPath(string saveID)
        {
            return $"data/{saveID}.json";
        }

        /// <summary>Stash all tractor and garage data to a separate file to avoid breaking the save file.</summary>
        private void StashCustomData()
        {
            // back up garages
            Building[] garages = this.GetGarages(this.Farm).ToArray();
            CustomSaveData saveData = new CustomSaveData(garages);
            this.Helper.WriteJsonFile(this.GetDataPath(Constants.SaveFolderName), saveData);

            // remove tractors + buildings
            foreach (Building garage in garages)
                this.Farm.destroyStructure(garage);
            this.RemoveTractors();
        }

        /// <summary>Restore tractor and garage data removed by <see cref="StashCustomData"/>.</summary>
        private void RestoreCustomData()
        {
            // get save data
            CustomSaveData saveData = this.Helper.ReadJsonFile<CustomSaveData>(this.GetDataPath(Constants.SaveFolderName));
            if (saveData?.Buildings == null)
                return;

            // add tractor + garages
            BluePrint blueprint = this.GetBlueprint();
            foreach (CustomSaveBuilding garage in saveData.Buildings)
            {
                // add garage
                Building newGarage = new Stable(blueprint, garage.Tile) { buildingType = this.GarageBuildingType, daysOfConstructionLeft = 0 }; // rebuild to avoid data issues
                this.Farm.buildStructure(newGarage, garage.Tile, false, Game1.player);

                // add tractor
                if (this.Tractor == null)
                {
                    // spawn tractor
                    foreach (Building building in this.GetGarages(this.Farm))
                    {
                        if (building.daysOfConstructionLeft > 0)
                            continue;
                        this.Tractor = new Tractor(building.tileX + 1, building.tileY + 1, this.Helper.Content) { name = this.TractorName };
                        this.Farm.characters.Add(this.Tractor);
                        Game1.warpCharacter(this.Tractor, "Farm", new Vector2(building.tileX + 1, building.tileY + 1), false, true);
                        break;
                    }
                }
            }
        }

        /// <summary>Migrate the legacy <c>TractorModSave.json</c> file to the new config files.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        private void MigrateLegacySaveData(IModHelper helper)
        {
            // get file
            const string filename = "TractorModSave.json";
            FileInfo file = new FileInfo(Path.Combine(helper.DirectoryPath, filename));
            if (!file.Exists)
                return;

            // read legacy data
            this.Monitor.Log($"Found legacy {filename}, migrating to new save data...");
            IDictionary<string, CustomSaveData> saves = new Dictionary<string, CustomSaveData>();
            {
                LegacySaveData data = helper.ReadJsonFile<LegacySaveData>(filename);
                if (data.Saves != null && data.Saves.Any())
                {
                    foreach (LegacySaveData.LegacySaveEntry saveData in data.Saves)
                    {
                        saves[$"{saveData.FarmerName}_{saveData.SaveSeed}"] = new CustomSaveData(
                            saveData.TractorHouse.Select(p => new CustomSaveBuilding(new Vector2(p.X, p.Y), this.GarageBuildingType))
                        );
                    }
                }
            }

            // write new files
            foreach (var save in saves)
            {
                if (save.Value.Buildings.Any())
                    helper.WriteJsonFile(this.GetDataPath(save.Key), save.Value);
            }

            // delete old file
            file.Delete();
        }

        /// <summary>Remove all tractors from the game.</summary>
        private void RemoveTractors()
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
                location.characters.RemoveAll(p => p is Tractor);
        }

        /****
        ** Action methods
        ****/
        /// <summary>Update tractor effects and actions in the game.</summary>
        private void Update()
        {
            if (Game1.player == null || this.Tractor?.rider != Game1.player || Game1.activeClickableMenu != null)
                return; // tractor isn't enabled

            // apply tractor speed buff
            Buff speedBuff = Game1.buffsDisplay.otherBuffs.FirstOrDefault(p => p.which == this.BuffUniqueID);
            if (speedBuff == null)
            {
                speedBuff = new Buff(0, 0, 0, 0, 0, 0, 0, 0, 0, ModConfig.TractorSpeed, 0, 0, 1, "Tractor Power", "Tractor Power") { which = this.BuffUniqueID };
                Game1.buffsDisplay.addOtherBuff(speedBuff);
            }
            speedBuff.millisecondsDuration = 1000;

            // perform tractor action
            Tool tool = Game1.player.CurrentTool;
            Item item = Game1.player.CurrentItem;
            Vector2[] grid = this.GetTileGrid(Game1.player.getTileLocation(), this.ModConfig.Distance).ToArray();
            if (tool is MeleeWeapon && tool.name.ToLower().Contains("scythe"))
                this.HarvestTiles(grid);
            else if (tool != null)
                this.ApplyTool(tool, grid);
            else if (item != null)
                this.ApplyItem(item, grid);
        }

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
        /// <param name="tiles">The tiles to harvest.</param>
        private void HarvestTiles(Vector2[] tiles)
        {
            if (!this.ModConfig.CanHarvest)
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
                        Farm.tryToAddHay(2);
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
            switch (tool)
            {
                case WateringCan _:
                    if (!this.ModConfig.CanWater)
                        return;
                    break;

                case Hoe _:
                    if (!this.ModConfig.CanHoeDirt)
                        return;
                    break;

                default:
                    if (!this.ModConfig.CustomTools.Contains(tool.name))
                        return;
                    break;
            }

            // apply cooldown
            this.ActiveToolTicks++;
            if (this.ActiveToolTicks % this.TicksBetweenToolUses != 0)
                return;
            this.ActiveToolTicks = 0;

            // track things that shouldn't decrease
            WateringCan wateringCan = tool as WateringCan;
            int waterInCan = wateringCan?.WaterLeft ?? 0;
            float stamina = Game1.player.stamina;
            int toolUpgrade = tool.upgradeLevel;
            Vector2 mountPosition = this.Tractor.position;

            // use tools
            this.Tractor.position = new Vector2(0, 0);
            if (wateringCan != null)
                wateringCan.WaterLeft = wateringCan.waterCanMax;
            tool.upgradeLevel = Tool.iridium;
            Game1.player.toolPower = 0;
            foreach (Vector2 tile in tiles)
                tool.DoFunction(Game1.currentLocation, (int)(tile.X * Game1.tileSize), (int)(tile.Y * Game1.tileSize), 0, Game1.player);

            // reset tools
            this.Tractor.position = mountPosition;
            if (wateringCan != null)
                wateringCan.WaterLeft = waterInCan;
            tool.upgradeLevel = toolUpgrade;
            Game1.player.stamina = stamina;
        }

        /****
        ** Helper methods
        ****/
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

        /// <summary>Get all garages in the given location.</summary>
        /// <param name="location">The location to search.</param>
        private IEnumerable<Building> GetGarages(BuildableGameLocation location)
        {
            return location.buildings.Where(building => building.buildingType == this.GarageBuildingType);
        }

        /// <summary>Get a blueprint to construct the tractor garage.</summary>
        private BluePrint GetBlueprint()
        {
            return new BluePrint(this.GarageBuildingType)
            {
                texture = this.Helper.Content.Load<Texture2D>(@"assets\TractorHouse.png"),
                humanDoor = new Point(-1, -1),
                animalDoor = new Point(-2, -1),
                mapToWarpTo = "null",
                displayName = "Tractor Garage",
                description = "A structure to store PhthaloBlue Corp.'s tractor.\nTractor included!",
                blueprintType = "Buildings",
                nameOfBuildingToUpgrade = "",
                actionBehavior = "null",
                maxOccupants = -1,
                moneyRequired = this.ModConfig.TractorHousePrice,
                tilesWidth = 4,
                tilesHeight = 2,
                sourceRectForMenuView = new Rectangle(0, 0, 64, 96),
                itemsRequired = new Dictionary<int, int>
                {
                    [SObject.ironBar] = 20,
                    [SObject.iridiumBar] = 5,
                    [787] = 5 // battery pack
                },
                magical = true,
                namesOfOkayBuildingLocations = new List<string> { "Farm" }
            };
        }
    }
}
