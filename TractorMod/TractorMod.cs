using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
        private Vector2 tractorSpawnLocation = new Vector2(70, 13);
        private TractorConfig ModConfig { get; set; }
        private Tractor Tractor;
        private SaveCollection AllSaves;
        private bool IsNewDay;
        private bool IsNewTractor;
        private const int buffUniqueID = 58012397;
        private bool TractorOn;
        private Farm Farm;

        /// <summary>The tractor garage's building type.</summary>
        private readonly string GarageBuildingType = "TractorGarage";

        /// <summary>The tractor's NPC name.</summary>
        private readonly string TractorName = "Tractor";


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.ModConfig = helper.ReadConfig<TractorConfig>();

            // spawn tractor & remove it before save
            TimeEvents.AfterDayStarted += this.TimeEvents_AfterDayStarted;
            LocationEvents.CurrentLocationChanged += this.LocationEvents_CurrentLocationChanged;
            SaveEvents.BeforeSave += this.SaveEvents_BeforeSave;

            // so that weird shit wouldnt happen
            MenuEvents.MenuChanged += this.MenuEvents_MenuChanged;

            // handle player interaction
            GameEvents.UpdateTick += this.UpdateTickEvent;
        }


        /*********
        ** Private methods
        *********/
        private void TimeEvents_AfterDayStarted(object sender, EventArgs eventArgs)
        {
            // set up for new day
            this.IsNewDay = true;
            this.IsNewTractor = true;
            this.Farm = Game1.getFarm();
        }

        private void LocationEvents_CurrentLocationChanged(object sender, EventArgsCurrentLocationChanged e)
        {
            // spawn tractor house & tractor
            if (this.IsNewDay && e.NewLocation == this.Farm)
            {
                this.LoadModInfo();
                this.IsNewDay = false;
            }
        }

        private void SaveEvents_BeforeSave(object sender, EventArgs eventArgs)
        {
            // save mod data
            this.SaveModInfo();

            // remove tractor from save
            foreach (Building tractorHouse in this.GetGarages(this.Farm).ToArray())
                this.Farm.destroyStructure(tractorHouse);
            foreach (GameLocation location in Game1.locations)
                this.RemoveEveryCharactersOfType<Tractor>(location);
        }

        private void MenuEvents_MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            // add blueprint carpenter menu
            if (e.NewMenu is CarpenterMenu menu)
            {
                this.Helper.Reflection
                    .GetPrivateValue<List<BluePrint>>(menu, "blueprints")
                    .Add(this.GetBlueprint());
            }
        }

        private void UpdateTickEvent(object sender, EventArgs e)
        {
            if (this.ModConfig == null || Game1.currentLocation == null)
                return;

            KeyboardState keyboard = Keyboard.GetState();
            if (keyboard.IsKeyDown(ModConfig.UpdateConfig))
                ModConfig = this.Helper.ReadConfig<TractorConfig>();
            DoAction(keyboard);
        }

        private void SpawnTractor(bool spawnAtFirstTractorHouse = true)
        {
            if (!this.IsNewTractor)
                return;

            foreach (GameLocation GL in Game1.locations)
                RemoveEveryCharactersOfType<Tractor>(GL);

            if (!spawnAtFirstTractorHouse)
            {
                this.Tractor = new Tractor((int)tractorSpawnLocation.X, (int)tractorSpawnLocation.Y, this.Helper.Content) { name = this.TractorName };
                this.Farm.characters.Add(this.Tractor);
                Game1.warpCharacter(this.Tractor, "Farm", tractorSpawnLocation, false, true);
                IsNewTractor = false;
                return;
            }

            // spawn tractor
            foreach (Building building in this.GetGarages(this.Farm))
            {
                if (building.daysOfConstructionLeft > 0)
                    continue;
                this.Tractor = new Tractor(building.tileX + 1, building.tileY + 1, this.Helper.Content) { name = this.TractorName };
                this.Farm.characters.Add(this.Tractor);
                Game1.warpCharacter(this.Tractor, "Farm", new Vector2(building.tileX + 1, building.tileY + 1), false, true);
                this.IsNewTractor = false;
                break;
            }
        }

        //use to write AllSaves info to some .json file to store save
        private void SaveModInfo()
        {
            if (AllSaves == null)
                AllSaves = new SaveCollection().Add(new CustomSaveData(Game1.player.name, Game1.uniqueIDForThisGame));

            CustomSaveData currentSave = AllSaves.FindSave(Game1.player.name, Game1.uniqueIDForThisGame);

            if (currentSave.SaveSeed != ulong.MaxValue)
            {
                currentSave.TractorHouse.Clear();
                foreach (Building building in this.GetGarages(this.Farm))
                    currentSave.AddGarage(building.tileX, building.tileY);
            }
            else
            {
                AllSaves.saves.Add(new CustomSaveData(Game1.player.name, Game1.uniqueIDForThisGame));
                SaveModInfo();
                return;
            }
            this.Helper.WriteJsonFile("TractorModSave.json", AllSaves);
        }

        //use to load save info from some .json file to AllSaves
        private void LoadModInfo()
        {
            this.AllSaves = this.Helper.ReadJsonFile<SaveCollection>("TractorModSave.json") ?? new SaveCollection();
            CustomSaveData saveInfo = this.AllSaves.FindSave(Game1.player.name, Game1.uniqueIDForThisGame);
            if (saveInfo != null && saveInfo.SaveSeed != ulong.MaxValue)
            {
                foreach (Vector2 tile in saveInfo.GetGarages())
                {
                    BluePrint blueprint = this.GetBlueprint();
                    Building building = new Stable(blueprint, tile) { daysOfConstructionLeft = 0 };
                    this.Farm.buildStructure(building, tile, false, Game1.player);
                    if (this.IsNewTractor)
                        this.SpawnTractor();
                }
            }
        }

        private void RemoveEveryCharactersOfType<T>(GameLocation GL)
        {
            bool found = false;
            foreach (NPC character in GL.characters)
            {
                if (character is T)
                {
                    found = true;
                    GL.characters.Remove(character);
                    break;
                }
            }
            if (found)
                RemoveEveryCharactersOfType<T>(GL);
        }

        //execute most of the mod thinking here
        private void DoAction(KeyboardState currentKeyboardState)
        {
            if (Game1.currentLocation == null)
                return;

            // summon tractor
            if (currentKeyboardState.IsKeyDown(ModConfig.TractorKey))
            {
                if (this.Tractor != null)
                    Game1.warpCharacter(this.Tractor, Game1.currentLocation.name, Game1.player.getTileLocation(), false, true);
            }

            //staring tractorMod
            this.TractorOn = false;
            if (this.Tractor != null)
            {
                if (this.Tractor.rider == Game1.player)
                    this.TractorOn = true;
            }
            else //this should be unreachable code
                this.SpawnTractor();

            bool BuffAlready = false;
            if (!TractorOn)
                return;

            //find if tractor buff already applied
            foreach (Buff buff in Game1.buffsDisplay.otherBuffs)
            {
                if (buff.which == buffUniqueID)
                {
                    if (buff.millisecondsDuration <= 35)
                    {
                        buff.millisecondsDuration = 1000;
                    }
                    BuffAlready = true;
                    break;
                }
            }

            //create new buff if its not already applied
            if (!BuffAlready)
            {
                Buff tractorBuff = new Buff(0, 0, 0, 0, 0, 0, 0, 0, 0, ModConfig.TractorSpeed, 0, 0, 1, "Tractor Power", "Tractor Power");
                tractorBuff.which = buffUniqueID;
                tractorBuff.millisecondsDuration = 1000;
                Game1.buffsDisplay.addOtherBuff(tractorBuff);
                BuffAlready = true;
            }


            //if Tractor Mode (buff) is ON
            if (Game1.player.CurrentTool == null)
                ItemAction();
            else
                RunToolAction();
        }

        private void RunToolAction()
        {
            if (Game1.player.CurrentTool is MeleeWeapon && Game1.player.CurrentTool.name.ToLower().Contains("scythe"))
                HarvestAction();
            else
                ToolAction();
        }

        private void ItemAction()
        {
            if (Game1.player.CurrentItem == null)
                return;

            if (Game1.player.CurrentItem.getCategoryName().ToLower() == "seed" || Game1.player.CurrentItem.getCategoryName().ToLower() == "fertilizer")
            {
                List<Vector2> affectedTileGrid = MakeVector2TileGrid(Game1.player.getTileLocation(), ModConfig.ItemRadius);

                foreach (Vector2 tile in affectedTileGrid)
                {
                    TerrainFeature terrainTile;
                    if (Game1.currentLocation.terrainFeatures.TryGetValue(tile, out terrainTile))
                    {
                        if (Game1.currentLocation.terrainFeatures[tile] is HoeDirt)
                        {
                            HoeDirt hoedirtTile = (HoeDirt)Game1.currentLocation.terrainFeatures[tile];

                            if (Game1.player.CurrentItem.getCategoryName().ToLower() == "seed")
                            {
                                if (hoedirtTile.crop != null)
                                    continue;
                                if (hoedirtTile.plant(Game1.player.CurrentItem.parentSheetIndex, (int)tile.X, (int)tile.Y, Game1.player))
                                {
                                    Game1.player.CurrentItem.Stack -= 1;
                                    if (Game1.player.CurrentItem.Stack <= 0)
                                    {
                                        Game1.player.removeItemFromInventory(Game1.player.CurrentItem);
                                        return;
                                    }
                                }
                            }
                            if (Game1.player.CurrentItem.getCategoryName().ToLower() == "fertilizer")
                            {
                                if (hoedirtTile.fertilizer != 0)
                                    continue;
                                hoedirtTile.fertilizer = Game1.player.CurrentItem.parentSheetIndex;
                                Game1.player.CurrentItem.Stack -= 1;
                                if (Game1.player.CurrentItem.Stack <= 0)
                                {
                                    Game1.player.removeItemFromInventory(Game1.player.CurrentItem);
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void HarvestAction()
        {
            //check if tool is enable from config
            ToolConfig ConfigForCurrentTool = new ToolConfig("");
            foreach (ToolConfig TC in ModConfig.Tool)
            {
                if (Game1.player.CurrentTool.name.Contains("Scythe"))
                {
                    ConfigForCurrentTool = TC;
                    break;
                }
            }
            if (ConfigForCurrentTool.Name == "")
                return;
            int effectRadius = ConfigForCurrentTool.EffectRadius;
            List<Vector2> affectedTileGrid = MakeVector2TileGrid(Game1.player.getTileLocation(), effectRadius);

            //harvesting objects
            foreach (Vector2 tile in affectedTileGrid)
            {
                StardewValley.Object anObject;
                if (Game1.currentLocation.objects.TryGetValue(tile, out anObject))
                {
                    if (anObject.isSpawnedObject)
                    {
                        if (anObject.isForage(Game1.currentLocation))
                        {
                            bool gatherer = CheckFarmerProfession(Game1.player, SFarmer.gatherer);
                            bool botanist = CheckFarmerProfession(Game1.player, SFarmer.botanist);
                            if (botanist)
                                anObject.quality = 4;
                            if (gatherer)
                            {
                                int num = new Random().Next(0, 100);
                                if (num < 20)
                                {
                                    anObject.stack *= 2;
                                }
                            }
                        }

                        for (int i = 0; i < anObject.stack; i++)
                            Game1.currentLocation.debris.Add(new Debris(anObject, new Vector2(tile.X * Game1.tileSize, tile.Y * Game1.tileSize)));
                        Game1.currentLocation.removeObject(tile, false);
                        continue;
                    }

                    if (anObject.name.ToLower().Contains("weed"))
                    {
                        Game1.createObjectDebris(771, (int)tile.X, (int)tile.Y, -1, 0, 1f, Game1.currentLocation); //fiber
                        if (new Random().Next(0, 10) < 1) //10% mixed seeds
                            Game1.createObjectDebris(770, (int)tile.X, (int)tile.Y, -1, 0, 1f, Game1.currentLocation); //fiber
                        Game1.currentLocation.removeObject(tile, false);
                    }
                }
            }

            //harvesting plants
            foreach (Vector2 tile in affectedTileGrid)
            {
                TerrainFeature terrainTile;
                if (Game1.currentLocation.terrainFeatures.TryGetValue(tile, out terrainTile))
                {
                    if (Game1.currentLocation.terrainFeatures[tile] is HoeDirt)
                    {
                        HoeDirt hoedirtTile = (HoeDirt)Game1.currentLocation.terrainFeatures[tile];
                        if (hoedirtTile.crop == null)
                            continue;

                        int harvestMethod = hoedirtTile.crop.harvestMethod;
                        hoedirtTile.crop.harvestMethod = Crop.sickleHarvest;

                        if (hoedirtTile.crop.whichForageCrop == 1) //spring onion
                        {
                            StardewValley.Object anObject = new StardewValley.Object(399, 1);
                            bool gatherer = CheckFarmerProfession(Game1.player, SFarmer.gatherer);
                            bool botanist = CheckFarmerProfession(Game1.player, SFarmer.botanist);
                            if (botanist)
                            {
                                anObject.quality = 4;
                            }
                            if (gatherer)
                            {
                                int num = new Random().Next(0, 100);
                                if (num < 20)
                                {
                                    anObject.stack *= 2;
                                }
                            }
                            for (int i = 0; i < anObject.stack; i++)
                                Game1.currentLocation.debris.Add(new Debris(anObject, new Vector2(tile.X * Game1.tileSize, tile.Y * Game1.tileSize)));

                            hoedirtTile.destroyCrop(tile);
                            continue;
                        }

                        if (hoedirtTile.crop.harvest((int)tile.X, (int)tile.Y, hoedirtTile))
                        {
                            if (hoedirtTile.crop.indexOfHarvest == 421) //sun flower
                            {
                                int seedDrop = new Random().Next(1, 4);
                                for (int i = 0; i < seedDrop; i++)
                                    Game1.createObjectDebris(431, (int)tile.X, (int)tile.Y, -1, 0, 1f, Game1.currentLocation); //spawn sunflower seeds
                            }

                            if (hoedirtTile.crop.regrowAfterHarvest == -1)
                            {
                                hoedirtTile.destroyCrop(tile);
                            }
                        }

                        if (hoedirtTile.crop != null)
                            hoedirtTile.crop.harvestMethod = harvestMethod;
                        continue;
                    }

                    if (Game1.currentLocation.terrainFeatures[tile] is FruitTree)
                    {
                        FruitTree tree = (FruitTree)Game1.currentLocation.terrainFeatures[tile];
                        tree.shake(tile, false);
                        continue;
                    }

                    if (Game1.currentLocation.terrainFeatures[tile] is Grass)
                    {
                        Game1.currentLocation.terrainFeatures.Remove(tile);
                        Farm.tryToAddHay(2);
                        continue;
                    }
                }
            }
        }

        private void ToolAction()
        {
            Tool currentTool = Game1.player.CurrentTool;

            //check if tool is enable from config
            ToolConfig configForCurrentTool = null;
            foreach (ToolConfig toolConfig in ModConfig.Tool)
            {
                if (currentTool.name.Contains(toolConfig.Name))
                {
                    configForCurrentTool = toolConfig;
                    break;
                }
            }

            if (configForCurrentTool == null || currentTool.upgradeLevel < configForCurrentTool.MinLevel)
                return;

            if (configForCurrentTool.ActiveEveryTickAmount > 1)
            {
                configForCurrentTool.IncrementTicks();
                if (!configForCurrentTool.IsReady())
                    return;
            }

            int effectRadius = configForCurrentTool.EffectRadius;
            int currentWater = 0;
            if (currentTool is WateringCan)
            {
                WateringCan currentWaterCan = (WateringCan)currentTool;
                currentWater = currentWaterCan.WaterLeft;
            }

            float currentStamina = Game1.player.stamina;
            List<Vector2> affectedTileGrid = MakeVector2TileGrid(Game1.player.getTileLocation(), effectRadius);

            //if player on horse
            Vector2 currentMountPosition = new Vector2();
            if (Game1.player.isRidingHorse())
            {
                currentMountPosition = Game1.player.getMount().position;
                Game1.player.getMount().position = new Vector2(0, 0);
            }

            //Tool 

            //before tool use
            int toolUpgrade = currentTool.upgradeLevel;
            currentTool.upgradeLevel = 4;
            Game1.player.toolPower = 0;

            //tool use
            foreach (Vector2 tile in affectedTileGrid)
            {
                currentTool.DoFunction(Game1.currentLocation, (int)(tile.X * Game1.tileSize), (int)(tile.Y * Game1.tileSize), 0, Game1.player);
            }

            //after tool use
            if (Game1.player.isRidingHorse())
                Game1.player.getMount().position = currentMountPosition;
            currentTool.upgradeLevel = toolUpgrade;
            Game1.player.stamina = currentStamina;

            if (currentTool is WateringCan)
            {
                WateringCan currentWaterCan = (WateringCan)currentTool;
                currentWaterCan.WaterLeft = currentWater;
            }
        }

        private List<Vector2> MakeVector2TileGrid(Vector2 origin, int size)
        {
            List<Vector2> grid = new List<Vector2>();
            for (int i = 0; i < 2 * size + 1; i++)
            {
                for (int j = 0; j < 2 * size + 1; j++)
                {
                    Vector2 newVec = new Vector2(origin.X - size, origin.Y - size);

                    newVec.X += i;
                    newVec.Y += j;

                    grid.Add(newVec);
                }
            }

            return grid;
        }

        private bool CheckFarmerProfession(SFarmer farmerInput, int professionIndex)
        {
            foreach (int i in farmerInput.professions)
            {
                if (i == professionIndex)
                    return true;
            }
            return false;
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
                magical = false,
                namesOfOkayBuildingLocations = new List<string> { "Farm" }
            };
        }

        /// <summary>Get all garages in the given location.</summary>
        /// <param name="location">The location to search.</param>
        private IEnumerable<Building> GetGarages(BuildableGameLocation location)
        {
            return location.buildings.Where(building => building.buildingType == this.GarageBuildingType);
        }
    }
}
