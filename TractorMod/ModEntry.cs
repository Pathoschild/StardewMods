using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common.Integrations.FarmExpansion;
using Pathoschild.Stardew.Common.Utilities;
using Pathoschild.Stardew.TractorMod.Framework;
using Pathoschild.Stardew.TractorMod.Framework.Attachments;
using Pathoschild.Stardew.TractorMod.Framework.Config;
using Pathoschild.Stardew.TractorMod.Framework.ModAttachments;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;

namespace Pathoschild.Stardew.TractorMod
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod, IAssetLoader
    {
        /*********
        ** Fields
        *********/
        /****
        ** Constants
        ****/
        /// <summary>The <see cref="Building.maxOccupants"/> value which identifies a tractor garage.</summary>
        private readonly int MaxOccupantsID = -794739;

        /// <summary>The update rate when only one player is in a location (as a frame multiple).</summary>
        private readonly uint TextureUpdateRateWithSinglePlayer = 30;

        /// <summary>The update rate when multiple players are in the same location (as a frame multiple). This should be more frequent due to sprite broadcasts, new horses instances being created during NetRef&lt;Horse&gt; syncs, etc.</summary>
        private readonly uint TextureUpdateRateWithMultiplePlayers = 3;

        /// <summary>The full type name for the Farm Expansion's construction menu.</summary>
        private readonly string FarmExpansionMenuFullName = "FarmExpansion.Menus.FECarpenterMenu";

        /// <summary>The full type name for the Pelican Fiber mod's construction menu.</summary>
        private readonly string PelicanFiberMenuFullName = "PelicanFiber.Framework.ConstructionMenu";

        /// <summary>The building type for the garage blueprint.</summary>
        private readonly string BlueprintBuildingType = "TractorGarage";

        /// <summary>The minimum version the host must have for the mod to be enabled on a farmhand.</summary>
        private readonly string MinHostVersion = "4.7-alpha.2";

        /// <summary>The absolute path to legacy mod data for the current save.</summary>
        private string LegacySaveDataRelativePath => Path.Combine("data", $"{Constants.SaveFolderName}.json");

        /****
        ** State
        ****/
        /// <summary>The mod settings.</summary>
        private ModConfig Config;

        /// <summary>The tractor being ridden by the current player.</summary>
        private TractorManager TractorManager;

        /// <summary>The garage texture to apply.</summary>
        private Texture2D GarageTexture;

        /// <summary>The tractor texture to apply.</summary>
        private Texture2D TractorTexture;

        /// <summary>Whether the mod is enabled for the current farmhand.</summary>
        private bool IsEnabled = true;

        /// <summary>Content packs loaded for TractorMod.</summary>
        private IEnumerable<IContentPack> ContentPacks;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // read config
            this.Config = helper.ReadConfig<ModConfig>();

            // init tractor logic
            StandardAttachmentsConfig attachmentConfig = this.Config.StandardAttachments;
            this.TractorManager = new TractorManager(this.Config, this.Helper.Translation, this.Helper.Reflection, attachments: new IAttachment[]
            {
                new CustomAttachment(this.Config.CustomAttachments), // should be first so it can override default attachments
                new AxeAttachment(attachmentConfig.Axe),
                new FertilizerAttachment(attachmentConfig.Fertilizer),
                new GrassStarterAttachment(attachmentConfig.GrassStarter),
                new HoeAttachment(attachmentConfig.Hoe),
                new MeleeWeaponAttachment(attachmentConfig.MeleeWeapon),
                new PickaxeAttachment(attachmentConfig.PickAxe),
                new ScytheAttachment(attachmentConfig.Scythe),
                new SeedAttachment(attachmentConfig.Seeds),
                new SeedBagAttachment(attachmentConfig.SeedBagMod),
                new SlingshotAttachment(attachmentConfig.Slingshot, this.Helper.Reflection),
                new WateringCanAttachment(attachmentConfig.WateringCan)
            });

            // hook events
            IModEvents events = helper.Events;
            events.GameLoop.GameLaunched += this.OnGameLaunched;
            events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            events.GameLoop.DayStarted += this.OnDayStarted;
            events.GameLoop.DayEnding += this.OnDayEnding;
            events.GameLoop.Saving += this.OnSaving;
            if (this.Config.HighlightRadius)
                events.Display.Rendered += this.OnRendered;
            events.Display.MenuChanged += this.OnMenuChanged;
            events.Input.ButtonPressed += this.OnButtonPressed;
            events.World.NpcListChanged += this.OnNpcListChanged;
            events.World.LocationListChanged += this.OnLocationListChanged;
            events.GameLoop.UpdateTicked += this.OnUpdateTicked;

            // validate translations
            if (!helper.Translation.GetTranslations().Any())
                this.Monitor.Log("The translation files in this mod's i18n folder seem to be missing. The mod will still work, but you'll see 'missing translation' messages. Try reinstalling the mod to fix this.", LogLevel.Warn);
        }

        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanLoad<T>(IAssetInfo asset)
        {
            // Allow for garages from older versions that didn't get normalised correctly.
            // This can be removed once support for legacy data is dropped.
            return asset.AssetNameEquals($"Buildings/{this.BlueprintBuildingType}");
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
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // add to Farm Expansion carpenter menu
            FarmExpansionIntegration farmExpansion = new FarmExpansionIntegration(this.Helper.ModRegistry, this.Monitor);
            if (farmExpansion.IsLoaded)
            {
                farmExpansion.AddFarmBluePrint(this.GetBlueprint());
                farmExpansion.AddExpansionBluePrint(this.GetBlueprint());
            }

            // get the content packs that may contain assets for the tractor or garage
            this.ContentPacks = this.Helper.ContentPacks.GetOwned();
        }

        /// <summary>The event called after a save slot is loaded.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            // load legacy data
            if (Context.IsMainPlayer)
                this.LoadLegacyData();

            // check if mod should be enabled for the current player
            this.IsEnabled = Context.IsMainPlayer;
            if (!this.IsEnabled)
            {
                ISemanticVersion hostVersion = this.Helper.Multiplayer.GetConnectedPlayer(Game1.MasterPlayer.UniqueMultiplayerID)?.GetMod(this.ModManifest.UniqueID)?.Version;
                if (hostVersion == null)
                {
                    this.IsEnabled = false;
                    this.Monitor.Log("This mod is disabled because the host player doesn't have it installed.", LogLevel.Warn);
                }
                else if (hostVersion.IsOlderThan(this.MinHostVersion))
                {
                    this.IsEnabled = false;
                    this.Monitor.Log($"This mod is disabled because the host player has {this.ModManifest.Name} {hostVersion}, but the minimum compatible version is {this.MinHostVersion}.", LogLevel.Warn);
                }
                else
                    this.IsEnabled = true;
            }
        }

        /// <summary>The event called when a new day begins.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            if (!this.IsEnabled)
                return;

            // reload textures
            this.TractorTexture = this.GetTexture("tractor");
            this.GarageTexture = this.GetTexture("garage");

            // init garages + tractors
            if (Context.IsMainPlayer)
            {
                foreach (BuildableGameLocation location in this.GetBuildableLocations())
                {
                    foreach (Stable garage in this.GetGaragesIn(location))
                    {
                        // spawn new tractor if needed
                        Horse tractor = this.FindHorse(garage.HorseId);
                        if (!garage.isUnderConstruction())
                        {
                            int x = garage.tileX.Value + 1;
                            int y = garage.tileY.Value + 1;
                            if (tractor == null)
                            {
                                tractor = new Horse(garage.HorseId, x, y);
                                location.addCharacter(tractor);
                            }
                            tractor.DefaultPosition = new Vector2(x, y);
                        }

                        // normalise tractor
                        if (tractor != null)
                            tractor.Name = TractorManager.GetTractorName(garage.HorseId);

                        // apply textures
                        this.ApplyTextures(garage);
                        this.ApplyTextures(tractor);
                    }
                }
            }
        }

        /// <summary>The event called after the location list changes.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnLocationListChanged(object sender, LocationListChangedEventArgs e)
        {
            if (!this.IsEnabled)
                return;

            // rescue lost tractors
            if (Context.IsMainPlayer)
            {
                foreach (GameLocation location in e.Removed)
                {
                    foreach (Horse tractor in this.GetTractorsIn(location).ToArray())
                        TractorManager.SetLocation(tractor, Game1.getFarm(), tractor.DefaultPosition);
                }
            }
        }

        /// <summary>The event called after the list of NPCs in a location changes.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnNpcListChanged(object sender, NpcListChangedEventArgs e)
        {
            if (!this.IsEnabled)
                return;

            // workaround for mines not spawning a ladder on monster levels
            if (e.Location is MineShaft mine && e.IsCurrentLocation)
            {
                if (mine.mustKillAllMonstersToAdvance() && mine.characters.All(p => p is Horse))
                {
                    IReflectedField<bool> hasLadder = this.Helper.Reflection.GetField<bool>(mine, "ladderHasSpawned");
                    if (!hasLadder.GetValue())
                    {
                        mine.createLadderAt(mine.mineEntrancePosition(Game1.player));
                        hasLadder.SetValue(true);
                    }
                }
            }

            // workaround for instantly-built tractors spawning a horse
            if (Context.IsMainPlayer && e.Location is BuildableGameLocation buildableLocation)
            {
                Horse[] horses = e.Added.OfType<Horse>().ToArray();
                if (horses.Any())
                {
                    HashSet<Guid> tractorIDs = new HashSet<Guid>(this.GetGaragesIn(buildableLocation).Select(p => p.HorseId));
                    foreach (Horse horse in horses)
                    {
                        if (tractorIDs.Contains(horse.HorseId) && !TractorManager.IsTractor(horse))
                            horse.Name = TractorManager.GetTractorName(horse.HorseId);
                    }
                }
            }
        }

        /// <summary>The event called when the game updates (roughly sixty times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!this.IsEnabled)
                return;

            // multiplayer: override textures in the current location
            if (Context.IsWorldReady && Game1.currentLocation != null)
            {
                uint updateRate = Game1.currentLocation.farmers.Count > 1 ? this.TextureUpdateRateWithMultiplePlayers : this.TextureUpdateRateWithSinglePlayer;
                if (e.IsMultipleOf(updateRate))
                {
                    foreach (Horse horse in this.GetTractorsIn(Game1.currentLocation))
                        this.ApplyTextures(horse);
                    foreach (Stable stable in this.GetGaragesIn(Game1.currentLocation))
                        this.ApplyTextures(stable);
                }
            }

            // override blueprint texture
            if (Game1.activeClickableMenu != null)
                this.ApplyTextures(Game1.activeClickableMenu);

            // update tractor effects
            if (Context.IsPlayerFree)
                this.TractorManager?.Update();
        }

        /// <summary>The event called before the day ends.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            if (!this.IsEnabled)
                return;

            if (Context.IsMainPlayer)
            {
                // collect valid stable IDs
                HashSet<Guid> validStableIDs = new HashSet<Guid>(
                    from location in this.GetBuildableLocations()
                    from garage in this.GetGaragesIn(location)
                    select garage.HorseId
                );

                // get locations reachable by Utility.findHorse
                HashSet<GameLocation> vanillaLocations = new HashSet<GameLocation>(Game1.locations, new ObjectReferenceComparer<GameLocation>());

                // clean up
                foreach (GameLocation location in this.GetLocations())
                {
                    bool isValidLocation = vanillaLocations.Contains(location);

                    foreach (Horse tractor in this.GetTractorsIn(location).ToArray())
                    {
                        // remove invalid tractor (e.g. building demolished)
                        if (!validStableIDs.Contains(tractor.HorseId))
                        {
                            location.characters.Remove(tractor);
                            continue;
                        }

                        // move tractor out of location that Utility.findHorse can't find
                        if (!isValidLocation)
                            Game1.warpCharacter(tractor, "Farm", new Point(0, 0));
                    }
                }
            }
        }

        /// <summary>The event called before the game starts saving.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaving(object sender, SavingEventArgs e)
        {
            if (!this.IsEnabled)
                return;

            // host: remove legacy data
            if (Context.IsMainPlayer)
            {
                // remove legacy file (pre-4.6)
                FileInfo legacyFile = new FileInfo(Path.Combine(this.Helper.DirectoryPath, this.LegacySaveDataRelativePath));
                if (legacyFile.Exists)
                    legacyFile.Delete();

                // remove legacy save data (4.6)
                this.Helper.Data.WriteSaveData<LegacySaveData>("tractors", null);
            }
        }

        /// <summary>The event called after the game draws to the screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRendered(object sender, RenderedEventArgs e)
        {
            if (!this.IsEnabled)
                return;

            // render debug radius
            if (this.Config.HighlightRadius && Context.IsWorldReady && Game1.activeClickableMenu == null && this.TractorManager?.IsCurrentPlayerRiding == true)
                this.TractorManager.DrawRadius(Game1.spriteBatch);
        }

        /// <summary>The event called after an active menu is opened or closed.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (!this.IsEnabled || !Context.IsWorldReady)
                return;

            // add blueprints
            if (e.NewMenu is CarpenterMenu || e.NewMenu?.GetType().FullName == this.PelicanFiberMenuFullName)
            {
                // get field
                IList<BluePrint> blueprints = this.Helper.Reflection
                    .GetField<List<BluePrint>>(e.NewMenu, e.NewMenu is CarpenterMenu ? "blueprints" : "Blueprints")
                    .GetValue();

                // add garage blueprint
                blueprints.Add(this.GetBlueprint());

                // add stable blueprint if needed
                // (If player built a tractor garage first, the game won't let them build a stable since it thinks they already have one. Derived from the CarpenterMenu constructor.)
                if (!blueprints.Any(p => p.name == "Stable" && p.maxOccupants != this.MaxOccupantsID))
                {
                    Farm farm = Game1.getFarm();

                    int cabins = farm.getNumberBuildingsConstructed("Cabin");
                    int stables = farm.getNumberBuildingsConstructed("Stable") - Game1.getFarm().buildings.OfType<Stable>().Count(this.IsGarage);
                    if (stables < cabins + 1)
                        blueprints.Add(new BluePrint("Stable"));
                }
            }
        }

        /// <summary>The event called when the player presses a keyboard button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!this.IsEnabled)
                return;

            // summon available tractor
            if (Context.IsPlayerFree && this.Config.Controls.SummonTractor.Contains(e.Button) && !Game1.player.isRidingHorse())
                this.SummonTractor();
        }

        /****
        ** Helper methods
        ****/
        /// <summary>Summon an unused tractor to the player's current position, if any are available.</summary>
        private void SummonTractor()
        {
            // find nearest horse in player's current location (if available)
            Horse horse = this
                .GetTractorsIn(Game1.currentLocation, includeMounted: false)
                .OrderBy(match => Utility.distance(Game1.player.getTileX(), Game1.player.getTileY(), match.getTileX(), match.getTileY()))
                .FirstOrDefault();

            // else get horse from any location
            if (horse == null)
            {
                horse = this
                    .GetLocations()
                    .SelectMany(location => this.GetTractorsIn(location, includeMounted: false))
                    .FirstOrDefault();
            }

            // warp to player
            if (horse != null)
                TractorManager.SetLocation(horse, Game1.currentLocation, Game1.player.getTileLocation());
        }

        /// <summary>Migrate tractors and garages from older versions of the mod.</summary>
        /// <remarks>The Robin construction logic is derived from <see cref="NPC.reloadSprite"/> and <see cref="Farm.resetForPlayerEntry"/>.</remarks>
        private void LoadLegacyData()
        {
            // fix building types
            foreach (BuildableGameLocation location in this.GetBuildableLocations())
            {
                foreach (Stable stable in location.buildings.OfType<Stable>())
                {
                    if (stable.buildingType.Value == this.BlueprintBuildingType)
                    {
                        stable.buildingType.Value = "Stable";
                        stable.maxOccupants.Value = this.MaxOccupantsID;
                    }
                }
            }

            // get save data
            LegacySaveData saveData = this.Helper.Data.ReadSaveData<LegacySaveData>("tractors"); // 4.6
            if (saveData?.Buildings == null)
                saveData = this.Helper.Data.ReadJsonFile<LegacySaveData>(this.LegacySaveDataRelativePath); // pre-4.6
            if (saveData?.Buildings == null)
                return;

            // add tractor + garages
            BuildableGameLocation[] locations = this.GetBuildableLocations().ToArray();
            foreach (LegacySaveDataBuilding garageData in saveData.Buildings)
            {
                // get location
                BuildableGameLocation location = locations.FirstOrDefault(p => (p.uniqueName.Value ?? p.Name) == (garageData.Map ?? "Farm"));
                if (location == null)
                {
                    this.Monitor.Log($"Ignored legacy tractor garage in unknown location '{garageData.Map}'.", LogLevel.Warn);
                    continue;
                }

                // add garage
                Stable garage = location.buildings.OfType<Stable>().FirstOrDefault(p => p.tileX.Value == (int)garageData.Tile.X && p.tileY.Value == (int)garageData.Tile.Y);
                if (garage == null)
                {
                    garage = new Stable(garageData.TractorID, this.GetBlueprint(), garageData.Tile);
                    garage.daysOfConstructionLeft.Value = 0;
                    location.buildings.Add(garage);
                }
                garage.maxOccupants.Value = this.MaxOccupantsID;
                garage.load();
            }
        }

        /// <summary>Get all available locations.</summary>
        private IEnumerable<GameLocation> GetLocations()
        {
            GameLocation[] mainLocations = (Context.IsMainPlayer ? Game1.locations : this.Helper.Multiplayer.GetActiveLocations()).ToArray();

            foreach (GameLocation location in mainLocations.Concat(MineShaft.activeMines))
            {
                yield return location;

                if (location is BuildableGameLocation buildableLocation)
                {
                    foreach (Building building in buildableLocation.buildings)
                    {
                        if (building.indoors.Value != null)
                            yield return building.indoors.Value;
                    }
                }
            }
        }

        /// <summary>Get all available buildable locations.</summary>
        private IEnumerable<BuildableGameLocation> GetBuildableLocations()
        {
            return this.GetLocations().OfType<BuildableGameLocation>();
        }

        /// <summary>Get all tractors in the given location.</summary>
        /// <param name="location">The location to scan.</param>
        /// <param name="includeMounted">Whether to include horses that are currently being ridden.</param>
        private IEnumerable<Horse> GetTractorsIn(GameLocation location, bool includeMounted = true)
        {
            // single-player
            if (!Context.IsMultiplayer || !includeMounted)
                return location.characters.OfType<Horse>().Where(this.IsTractor);

            // multiplayer
            return
                location.characters.OfType<Horse>().Where(this.IsTractor)
                    .Concat(
                        from player in location.farmers
                        where this.IsTractor(player.mount)
                        select player.mount
                    )
                    .Distinct(new ObjectReferenceComparer<Horse>());
        }

        /// <summary>Get all tractor garages in the given location.</summary>
        /// <param name="location">The location to scan.</param>
        private IEnumerable<Stable> GetGaragesIn(GameLocation location)
        {
            return location is BuildableGameLocation buildableLocation
                ? buildableLocation.buildings.OfType<Stable>().Where(this.IsGarage)
                : Enumerable.Empty<Stable>();
        }

        /// <summary>Find all horses with a given ID.</summary>
        /// <param name="id">The unique horse ID.</param>
        private Horse FindHorse(Guid id)
        {
            foreach (GameLocation location in this.GetLocations())
            {
                foreach (Horse horse in location.characters.OfType<Horse>())
                {
                    if (horse.HorseId == id)
                        return horse;
                }
            }

            return null;
        }

        /// <summary>Get whether a stable is a tractor garage.</summary>
        /// <param name="stable">The stable to check.</param>
        private bool IsGarage(Stable stable)
        {
            return
                stable != null
                && (
                    stable.maxOccupants.Value == this.MaxOccupantsID
                    || stable.buildingType.Value == this.BlueprintBuildingType // freshly constructed, not yet normalised
                );
        }

        /// <summary>Get whether a horse is a tractor.</summary>
        /// <param name="horse">The horse to check.</param>
        private bool IsTractor(Horse horse)
        {
            return TractorManager.IsTractor(horse);
        }

        /// <summary>Get a blueprint to construct the tractor garage.</summary>
        private BluePrint GetBlueprint()
        {
            return new BluePrint("Stable")
            {
                displayName = this.Helper.Translation.Get("garage.name"),
                description = this.Helper.Translation.Get("garage.description"),
                maxOccupants = this.MaxOccupantsID,
                moneyRequired = this.Config.BuildPrice,
                tilesWidth = 4,
                tilesHeight = 2,
                sourceRectForMenuView = new Rectangle(0, 0, 64, 96),
                itemsRequired = this.Config.BuildMaterials
            };
        }

        /// <summary>Get the asset texture from a content pack or the assets folder (including seasonal logic if applicable).</summary>
        /// <param name="spritesheet">The spritesheet name without the path or extension (like 'tractor' or 'garage').</param>
        private Texture2D GetTexture(string spritesheet)
        {
            // Try to load the assets from a content pack
            // If there are multiple content packs loaded, always use the first one
            foreach (IContentPack contentPack in this.ContentPacks)
            {
                try
                {
                    // try to get the seasonal asset
                    return contentPack.LoadAsset<Texture2D>(this.FormatSeasonalSpritesheet(spritesheet));
                }
                catch
                {
                    // do nothing
                    this.Monitor.Log($"No {Game1.currentSeason}_{spritesheet}.png found in {contentPack.Manifest.UniqueID} ", LogLevel.Trace);
                }

                try
                {
                    // try to get the non-seasonal asset
                    return contentPack.LoadAsset<Texture2D>($"{spritesheet}.png");
                }
                catch
                {
                    // do nothing
                    this.Monitor.Log($"No {spritesheet}.png found in {contentPack.Manifest.UniqueID} ", LogLevel.Trace);
                }
            }

            // use the default spriteseets since an asset could not be loaded from a content pack
            return this.Helper.Content.Load<Texture2D>(this.GetTextureKey(spritesheet));
        }

        /// <summary>Format the spritesheet with seasonal logic.</summary>
        /// <param name="spritesheet">The spritesheet name without the path or extension (like 'tractor' or 'garage').</param>
        private string FormatSeasonalSpritesheet(string spritesheet)
        {
            return $"{Game1.currentSeason}_{spritesheet}.png";
        }

        /// <summary>Get the asset key for a texture from the assets folder (including seasonal logic if applicable).</summary>
        /// <param name="spritesheet">The spritesheet name without the path or extension (like 'tractor' or 'garage').</param>
        private string GetTextureKey(string spritesheet)
        {
            string seasonalKey = this.FormatSeasonalSpritesheet(spritesheet);

            // try seasonal texture
            if (File.Exists(Path.Combine(this.Helper.DirectoryPath, seasonalKey)))
                return seasonalKey;

            // default to single texture
            return $"assets/{spritesheet}.png";
        }

        /// <summary>Apply the mod textures to the given menu, if applicable.</summary>
        /// <param name="menu">The menu to change.</param>
        private void ApplyTextures(IClickableMenu menu)
        {
            // vanilla menu
            if (menu is CarpenterMenu carpenterMenu)
            {
                if (carpenterMenu.CurrentBlueprint.maxOccupants == this.MaxOccupantsID)
                {
                    Building building = this.Helper.Reflection.GetField<Building>(carpenterMenu, "currentBuilding").GetValue();
                    if (building.texture.Value != this.GarageTexture)
                        building.texture = new Lazy<Texture2D>(() => this.GarageTexture);
                }
                return;
            }

            // Farm Expansion & Pelican Fiber menus
            bool isFarmExpansion = menu.GetType().FullName == this.FarmExpansionMenuFullName;
            bool isPelicanFiber = !isFarmExpansion && menu.GetType().FullName == this.PelicanFiberMenuFullName;
            if (isFarmExpansion || isPelicanFiber)
            {
                BluePrint currentBlueprint = this.Helper.Reflection.GetProperty<BluePrint>(menu, "CurrentBlueprint").GetValue();
                if (currentBlueprint.maxOccupants == this.MaxOccupantsID)
                {
                    Building building = this.Helper.Reflection.GetField<Building>(menu, isFarmExpansion ? "currentBuilding" : "CurrentBuilding").GetValue();
                    if (building.texture.Value != this.GarageTexture)
                        building.texture = new Lazy<Texture2D>(() => this.GarageTexture);
                }
            }
        }

        /// <summary>Apply the mod textures to the given stable, if applicable.</summary>
        /// <param name="horse">The horse to change.</param>
        private void ApplyTextures(Horse horse)
        {
            if (this.IsTractor(horse))
                this.Helper.Reflection.GetField<Texture2D>(horse.Sprite, "spriteTexture").SetValue(this.TractorTexture);
        }

        /// <summary>Apply the mod textures to the given stable, if applicable.</summary>
        /// <param name="stable">The stable to change.</param>
        private void ApplyTextures(Stable stable)
        {
            if (this.IsGarage(stable))
                stable.texture = new Lazy<Texture2D>(() => this.GarageTexture);
        }
    }
}
