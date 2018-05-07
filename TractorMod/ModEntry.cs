using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Integrations.FarmExpansion;
using Pathoschild.Stardew.TractorMod.Framework;
using Pathoschild.Stardew.TractorMod.Framework.Attachments;
using Pathoschild.Stardew.TractorMod.Framework.Config;
using Pathoschild.Stardew.TractorMod.Framework.ModAttachments;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.TractorMod
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod, IAssetLoader
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

        /// <summary>Whether the mod is enabled.</summary>
        private bool IsEnabled = true;

        /// <summary>Manages the tractor instance.</summary>
        private Tractor Tractor;

        /// <summary>Whether Robin is busy constructing a garage.</summary>
        private bool IsRobinBusy;

        /// <summary>Whether the player has any tractor garages.</summary>
        private bool HasAnyGarages;

        /// <summary>Whether the Pelican Fiber mod is loaded.</summary>
        private bool IsPelicanFiberLoaded;

        /// <summary>The tractor texture to display.</summary>
        private Texture2D TractorTexture;

        /// <summary>The garage texture to display.</summary>
        private Texture2D GarageTexture;

        /// <summary>The season for which the textures were loaded.</summary>
        private string TextureSeason;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // enable mod compatibility fixes
            this.IsPelicanFiberLoaded = helper.ModRegistry.IsLoaded("jwdred.PelicanFiber");

            // read config
            this.MigrateLegacySaveData(helper);
            this.Config = helper.ReadConfig<ModConfig>();

            // init attachments
            StandardAttachmentsConfig attachmentConfig = this.Config.StandardAttachments;
            this.Attachments = new IAttachment[]
            {
                new CustomAttachment(this.Config.CustomAttachments), // should be first so it can override default attachments
                new AxeAttachment(attachmentConfig.Axe),
                new FertilizerAttachment(attachmentConfig.Fertilizer),
                new GrassStarterAttachment(attachmentConfig.GrassStarter),
                new HoeAttachment(attachmentConfig.Hoe),
                new PickaxeAttachment(attachmentConfig.PickAxe),
                new ScytheAttachment(attachmentConfig.Scythe),
                new SeedAttachment(attachmentConfig.Seeds),
                new SeedBagAttachment(attachmentConfig.SeedBagMod),
                new WateringCanAttachment(attachmentConfig.WateringCan)
            };

            // hook events
            GameEvents.FirstUpdateTick += this.GameEvents_FirstUpdateTick;
            TimeEvents.AfterDayStarted += this.TimeEvents_AfterDayStarted;
            SaveEvents.BeforeSave += this.SaveEvents_BeforeSave;
            if (this.Config.HighlightRadius)
                GraphicsEvents.OnPostRenderEvent += this.GraphicsEvents_OnPostRenderEvent;
            MenuEvents.MenuChanged += this.MenuEvents_MenuChanged;
            InputEvents.ButtonPressed += this.InputEvents_ButtonPressed;
            GameEvents.UpdateTick += this.GameEvents_UpdateTick;
            PlayerEvents.Warped += this.PlayerEvents_Warped;
            LocationEvents.LocationsChanged += this.LocationEvents_LocationsChanged;
            LocationEvents.BuildingsChanged += this.LocationEvents_BuildingsChanged;

            // validate translations
            if (!helper.Translation.GetTranslations().Any())
                this.Monitor.Log("The translation files in this mod's i18n folder seem to be missing. The mod will still work, but you'll see 'missing translation' messages. Try reinstalling the mod to fix this.", LogLevel.Warn);
        }

        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanLoad<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("Buildings/TractorGarage");
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public T Load<T>(IAssetInfo asset)
        {
            return (T)(object)this.GarageTexture;
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Event handlers
        ****/
        /// <summary>The event called after the first game update, once all mods are loaded.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void GameEvents_FirstUpdateTick(object sender, EventArgs e)
        {
            // enable Farm Expansion integration
            FarmExpansionIntegration farmExpansion = new FarmExpansionIntegration(this.Helper.ModRegistry, this.Monitor);
            if (farmExpansion.IsLoaded)
            {
                farmExpansion.AddFarmBluePrint(this.GetBlueprint());
                farmExpansion.AddExpansionBluePrint(this.GetBlueprint());
            }
        }

        /// <summary>The event called when a new day begins.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void TimeEvents_AfterDayStarted(object sender, EventArgs e)
        {
            // disable in multiplayer mode
            this.IsEnabled = !Context.IsMultiplayer;
            if (!this.IsEnabled)
            {
                this.Monitor.Log("Disabled mod; not compatible with multiplayer mode yet.", LogLevel.Warn);
                return;
            }

            // set textures
            if (this.GarageTexture == null || this.TractorTexture == null || this.TextureSeason != Game1.Date.Season)
            {
                this.TractorTexture = this.Helper.Content.Load<Texture2D>(this.GetTextureKey("tractor"));
                this.GarageTexture = this.Helper.Content.Load<Texture2D>(this.GetTextureKey("garage")); // preload asset to avoid errors if loaded during draw loop
                this.TextureSeason = Game1.Date.Season;
            }

            // set up for new day
            this.Tractor = null;
            this.RestoreCustomData();
            this.HasAnyGarages = this.FindGarages().Any();
        }

        /// <summary>The event called before the game starts saving.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void SaveEvents_BeforeSave(object sender, EventArgs e)
        {
            if (!this.IsEnabled)
                return;

            this.StashCustomData();
        }

        /// <summary>The event called when the game is drawing to the screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void GraphicsEvents_OnPostRenderEvent(object sender, EventArgs e)
        {
            if (!this.IsEnabled)
                return;

            if (Context.IsWorldReady && Game1.activeClickableMenu == null && this.Config.HighlightRadius && this.Tractor?.IsRiding == true)
                this.Tractor?.DrawRadius(Game1.spriteBatch);
        }

        /// <summary>The event called after a new menu is opened.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void MenuEvents_MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            if (!this.IsEnabled)
                return;

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
            }
        }

        /// <summary>The event called when a building is added or removed in a location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void LocationEvents_LocationsChanged(object sender, EventArgsLocationsChanged e)
        {
            foreach (BuildableGameLocation location in e.Added.OfType<BuildableGameLocation>())
            {
                foreach (Building building in location.buildings)
                    this.FixGarage(building, location);
            }
        }

        /// <summary>The event called when a building is added or removed in a location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void LocationEvents_BuildingsChanged(object sender, EventArgsLocationBuildingsChanged e)
        {
            if (e.Location is BuildableGameLocation location)
            {
                foreach (Building building in e.Added)
                    this.FixGarage(building, location);
            }
        }

        /// <summary>The event called when the player presses a keyboard button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void InputEvents_ButtonPressed(object sender, EventArgsInput e)
        {
            if (!this.IsEnabled)
                return;

            // summon tractor
            if (Context.IsPlayerFree && this.Config.Controls.SummonTractor.Contains(e.Button))
                this.Tractor?.SetLocation(Game1.currentLocation, Game1.player.getTileLocation());
        }

        /// <summary>The event called when the game updates (roughly sixty times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void GameEvents_UpdateTick(object sender, EventArgs e)
        {
            if (!this.IsEnabled)
                return;

            if (Context.IsPlayerFree)
                this.Tractor?.Update();
        }

        /// <summary>The event called when the player warps to a new location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void PlayerEvents_Warped(object sender, EventArgsPlayerWarped e)
        {
            if (!this.IsEnabled)
                return;

            this.Tractor?.UpdateForNewLocation(e.PriorLocation, e.NewLocation);
        }

        /****
        ** State methods
        ****/
        /// <summary>Fix a garage if necessary so it has the correct custom behavior.</summary>
        /// <param name="building">The building to fix.</param>
        /// <param name="location">The location containing the building.</param>
        /// <returns>Returns whether the building is a tractor.</returns>
        private void FixGarage(Building building, BuildableGameLocation location)
        {
            // skip if no fix needed
            if (building is TractorGarage || building.buildingType.Value != this.GarageBuildingType)
                return;

            // replace building
            location.destroyStructure(building);
            this.SpawnGarage(location, building.tileX.Value, building.tileY.Value, building.daysOfConstructionLeft.Value, Guid.NewGuid(), 0);
        }

        /// <summary>Spawn a garage in a location (and tractor if needed).</summary>
        /// <param name="tractorID">The unique ID for the associated tractor.</param>
        /// <param name="location">The location in which to spawn a garage.</param>
        /// <param name="tileX">The tile X position at which to spawn it.</param>
        /// <param name="tileY">The tile Y position at which to spawn it.</param>
        /// <param name="daysOfConstructionLeft">The days of construction left.</param>
        /// <param name="hatID">The tractor's hat ID.</param>
        [SuppressMessage("ReSharper", "PossibleLossOfFraction", Justification = "Deliberate conversion from pixels to tiles.")]
        private void SpawnGarage(BuildableGameLocation location, int tileX, int tileY, int daysOfConstructionLeft, Guid tractorID, int? hatID)
        {
            // add new garage
            TractorGarage garage = new TractorGarage(tractorID, this.GetBlueprint(), new Vector2(tileX, tileY), daysOfConstructionLeft);
            location.buildings.Add(garage);

            // add Robin construction
            if (garage.isUnderConstruction() && !this.IsRobinBusy && garage.daysOfConstructionLeft.Value < this.GarageConstructionDays)
            {
                this.IsRobinBusy = true;
                NPC robin = Game1.getCharacterFromName("Robin");

                // update Robin
                robin.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
                {
                    new FarmerSprite.AnimationFrame(24, 75),
                    new FarmerSprite.AnimationFrame(25, 75),
                    new FarmerSprite.AnimationFrame(26, 300, false, false, farmer => this.Helper.Reflection.GetMethod(robin,"robinHammerSound").Invoke(farmer)),
                    new FarmerSprite.AnimationFrame(27, 1000, false, false, farmer => this.Helper.Reflection.GetMethod(robin,"robinVariablePause").Invoke(farmer))
                });
                robin.ignoreScheduleToday = true;
                Game1.warpCharacter(robin, location, new Vector2(garage.tileX.Value + garage.tilesWide.Value / 2, garage.tileY.Value + garage.tilesHigh.Value / 2));
                robin.position.X += Game1.tileSize / 4;
                robin.position.Y -= Game1.tileSize / 2;
                robin.CurrentDialogue.Clear();
                robin.CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3926"), robin));
            }

            // spawn tractor if needed
            if (this.Tractor == null && !garage.isUnderConstruction())
            {
                Tractor tractor = new Tractor(tractorID, tileX + 1, tileY + 1, this.Config, this.Attachments, this.Helper.Content.GetActualAssetKey(this.GetTextureKey("tractor")), this.Helper.Translation, this.Helper.Reflection);
                tractor.SetLocation(location, new Vector2(tileX + 1, tileY + 1));
                if (hatID.HasValue)
                    tractor.hat.Value = new Hat(hatID.Value);
                this.Tractor = tractor;
            }
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
            // stash data
            GarageMetadata[] garages = this.FindGarages().ToArray();
            CustomSaveData saveData = new CustomSaveData(garages.Select(p => p.SaveData));
            this.Helper.WriteJsonFile(this.GetDataPath(Constants.SaveFolderName), saveData);

            // remove tractors + buildings
            foreach (GarageMetadata garage in garages)
                garage.Location.destroyStructure(garage.Building);
            foreach (GameLocation location in CommonHelper.GetLocations())
                location.characters.Filter(p => !(p is Tractor));

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
                int daysOfConstructionLeft = Math.Max(0, garageData.DaysOfConstructionLeft - 1);
                this.SpawnGarage(location, (int)garageData.Tile.X, (int)garageData.Tile.Y, daysOfConstructionLeft, garageData.TractorID, garageData.TractorHatID);
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
                            saveData.TractorHouse.Select(p => new CustomSaveBuilding(new Vector2(p.X, p.Y), Guid.NewGuid(), null, this.GarageBuildingType, "Farm", 0))
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
        /// <summary>Get all tractor garages in the game.</summary>
        private IEnumerable<GarageMetadata> FindGarages()
        {
            foreach (BuildableGameLocation location in CommonHelper.GetLocations().OfType<BuildableGameLocation>())
            {
                foreach (TractorGarage garage in location.buildings.OfType<TractorGarage>())
                {
                    Tractor tractor = Utility.findHorse(garage.HorseId) as Tractor;
                    int? tractorHatID = tractor?.hat.Value?.which;
                    var saveData = new CustomSaveBuilding(new Vector2(garage.tileX.Value, garage.tileY.Value), garage.HorseId, tractorHatID, this.GarageBuildingType, this.GetMapName(location), garage.daysOfConstructionLeft.Value);

                    yield return new GarageMetadata(location, garage, saveData);
                }
            }
        }

        /// <summary>Get a blueprint to construct the tractor garage.</summary>
        private BluePrint GetBlueprint()
        {
            BluePrint blueprint = new BluePrint("Stable") // init vanilla blueprint first to avoid errors
            {
                name = this.GarageBuildingType,
                humanDoor = new Point(-1, -1),
                animalDoor = new Point(-2, -1),
                mapToWarpTo = null,
                displayName = this.Helper.Translation.Get("garage.name"),
                description = this.Helper.Translation.Get("garage.description"),
                blueprintType = "Buildings",
                daysToConstruct = this.GarageConstructionDays,
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

            string textureKey = this.GetTextureKey("garage");
            this.Helper.Reflection.GetField<string>(blueprint, nameof(BluePrint.textureName)).SetValue(this.Helper.Content.GetActualAssetKey(textureKey));
            this.Helper.Reflection.GetField<Texture2D>(blueprint, nameof(BluePrint.texture)).SetValue(this.GarageTexture);

            return blueprint;
        }

        /// <summary>Get the asset key for a texture from the assets folder (including seasonal logic if applicable).</summary>
        /// <param name="spritesheet">The spritesheet name without the path or extension (like 'tractor' or 'garage').</param>
        private string GetTextureKey(string spritesheet)
        {
            // try seasonal texture
            string seasonalKey = $"assets/{Game1.currentSeason}_{spritesheet}.png";
            if (File.Exists(Path.Combine(this.Helper.DirectoryPath, seasonalKey)))
                return seasonalKey;

            // default to single texture
            return $"assets/{spritesheet}.png";
        }

        /// <summary>Get a unique map name for the given location.</summary>
        private string GetMapName(GameLocation location)
        {
            string uniqueName = location.uniqueName.Value;
            return uniqueName ?? location.Name;
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
