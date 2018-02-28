using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.TractorMod.Framework;
using Pathoschild.Stardew.TractorMod.Framework.Attachments;
using Pathoschild.Stardew.TractorMod.Framework.ModAttachments;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.TractorMod
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod
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

        /// <summary>The full type name for the Farm Expansion mod's construction menu.</summary>
        private readonly string FarmExpansionMenuFullName = "FarmExpansion.Menus.FECarpenterMenu";

        /// <summary>The number of days needed to build a tractor garage.</summary>
        private readonly int GarageConstructionDays = 3;

        /****
        ** State
        ****/
        /// <summary>The mod settings.</summary>
        private ModConfig Config;

        /// <summary>The tractor attachments to apply.</summary>
        private IAttachment[] Attachments;

        /// <summary>Manages the tractor instance.</summary>
        private TractorManager Tractor;

        /// <summary>Whether Robin is busy constructing a garage.</summary>
        private bool IsRobinBusy;

        /// <summary>The tractor garages which started construction today.</summary>
        private readonly List<Building> GaragesStartedToday = new List<Building>();

        /// <summary>Whether the player has any tractor garages.</summary>
        private bool HasAnyGarages;

        /// <summary>Whether the Pelican Fiber mod is loaded.</summary>
        private bool IsPelicanFiberLoaded;

        /// <summary>Whether the Farm Expansion mod is loaded.</summary>
        private bool IsFarmExpansionLoaded;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // enable mod compatibility fixes
            this.IsPelicanFiberLoaded = helper.ModRegistry.IsLoaded("jwdred.PelicanFiber");
            this.IsFarmExpansionLoaded = helper.ModRegistry.IsLoaded("Advize.FarmExpansion") && helper.ModRegistry.Get("Advize.FarmExpansion").Version.IsNewerThan("3.0"); // fields added in 3.0.1

            // read config
            this.MigrateLegacySaveData(helper);
            this.Config = helper.ReadConfig<ModConfig>();
            this.Attachments = new IAttachment[]
            {
                new CustomAttachment(this.Config), // should be first so it can override default attachments
                new AxeAttachment(this.Config),
                new FertilizerAttachment(this.Config),
                new GrassStarterAttachment(this.Config),
                new HoeAttachment(this.Config),
                new PickaxeAttachment(this.Config),
                new ScytheAttachment(this.Config),
                new SeedAttachment(this.Config),
                new SeedBagAttachment(this.Config),
                new WateringCanAttachment(this.Config)
            };

            // hook events
            TimeEvents.AfterDayStarted += this.TimeEvents_AfterDayStarted;
            SaveEvents.BeforeSave += this.SaveEvents_BeforeSave;
            if (this.Config.HighlightRadius)
                GraphicsEvents.OnPostRenderEvent += this.GraphicsEvents_OnPostRenderEvent;
            MenuEvents.MenuChanged += this.MenuEvents_MenuChanged;
            InputEvents.ButtonPressed += this.InputEvents_ButtonPressed;
            GameEvents.UpdateTick += this.GameEvents_UpdateTick;
            LocationEvents.CurrentLocationChanged += this.LocationEvents_CurrentLocationChanged;

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
            this.RestoreCustomData();
            this.HasAnyGarages = this.GetGarages().Any();
        }

        /// <summary>The event called before the game starts saving.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void SaveEvents_BeforeSave(object sender, EventArgs e)
        {
            this.StashCustomData();
        }

        /// <summary>The event called when the game is drawing to the screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void GraphicsEvents_OnPostRenderEvent(object sender, EventArgs e)
        {
            if (Context.IsWorldReady && Game1.activeClickableMenu == null && this.Config.HighlightRadius && this.Tractor?.IsRiding == true)
                this.Tractor?.DrawRadius(Game1.spriteBatch);
        }

        /// <summary>The event called after a new menu is opened.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void MenuEvents_MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            // add blueprint to carpenter menu
            if (Context.IsWorldReady && !this.HasAnyGarages)
            {
                // default menu
                if (e.NewMenu is CarpenterMenu)
                {
                    this.Helper.Reflection
                        .GetField<List<BluePrint>>(e.NewMenu, "blueprints")
                        .GetValue()
                        .Add(this.GetBlueprint());
                }

                // modded menus
                else if (this.IsPelicanFiberLoaded && e.NewMenu.GetType().FullName == this.PelicanFiberMenuFullName)
                {
                    this.Helper.Reflection
                        .GetField<List<BluePrint>>(e.NewMenu, "Blueprints")
                        .GetValue()
                        .Add(this.GetBlueprint());
                }
                else if (this.IsFarmExpansionLoaded && e.NewMenu.GetType().FullName == this.FarmExpansionMenuFullName)
                {
                    this.Helper.Reflection
                        .GetMethod(e.NewMenu, "AddFarmBluePrint")
                        .Invoke(this.GetBlueprint());
                }
            }
        }

        /// <summary>The event called when the player presses a keyboard button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void InputEvents_ButtonPressed(object sender, EventArgsInput e)
        {
            // summon tractor
            if (Context.IsPlayerFree && this.Config.Controls.SummonTractor.Contains(e.Button))
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

        /// <summary>The event called when the player warps to a new location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void LocationEvents_CurrentLocationChanged(object sender, EventArgsCurrentLocationChanged e)
        {
            this.Tractor?.UpdateForNewLocation(e.PriorLocation, e.NewLocation);
        }

        /****
        ** State methods
        ****/
        /// <summary>Detect and fix tractor garages that started construction today.</summary>
        private void ProcessNewConstruction()
        {
            foreach (GarageMetadata metadata in this.GetGarages().ToArray())
            {
                this.HasAnyGarages = true;
                Building garage = metadata.Building;
                BuildableGameLocation location = metadata.Location;

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
                    location.destroyStructure(garage);
                    location.buildings.Add(new TractorGarage(this.GetBlueprint(), new Vector2(garage.tileX, garage.tileY), 0));
                    if (this.Tractor == null)
                        this.Tractor = this.SpawnTractor(location, garage.tileX + 1, garage.tileY + 1);
                }
            }
        }

        /// <summary>Spawn a new tractor.</summary>
        /// <param name="location">The location in which to spawn a tractor.</param>
        /// <param name="tileX">The tile X position at which to spawn it.</param>
        /// <param name="tileY">The tile Y position at which to spawn it.</param>
        private TractorManager SpawnTractor(BuildableGameLocation location, int tileX, int tileY)
        {
            TractorManager tractor = new TractorManager(tileX, tileY, this.Config, this.Attachments, this.GetTexture("tractor"), this.Helper.Translation, this.Helper.Reflection);
            tractor.SetLocation(location, new Vector2(tileX, tileY));
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
            GarageMetadata[] garages = this.GetGarages().ToArray();
            CustomSaveData saveData = new CustomSaveData(garages.Select(p => p.SaveData));
            this.Helper.WriteJsonFile(this.GetDataPath(Constants.SaveFolderName), saveData);

            // remove tractors + buildings
            foreach (var garage in garages)
                garage.Location.destroyStructure(garage.Building);
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
            BuildableGameLocation[] locations = CommonHelper.GetLocations().OfType<BuildableGameLocation>().ToArray();
            foreach (CustomSaveBuilding garageData in saveData.Buildings)
            {
                // get location
                BuildableGameLocation location = locations.FirstOrDefault(p => this.GetMapName(p) == (garageData.Map ?? "Farm"));
                if (location == null)
                {
                    this.Monitor.Log($"Ignored tractor garage in unknown location '{garageData.Map}'.");
                    continue;
                }

                // add garage
                TractorGarage garage = new TractorGarage(blueprint, garageData.Tile, Math.Max(0, garageData.DaysOfConstructionLeft - 1));
                location.buildings.Add(garage);

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
                        new FarmerSprite.AnimationFrame(26, 300, false, false, farmer => this.Helper.Reflection.GetMethod(robin,"robinHammerSound").Invoke(farmer)),
                        new FarmerSprite.AnimationFrame(27, 1000, false, false, farmer => this.Helper.Reflection.GetMethod(robin,"robinVariablePause").Invoke(farmer))
                    });
                    robin.ignoreScheduleToday = true;
                    Game1.warpCharacter(robin, location.Name, new Vector2(garage.tileX + garage.tilesWide / 2, garage.tileY + garage.tilesHigh / 2), false, false);
                    robin.position.X += Game1.tileSize / 4;
                    robin.position.Y -= Game1.tileSize / 2;
                    robin.CurrentDialogue.Clear();
                    robin.CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3926"), robin));
                }

                // spawn tractor
                if (this.Tractor == null && !garage.isUnderConstruction())
                    this.Tractor = this.SpawnTractor(location, garage.tileX + 1, garage.tileY + 1);
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
                            saveData.TractorHouse.Select(p => new CustomSaveBuilding(new Vector2(p.X, p.Y), this.GarageBuildingType, "Farm", 0))
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
        private IEnumerable<GarageMetadata> GetGarages()
        {
            return
                (
                    from location in CommonHelper.GetLocations().OfType<BuildableGameLocation>()
                    from building in location.buildings
                    where building.buildingType == this.GarageBuildingType
                    select new GarageMetadata(location, building, new CustomSaveBuilding(new Vector2(building.tileX, building.tileY), this.GarageBuildingType, this.GetMapName(location), building.daysOfConstructionLeft))
                );
        }

        /// <summary>Get a blueprint to construct the tractor garage.</summary>
        private BluePrint GetBlueprint()
        {
            return new BluePrint(this.GarageBuildingType)
            {
                name = this.GarageBuildingType,
                texture = this.GetTexture("garage"),
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

        /// <summary>Get a texture from the assets folder (including seasonal logic if applicable).</summary>
        /// <param name="key">The unique key without the path or extension (like 'tractor' or 'garage').</param>
        private Texture2D GetTexture(string key)
        {
            // try seasonal texture
            string seasonalKey = $"assets/{Game1.currentSeason}_{key}.png";
            if (File.Exists(Path.Combine(this.Helper.DirectoryPath, seasonalKey)))
                return this.Helper.Content.Load<Texture2D>(seasonalKey);

            // default to single texture
            return this.Helper.Content.Load<Texture2D>($"assets/{key}.png");
        }

        /// <summary>Get a unique map name for the given location.</summary>
        private string GetMapName(GameLocation location)
        {
            return location.uniqueName ?? location.Name;
        }


        /*********
        ** Private models
        *********/
        /// <summary>A model which represents garage instances found in the game.</summary>
        private class GarageMetadata
        {
            /*********
            ** Accessors
            *********/
            /// <summary>The location containing the garage.</summary>
            public BuildableGameLocation Location { get; }

            /// <summary>The garage building.</summary>
            public Building Building { get; }

            /// <summary>The garage save data.</summary>
            public CustomSaveBuilding SaveData { get; }


            /*********
            ** Public methods
            *********/
            /// <summary>Construct an instance.</summary>
            /// <param name="location">The location containing the garage.</param>
            /// <param name="building">The garage building.</param>
            /// <param name="saveData">The garage save data.</param>
            public GarageMetadata(BuildableGameLocation location, Building building, CustomSaveBuilding saveData)
            {
                this.Location = location;
                this.Building = building;
                this.SaveData = saveData;
            }
        }
    }
}
