using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
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

        /// <summary>A request from a farmhand to warp a tractor to the given player.</summary>
        private readonly string RequestTractorMessageID = "TractorRequest";

        /// <summary>The absolute path to legacy mod data for the current save.</summary>
        private string LegacySaveDataRelativePath => Path.Combine("data", $"{Constants.SaveFolderName}.json");

        /****
        ** State
        ****/
        /// <summary>The mod settings.</summary>
        private ModConfig Config;

        /// <summary>The configured key bindings.</summary>
        private ModConfigKeys Keys;

        /// <summary>The tractor being ridden by the current player.</summary>
        private TractorManager TractorManager;

        /// <summary>The garage texture to apply.</summary>
        private Texture2D GarageTexture;

        /// <summary>The tractor texture to apply.</summary>
        private Texture2D TractorTexture;

        /// <summary>Whether the mod is enabled for the current farmhand.</summary>
        private bool IsEnabled = true;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // read config
            this.Config = helper.ReadConfig<ModConfig>();
            this.Keys = this.Config.Controls.ParseControls(helper.Input, this.Monitor);

            // init tractor logic
            this.TractorManager = new TractorManager(this.Config, this.Keys, this.Helper.Translation, this.Helper.Reflection);
            this.UpdateConfig();

            // hook events
            IModEvents events = helper.Events;
            events.GameLoop.GameLaunched += this.OnGameLaunched;
            events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            events.GameLoop.DayStarted += this.OnDayStarted;
            events.GameLoop.DayEnding += this.OnDayEnding;
            events.GameLoop.Saving += this.OnSaving;
            events.Display.Rendered += this.OnRendered;
            events.Display.MenuChanged += this.OnMenuChanged;
            events.Input.ButtonPressed += this.OnButtonPressed;
            events.World.NpcListChanged += this.OnNpcListChanged;
            events.World.LocationListChanged += this.OnLocationListChanged;
            events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            events.Multiplayer.ModMessageReceived += this.OnModMessageReceived;
            events.Player.Warped += this.OnWarped;

            // validate translations
            if (!helper.Translation.GetTranslations().Any())
                this.Monitor.Log("The translation files in this mod's i18n folder seem to be missing. The mod will still work, but you'll see 'missing translation' messages. Try reinstalling the mod to fix this.", LogLevel.Warn);
        }

        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanLoad<T>(IAssetInfo asset)
        {
            // Allow for garages from older versions that didn't get normalized correctly.
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

            // add Generic Mod Config Menu integration
            new GenericModConfigMenuIntegrationForTractor(
                getConfig: () => this.Config,
                getKeys: () => this.Keys,
                reset: () =>
                {
                    this.Config = new ModConfig();
                    this.Helper.WriteConfig(this.Config);
                    this.UpdateConfig();
                },
                saveAndApply: () =>
                {
                    this.Helper.WriteConfig(this.Config);
                    this.UpdateConfig();
                },
                modRegistry: this.Helper.ModRegistry,
                monitor: this.Monitor,
                manifest: this.ModManifest
            ).Register();
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
            this.TractorTexture = this.Helper.Content.Load<Texture2D>(this.GetTextureKey("tractor"));
            this.GarageTexture = this.Helper.Content.Load<Texture2D>(this.GetTextureKey("garage"));

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
                            Vector2 tractorTile = this.GetDefaultTractorTile(garage);
                            if (tractor == null)
                            {
                                tractor = new Horse(garage.HorseId, (int)tractorTile.X, (int)tractorTile.Y);
                                location.addCharacter(tractor);
                            }
                            tractor.DefaultPosition = tractorTile;
                        }

                        // normalize tractor
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
                        this.DismissTractor(tractor);
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

        /// <summary>The event called after the player warps into a new location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnWarped(object sender, WarpedEventArgs e)
        {
            if (!e.IsLocalPlayer || !this.TractorManager.IsCurrentPlayerRiding)
                return;

            // fix: warping onto a magic warp while mounted causes an infinite warp loop
            Vector2 tile = CommonHelper.GetPlayerTile(Game1.player);
            string touchAction = Game1.currentLocation.doesTileHaveProperty((int)tile.X, (int)tile.Y, "TouchAction", "Back");
            if (this.TractorManager.IsCurrentPlayerRiding && touchAction != null && touchAction.StartsWith("MagicWarp "))
                Game1.currentLocation.lastTouchActionLocation = tile;

            // fix: warping into an event may break the event (e.g. Mr Qi's event on mine level event for the 'Cryptic Note' quest)
            if (Game1.CurrentEvent != null)
                Game1.player.mount.dismount();
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
                    .GetField<List<BluePrint>>(e.NewMenu, "blueprints")
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
            if (!this.IsEnabled || !Context.IsPlayerFree)
                return;

            if (this.Keys.SummonTractor.JustPressedUnique() && !Game1.player.isRidingHorse())
                this.SummonTractor();
            else if (this.Keys.DismissTractor.JustPressedUnique() && Game1.player.isRidingHorse())
                this.DismissTractor(Game1.player.mount);
        }

        /// <summary>Raised after a mod message is received over the network.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            // tractor request from a farmhand
            if (e.Type == this.RequestTractorMessageID && Context.IsMainPlayer && e.FromModID == this.ModManifest.UniqueID)
            {
                Farmer player = Game1.getFarmer(e.FromPlayerID);
                if (player != null && !player.IsMainPlayer)
                {
                    this.Monitor.Log(
                        this.SummonLocalTractorTo(player)
                            ? $"Summon tractor for {player.Name} ({e.FromPlayerID})."
                            : $"Received tractor request for {player.Name} ({e.FromPlayerID}), but no tractor is available.",
                        LogLevel.Trace
                    );
                }
                else
                    this.Monitor.Log($"Received tractor request for {e.FromPlayerID}, but no such player was found.", LogLevel.Trace);
            }
        }

        /****
        ** Helper methods
        ****/
        /// <summary>Apply the mod configuration if it changed.</summary>
        private void UpdateConfig()
        {
            this.Keys = this.Config.Controls.ParseControls(this.Helper.Input, this.Monitor);

            var modRegistry = this.Helper.ModRegistry;
            var reflection = this.Helper.Reflection;
            var toolConfig = this.Config.StandardAttachments;
            this.TractorManager.UpdateConfig(this.Config, this.Keys, new IAttachment[]
            {
                new CustomAttachment(this.Config.CustomAttachments, modRegistry, reflection), // should be first so it can override default attachments
                new AxeAttachment(toolConfig.Axe, modRegistry, reflection),
                new FertilizerAttachment(toolConfig.Fertilizer, modRegistry, reflection),
                new GrassStarterAttachment(toolConfig.GrassStarter, modRegistry, reflection),
                new HoeAttachment(toolConfig.Hoe, modRegistry, reflection),
                new MeleeWeaponAttachment(toolConfig.MeleeWeapon, modRegistry, reflection),
                new MilkPailAttachment(toolConfig.MilkPail, modRegistry, reflection),
                new PickaxeAttachment(toolConfig.PickAxe, modRegistry, reflection),
                new ScytheAttachment(toolConfig.Scythe, modRegistry, reflection),
                new SeedAttachment(toolConfig.Seeds, modRegistry, reflection),
                modRegistry.IsLoaded(SeedBagAttachment.ModId) ? new SeedBagAttachment(toolConfig.SeedBagMod, modRegistry, reflection) : null,
                new ShearsAttachment(toolConfig.Shears, modRegistry, reflection),
                new SlingshotAttachment(toolConfig.Slingshot, modRegistry, reflection),
                new WateringCanAttachment(toolConfig.WateringCan, modRegistry, reflection)
            });
        }

        /// <summary>Summon an unused tractor to the player's current position, if any are available.</summary>
        private void SummonTractor()
        {
            bool summoned = this.SummonLocalTractorTo(Game1.player);
            if (!summoned && !Context.IsMainPlayer)
            {
                this.Monitor.Log("Sending tractor request to host player.", LogLevel.Trace);
                this.Helper.Multiplayer.SendMessage(
                    message: true,
                    messageType: this.RequestTractorMessageID,
                    modIDs: new[] { this.ModManifest.UniqueID },
                    playerIDs: new[] { Game1.MasterPlayer.UniqueMultiplayerID }
                );
            }
        }

        /// <summary>Summon an unused tractor to a player's current position, if any are available. If the player is a farmhand in multiplayer, only tractors in synced locations can be found by this method.</summary>
        /// <param name="player">The target player.</param>
        /// <returns>Returns whether a tractor was successfully summoned.</returns>
        private bool SummonLocalTractorTo(Farmer player)
        {
            // get player info
            if (player == null)
                return false;
            GameLocation location = player.currentLocation;
            Vector2 tile = player.getTileLocation();

            // find nearest tractor in player's current location (if available), else any location
            Horse tractor = this
                .GetTractorsIn(location, includeMounted: false)
                .OrderBy(match => Utility.distance(tile.X, tile.Y, match.getTileX(), match.getTileY()))
                .FirstOrDefault();
            if (tractor == null)
            {
                tractor = this
                    .GetLocations()
                    .SelectMany(loc => this.GetTractorsIn(loc, includeMounted: false))
                    .FirstOrDefault();
            }

            // create a tractor if needed
            if (tractor == null && this.Config.CanSummonWithoutGarage && Context.IsMainPlayer)
            {
                Guid id = Guid.NewGuid();
                tractor = new Horse(id, 0, 0) { Name = TractorManager.GetTractorName(id) };
                this.ApplyTextures(tractor);
            }

            // warp to player
            if (tractor != null)
            {
                TractorManager.SetLocation(tractor, location, tile);
                return true;
            }
            return false;
        }

        /// <summary>Send a tractor back home.</summary>
        /// <param name="tractor">The tractor to dismiss.</param>
        private void DismissTractor(Horse tractor)
        {
            if (tractor == null || !this.IsTractor(tractor))
                return;

            // dismount
            if (tractor.rider != null)
                tractor.dismount();

            // get home position (garage may have been moved since the tractor was spawned)
            Farm location = Game1.getFarm();
            Stable garage = location.buildings.OfType<Stable>().FirstOrDefault(p => p.HorseId == tractor.HorseId);
            Vector2 tile = garage != null
                ? this.GetDefaultTractorTile(garage)
                : tractor.DefaultPosition;

            // warp home
            TractorManager.SetLocation(tractor, location, tile);
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
                BuildableGameLocation location = locations.FirstOrDefault(p => p.NameOrUniqueName == (garageData.Map ?? "Farm"));
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
                    || stable.buildingType.Value == this.BlueprintBuildingType // freshly constructed, not yet normalized
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

        /// <summary>Get the default tractor tile position in a garage.</summary>
        /// <param name="garage">The tractor's home garage.</param>
        private Vector2 GetDefaultTractorTile(Stable garage)
        {
            return new Vector2(garage.tileX.Value + 1, garage.tileY.Value + 1);
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
                BluePrint currentBlueprint = this.Helper.Reflection.GetProperty<BluePrint>(menu, isFarmExpansion ? "CurrentBlueprint" : "currentBlueprint").GetValue();
                if (currentBlueprint.maxOccupants == this.MaxOccupantsID)
                {
                    Building building = this.Helper.Reflection.GetField<Building>(menu, "currentBuilding").GetValue();
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
