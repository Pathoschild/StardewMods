using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.TractorMod.Framework;
using Pathoschild.Stardew.TractorMod.Framework.Attachments;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.TractorMod
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

        /// <summary>The full type name for the Pelican Fiber mod's construction menu.</summary>
        private readonly string PelicanFiberMenuFullName = "PelicanFiber.Framework.ConstructionMenu";

        /// <summary>The number of days needed to build a tractor garage.</summary>
        private readonly int GarageConstructionDays = 3;

        /****
        ** State
        ****/
        /// <summary>The mod settings.</summary>
        private ModConfig Config;

        /// <summary>The tractor attachments to apply.</summary>
        private IAttachment[] Attachments;

        /// <summary>The current player's farm.</summary>
        private Farm Farm;

        /// <summary>Manages the tractor instance.</summary>
        private TractorManager Tractor;

        /// <summary>Whether Robin is busy constructing a garage.</summary>
        private bool IsRobinBusy;

        /// <summary>The tractor garages which started construction today.</summary>
        private readonly List<Building> GaragesStartedToday = new List<Building>();

        /// <summary>Whether the player has any tractor garages.</summary>
        private bool HasAnyGarages;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // read config
            this.MigrateLegacySaveData(helper);
            this.Config = helper.ReadConfig<ModConfig>();
            this.Attachments = new IAttachment[]
            {
                new CustomToolAttachment(this.Config), // should be first so it can override tools
                new AxeAttachment(this.Config),
                new FertilizerAttachment(),
                new HoeAttachment(),
                new PickaxeAttachment(this.Config),
                new ScytheAttachment(),
                new SeedAttachment(),
                new WateringCanAttachment()
            };

            // spawn/unspawn tractor and garages
            TimeEvents.AfterDayStarted += this.TimeEvents_AfterDayStarted;
            SaveEvents.BeforeSave += this.SaveEvents_BeforeSave;

            // add blueprint to Robin's shop
            MenuEvents.MenuChanged += this.MenuEvents_MenuChanged;

            // handle player interaction & tractor logic
            ControlEvents.KeyPressed += this.ControlEvents_KeyPressed;
            GameEvents.UpdateTick += this.GameEvents_UpdateTick;

            // validate translations
            if (!helper.Translation.GetTranslations().Any())
                this.Monitor.Log("The translation files in this mod's i18n folder seem to be missing. The mod will still work, but you'll see 'missing translation' messages. Try reinstalling the mod to fix this.", LogLevel.Warn);
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
            this.Tractor = null;
            this.GaragesStartedToday.Clear();
            this.Farm = Game1.getFarm();
            this.RestoreCustomData();
            this.HasAnyGarages = this.GetGarages(this.Farm).Any();
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
            if (Context.IsWorldReady && !this.HasAnyGarages)
            {
                if (e.NewMenu is CarpenterMenu)
                {
                    this.Helper.Reflection
                        .GetPrivateValue<List<BluePrint>>(e.NewMenu, "blueprints")
                        .Add(this.GetBlueprint());
                }
                else if (e.NewMenu.GetType().FullName == this.PelicanFiberMenuFullName)
                {
                    this.Helper.Reflection
                        .GetPrivateValue<List<BluePrint>>(e.NewMenu, "Blueprints")
                        .Add(this.GetBlueprint());
                }
            }
        }

        /// <summary>The event called when the player presses a keyboard button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ControlEvents_KeyPressed(object sender, EventArgsKeyPressed e)
        {
            // summon tractor
            if (e.KeyPressed == this.Config.TractorKey)
                this.Tractor?.SetLocation(Game1.currentLocation, Game1.player.getTileLocation());
        }

        /// <summary>The event called when the game updates (roughly sixty times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void GameEvents_UpdateTick(object sender, EventArgs e)
        {
            if (Game1.activeClickableMenu is CarpenterMenu || Game1.activeClickableMenu?.GetType().FullName == this.PelicanFiberMenuFullName)
                this.ProcessNewConstruction();
            if (Context.IsPlayerFree)
                this.Tractor?.Update();
        }

        /****
        ** State methods
        ****/
        /// <summary>Detect and fix tractor garages that started construction today.</summary>
        private void ProcessNewConstruction()
        {
            foreach (Building garage in this.GetGarages(this.Farm).Keys)
            {
                this.HasAnyGarages = true;

                // skip if not built today
                if (garage is TractorGarage)
                    continue;

                // set construction days after it's placed
                if (!this.GaragesStartedToday.Contains(garage))
                {
                    garage.daysOfConstructionLeft = this.GarageConstructionDays;
                    this.GaragesStartedToday.Add(garage);
                }

                // spawn tractor if built instantly by a mod
                if (!garage.isUnderConstruction())
                {
                    this.GaragesStartedToday.Remove(garage);
                    this.Farm.destroyStructure(garage);
                    this.Farm.buildings.Add(new TractorGarage(this.GetBlueprint(), new Vector2(garage.tileX, garage.tileY), 0));
                    if (this.Tractor == null)
                        this.Tractor = this.SpawnTractor(garage.tileX + 1, garage.tileY + 1);
                }
            }
        }

        /// <summary>Spawn a new tractor.</summary>
        /// <param name="tileX">The tile X position at which to spawn it.</param>
        /// <param name="tileY">The tile Y position at which to spawn it.</param>
        private TractorManager SpawnTractor(int tileX, int tileY)
        {
            TractorManager tractor = new TractorManager(tileX, tileY, this.Config, this.Attachments, this.Helper.Content, this.Helper.Translation, this.Helper.Reflection);
            tractor.SetLocation(this.Farm, new Vector2(tileX, tileY));
            tractor.SetPixelPosition(new Vector2(tractor.Current.Position.X + 20, tractor.Current.Position.Y));
            return tractor;
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
            IDictionary<Building, CustomSaveBuilding> garages = this.GetGarages(this.Farm);
            CustomSaveData saveData = new CustomSaveData(garages.Values);
            this.Helper.WriteJsonFile(this.GetDataPath(Constants.SaveFolderName), saveData);

            // remove tractors + buildings
            foreach (Building garage in garages.Keys)
                this.Farm.destroyStructure(garage);
            this.Tractor?.RemoveTractors();

            // reset Robin construction
            if (this.IsRobinBusy)
            {
                this.IsRobinBusy = false;
                NPC robin = Game1.getCharacterFromName("Robin");
                robin.ignoreScheduleToday = false;
                robin.CurrentDialogue.Clear();
                robin.dayUpdate(Game1.dayOfMonth);
            }
        }

        /// <summary>Restore tractor and garage data removed by <see cref="StashCustomData"/>.</summary>
        /// <remarks>The Robin construction logic is derived from <see cref="NPC.reloadSprite"/> and <see cref="StardewValley.Farm.resetForPlayerEntry"/>.</remarks>
        private void RestoreCustomData()
        {
            // get save data
            CustomSaveData saveData = this.Helper.ReadJsonFile<CustomSaveData>(this.GetDataPath(Constants.SaveFolderName));
            if (saveData?.Buildings == null)
                return;

            // add tractor + garages
            BluePrint blueprint = this.GetBlueprint();
            foreach (CustomSaveBuilding garageData in saveData.Buildings)
            {
                // add garage
                TractorGarage garage = new TractorGarage(blueprint, garageData.Tile, Math.Max(0, garageData.DaysOfConstructionLeft - 1));
                this.Farm.buildings.Add(garage);

                // add Robin construction
                if (garage.isUnderConstruction() && !this.IsRobinBusy)
                {
                    this.IsRobinBusy = true;
                    NPC robin = Game1.getCharacterFromName("Robin");

                    // update Robin
                    robin.ignoreMultiplayerUpdates = true;
                    robin.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
                    {
                        new FarmerSprite.AnimationFrame(24, 75),
                        new FarmerSprite.AnimationFrame(25, 75),
                        new FarmerSprite.AnimationFrame(26, 300, false, false, farmer => this.Helper.Reflection.GetPrivateMethod(robin,"robinHammerSound").Invoke(farmer)),
                        new FarmerSprite.AnimationFrame(27, 1000, false, false, farmer => this.Helper.Reflection.GetPrivateMethod(robin,"robinVariablePause").Invoke(farmer))
                    });
                    robin.ignoreScheduleToday = true;
                    Game1.warpCharacter(robin, this.Farm.Name, new Vector2(garage.tileX + garage.tilesWide / 2, garage.tileY + garage.tilesHigh / 2), false, false);
                    robin.position.X += Game1.tileSize / 4;
                    robin.position.Y -= Game1.tileSize / 2;
                    robin.CurrentDialogue.Clear();
                    robin.CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3926"), robin));
                }

                // spawn tractor
                if (this.Tractor == null && !garage.isUnderConstruction())
                    this.Tractor = this.SpawnTractor(garage.tileX + 1, garage.tileY + 1);
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
                            saveData.TractorHouse.Select(p => new CustomSaveBuilding(new Vector2(p.X, p.Y), this.GarageBuildingType, 0))
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

        /****
        ** Helper methods
        ****/
        /// <summary>Get garages in the given location to save.</summary>
        /// <param name="location">The location to search.</param>
        private IDictionary<Building, CustomSaveBuilding> GetGarages(BuildableGameLocation location)
        {
            return
                (
                    from building in location.buildings
                    where building.buildingType == this.GarageBuildingType
                    select new { Key = building, Value = new CustomSaveBuilding(new Vector2(building.tileX, building.tileY), this.GarageBuildingType, building.daysOfConstructionLeft) }
                )
                .ToDictionary(p => p.Key, p => p.Value);
        }

        /// <summary>Get a blueprint to construct the tractor garage.</summary>
        private BluePrint GetBlueprint()
        {
            return new BluePrint(this.GarageBuildingType)
            {
                name = this.GarageBuildingType,
                texture = this.Helper.Content.Load<Texture2D>(@"assets\TractorHouse.png"),
                humanDoor = new Point(-1, -1),
                animalDoor = new Point(-2, -1),
                mapToWarpTo = null,
                displayName = this.Helper.Translation.Get("garage.name"),
                description = this.Helper.Translation.Get("garage.description"),
                blueprintType = "Buildings",
                nameOfBuildingToUpgrade = null,
                actionBehavior = null,
                maxOccupants = -1,
                moneyRequired = this.Config.BuildPrice,
                tilesWidth = 4,
                tilesHeight = 2,
                sourceRectForMenuView = new Rectangle(0, 0, 64, 96),
                itemsRequired = this.Config.BuildUsesResources
                    ? new Dictionary<int, int> { [SObject.ironBar] = 20, [SObject.iridiumBar] = 5, [787/* battery pack */] = 5 }
                    : new Dictionary<int, int>(),
                namesOfOkayBuildingLocations = new List<string> { "Farm" }
            };
        }
    }
}
