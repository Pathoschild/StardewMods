using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Automate.Framework;
using Pathoschild.Stardew.Automate.Framework.Models;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Messages;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;

namespace Pathoschild.Stardew.Automate
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod
    {
        /*********
        ** Fields
        *********/
        /// <summary>The mod configuration.</summary>
        private ModConfig Config;

        /// <summary>The configured key bindings.</summary>
        private ModConfigKeys Keys => this.Config.Controls;

        /// <summary>Manages machine groups.</summary>
        private MachineManager MachineManager;

        /// <summary>Handles console commands from players.</summary>
        private CommandHandler CommandHandler;

        /// <summary>Whether to enable automation for the current save.</summary>
        private bool EnableAutomation => Context.IsMainPlayer;

        /// <summary>The number of ticks until the next automation cycle.</summary>
        private int AutomateCountdown;

        /// <summary>The current overlay being displayed, if any.</summary>
        private OverlayMenu CurrentOverlay;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
        {
            // read data file
            const string dataPath = "assets/data.json";
            DataModel data = null;
            try
            {
                data = this.Helper.Data.ReadJsonFile<DataModel>(dataPath);
                if (data?.FloorNames == null)
                    this.Monitor.Log($"The {dataPath} file seems to be missing or invalid. Floor connectors will be disabled.", LogLevel.Error);
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"The {dataPath} file seems to be invalid. Floor connectors will be disabled.\n{ex}", LogLevel.Error);
            }

            // read config
            this.Config = this.LoadConfig();

            // init
            this.MachineManager = new MachineManager(
                config: this.Config,
                data: data,
                defaultFactory: new AutomationFactory(
                    connectors: this.Config.ConnectorNames,
                    monitor: this.Monitor,
                    reflection: helper.Reflection,
                    data: data,
                    betterJunimosCompat: this.Config.ModCompatibility.BetterJunimos && helper.ModRegistry.IsLoaded("hawkfalcon.BetterJunimos"),
                    autoGrabberModCompat: this.Config.ModCompatibility.AutoGrabberMod && helper.ModRegistry.IsLoaded("Jotser.AutoGrabberMod"),
                    pullGemstonesFromJunimoHuts: this.Config.PullGemstonesFromJunimoHuts
                ),
                monitor: this.Monitor
            );

            this.CommandHandler = new CommandHandler(this.Monitor, this.Config, this.MachineManager);

            // hook events
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.Player.Warped += this.OnWarped;
            helper.Events.World.BuildingListChanged += this.OnBuildingListChanged;
            helper.Events.World.LocationListChanged += this.OnLocationListChanged;
            helper.Events.World.ObjectListChanged += this.OnObjectListChanged;
            helper.Events.World.TerrainFeatureListChanged += this.OnTerrainFeatureListChanged;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
            helper.Events.Multiplayer.ModMessageReceived += this.OnModMessageReceived;

            // hook commands
            helper.ConsoleCommands.Add("automate", "Run commands from the Automate mod. Enter 'automate help' for more info.", this.CommandHandler.HandleCommand);

            // log info
            this.Monitor.VerboseLog($"Initialized with automation every {this.Config.AutomationInterval} ticks.");
            if (this.Config.ModCompatibility.WarnForMissingBridgeMod)
                this.ReportMissingBridgeMods(data?.SuggestedIntegrations);
        }

        /// <summary>Get an API that other mods can access. This is always called after <see cref="Entry" />.</summary>
        public override object GetApi()
        {
            return new AutomateAPI(this.Monitor, this.MachineManager);
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Event handlers
        ****/
        /// <summary>The method invoked when the player loads a save.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            // disable if secondary player
            if (!this.EnableAutomation)
            {
                if (this.HostHasAutomate(out ISemanticVersion installedVersion))
                    this.Monitor.Log($"Automate {installedVersion} is installed by the main player, so machines will be automated by their instance.");
                else
                    this.Monitor.Log("Automate isn't installed by the main player, so machines won't be automated.", LogLevel.Warn);
                return;
            }

            // reset
            this.MachineManager.Reset();
            this.AutomateCountdown = this.Config.AutomationInterval;
            this.DisableOverlay();
        }

        /// <summary>The method invoked when the player warps to a new location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnWarped(object sender, WarpedEventArgs e)
        {
            if (e.IsLocalPlayer)
                this.ResetOverlayIfShown();
        }

        /// <summary>The method invoked when a location is added or removed.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnLocationListChanged(object sender, LocationListChangedEventArgs e)
        {
            if (!this.EnableAutomation)
                return;

            this.Monitor.VerboseLog("Location list changed, reloading machines in affected locations.");

            try
            {
                if (e.Removed.Any())
                    this.MachineManager.QueueReload(e.Removed);

                this.MachineManager.QueueReload(e.Added);
            }
            catch (Exception ex)
            {
                this.HandleError(ex, "updating locations");
            }
        }

        /// <summary>The method raised after buildings are added or removed in a location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnBuildingListChanged(object sender, BuildingListChangedEventArgs e)
        {
            if (!this.EnableAutomation)
                return;

            if (e.Location is BuildableGameLocation buildableLocation && e.Added.Concat(e.Removed).Any(building => this.MachineManager.Factory.IsAutomatable(buildableLocation, new Vector2(building.tileX.Value, building.tileY.Value), building)))
            {
                this.Monitor.VerboseLog($"Building list changed in {e.Location.Name}, reloading its machines.");
                this.MachineManager.QueueReload(e.Location);
            }
        }

        /// <summary>The method invoked when an object is added or removed to a location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnObjectListChanged(object sender, ObjectListChangedEventArgs e)
        {
            if (!this.EnableAutomation)
                return;

            if (e.Added.Concat(e.Removed).Any(obj => this.MachineManager.Factory.IsAutomatable(e.Location, obj.Key, obj.Value)))
            {
                this.Monitor.VerboseLog($"Object list changed in {e.Location.Name}, reloading its machines.");
                this.MachineManager.QueueReload(e.Location);
            }
        }

        /// <summary>The method invoked when a terrain feature is added or removed to a location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnTerrainFeatureListChanged(object sender, TerrainFeatureListChangedEventArgs e)
        {
            if (!this.EnableAutomation)
                return;

            if (e.Added.Concat(e.Removed).Any(obj => this.MachineManager.Factory.IsAutomatable(e.Location, obj.Key, obj.Value)))
            {
                this.Monitor.VerboseLog($"Terrain feature list changed in {e.Location.Name}, reloading its machines.");
                this.MachineManager.QueueReload(e.Location);
            }
        }

        /// <summary>The method invoked when the in-game clock time changes.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady || !this.EnableAutomation)
                return;

            try
            {
                // handle delay
                this.AutomateCountdown--;
                if (this.AutomateCountdown > 0)
                    return;
                this.AutomateCountdown = this.Config.AutomationInterval;

                // reload machines if needed
                if (this.MachineManager.ReloadQueuedLocations())
                    this.ResetOverlayIfShown();

                // process machines
                foreach (IMachineGroup group in this.MachineManager.GetActiveMachineGroups())
                    group.Automate();
            }
            catch (Exception ex)
            {
                this.HandleError(ex, "processing machines");
            }
        }

        /// <summary>Raised after the player presses any buttons on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            try
            {
                // toggle overlay
                if (Context.IsPlayerFree && this.Keys.ToggleOverlay.JustPressed())
                {
                    if (this.CurrentOverlay != null)
                        this.DisableOverlay();
                    else
                        this.EnableOverlay();
                }
            }
            catch (Exception ex)
            {
                this.HandleError(ex, "handling key input");
            }
        }

        /// <summary>Raised after a mod message is received over the network.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            // update automation if chest options changed
            if (Context.IsMainPlayer && e.FromModID == "Pathoschild.ChestsAnywhere" && e.Type == nameof(AutomateUpdateChestMessage))
            {
                var message = e.ReadAs<AutomateUpdateChestMessage>();
                var location = Game1.getLocationFromName(message.LocationName);
                var player = Game1.getFarmer(e.FromPlayerID);

                string label = player != Game1.MasterPlayer
                    ? $"{player.Name}/{e.FromModID}"
                    : e.FromModID;

                if (location != null)
                {
                    this.Monitor.Log($"Received chest update from {label} for chest at {message.LocationName} ({message.Tile}), updating machines.");
                    this.MachineManager.QueueReload(location);
                }
                else
                    this.Monitor.Log($"Received chest update from {label} for chest at {message.LocationName} ({message.Tile}), but no such location was found.");
            }
        }

        /****
        ** Methods
        ****/
        /// <summary>Read the config file, migrating legacy settings if applicable.</summary>
        private ModConfig LoadConfig()
        {
            // read raw config
            var config = this.Helper.ReadConfig<ModConfig>();
            bool changed = false;

            // normalize machine settings
            config.MachineOverrides = new Dictionary<string, ModConfigMachine>(config.MachineOverrides ?? new Dictionary<string, ModConfigMachine>(), StringComparer.OrdinalIgnoreCase);
            foreach (string key in config.MachineOverrides.Where(p => p.Value == null).Select(p => p.Key).ToArray())
            {
                config.MachineOverrides.Remove(key);
                changed = true;
            }

            // resave changes
            if (changed)
                this.Helper.WriteConfig(config);

            return config;
        }

        /// <summary>Log warnings if custom-machine frameworks are installed without their automation component.</summary>
        /// <param name="integrations">Mods which add custom machine recipes and require a separate automation component.</param>
        private void ReportMissingBridgeMods(DataModelIntegration[] integrations)
        {
            if (integrations?.Any() != true)
                return;

            var registry = this.Helper.ModRegistry;
            foreach (var integration in integrations)
            {
                if (registry.IsLoaded(integration.Id) && !registry.IsLoaded(integration.SuggestedId))
                    this.Monitor.Log($"Machine recipes added by {integration.Name} aren't currently automated. Install {integration.SuggestedName} too to enable them: {integration.SuggestedUrl}.", LogLevel.Warn);
            }
        }

        /// <summary>Get whether the host player has Automate installed.</summary>
        /// <param name="version">The installed version, if any.</param>
        private bool HostHasAutomate(out ISemanticVersion version)
        {
            if (Context.IsMainPlayer)
            {
                version = this.ModManifest.Version;
                return true;
            }

            IMultiplayerPeer host = this.Helper.Multiplayer.GetConnectedPlayer(Game1.MasterPlayer.UniqueMultiplayerID);
            IMultiplayerPeerMod mod = host?.Mods?.SingleOrDefault(p => string.Equals(p.ID, this.ModManifest.UniqueID, StringComparison.OrdinalIgnoreCase));

            version = mod?.Version;
            return mod != null;
        }

        /// <summary>Log an error and warn the user.</summary>
        /// <param name="ex">The exception to handle.</param>
        /// <param name="verb">The verb describing where the error occurred (e.g. "looking that up").</param>
        private void HandleError(Exception ex, string verb)
        {
            this.Monitor.Log($"Something went wrong {verb}:\n{ex}", LogLevel.Error);
            CommonHelper.ShowErrorMessage($"Huh. Something went wrong {verb}. The error log has the technical details.");
        }

        /// <summary>Disable the overlay, if shown.</summary>
        private void DisableOverlay()
        {
            this.CurrentOverlay?.Dispose();
            this.CurrentOverlay = null;
        }

        /// <summary>Enable the overlay.</summary>
        private void EnableOverlay()
        {
            this.CurrentOverlay ??= new OverlayMenu(this.Helper.Events, this.Helper.Input, this.Helper.Reflection, this.MachineManager.Factory.GetMachineGroups(Game1.currentLocation));
        }

        /// <summary>Reset the overlay if it's being shown.</summary>
        private void ResetOverlayIfShown()
        {
            if (this.CurrentOverlay != null)
            {
                this.DisableOverlay();
                this.EnableOverlay();
            }
        }
    }
}
