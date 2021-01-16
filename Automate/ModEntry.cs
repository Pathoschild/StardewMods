using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using Pathoschild.Stardew.Automate.Framework;
using Pathoschild.Stardew.Automate.Framework.Machines.Buildings;
using Pathoschild.Stardew.Automate.Framework.Models;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Messages;
using Pathoschild.Stardew.Common.Utilities;
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
        private ModConfigKeys Keys;

        /// <summary>Constructs machine groups.</summary>
        private MachineGroupFactory Factory;

        /// <summary>Handles console commands from players.</summary>
        private CommandHandler CommandHandler;

        /// <summary>Whether to enable automation for the current save.</summary>
        private bool EnableAutomation => Context.IsMainPlayer;

        /// <summary>An aggregate collection of machine groups linked by Junimo chests.</summary>
        private JunimoMachineGroup JunimoMachineGroup;

        /// <summary>The machines to process.</summary>
        private readonly List<IMachineGroup> ActiveMachineGroups = new();

        /// <summary>The disabled machine groups (e.g. machines not connected to a chest).</summary>
        private readonly List<IMachineGroup> DisabledMachineGroups = new();

        /// <summary>The locations that should be reloaded on the next update tick.</summary>
        private readonly HashSet<GameLocation> ReloadQueue = new HashSet<GameLocation>(new ObjectReferenceComparer<GameLocation>());

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
            this.Keys = this.Config.Controls.ParseControls(helper.Input, this.Monitor);
            this.Factory = new MachineGroupFactory(this.Config);
            this.Factory.Add(new AutomationFactory(
                connectors: this.Config.ConnectorNames,
                monitor: this.Monitor,
                reflection: helper.Reflection,
                data: data,
                betterJunimosCompat: this.Config.ModCompatibility.BetterJunimos && helper.ModRegistry.IsLoaded("hawkfalcon.BetterJunimos"),
                autoGrabberModCompat: this.Config.ModCompatibility.AutoGrabberMod && helper.ModRegistry.IsLoaded("Jotser.AutoGrabberMod"),
                pullGemstonesFromJunimoHuts: this.Config.PullGemstonesFromJunimoHuts
            ));
            this.JunimoMachineGroup = new(this.Factory.SortMachines);
            this.CommandHandler = new CommandHandler(this.Monitor, this.Config, this.Factory, this.GetActiveMachineGroups);

            // hook events
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.Player.Warped += this.OnWarped;
            helper.Events.World.BuildingListChanged += this.OnBuildingListChanged;
            helper.Events.World.LocationListChanged += this.OnLocationListChanged;
            helper.Events.World.ObjectListChanged += this.OnObjectListChanged;
            helper.Events.World.TerrainFeatureListChanged += this.OnTerrainFeatureListChanged;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
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
            return new AutomateAPI(this.Monitor, this.Factory, this.GetForApi);
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
            this.ActiveMachineGroups.Clear();
            this.DisabledMachineGroups.Clear();
            this.AutomateCountdown = this.Config.AutomationInterval;
            this.DisableOverlay();
            this.ReloadQueue.AddMany(CommonHelper.GetLocations());
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
                // remove locations
                if (e.Removed.Any())
                {
                    foreach (GameLocation location in e.Removed)
                    {
                        string locationKey = this.Factory.GetLocationKey(location);

                        this.ActiveMachineGroups.RemoveAll(p => p.LocationKey == locationKey);
                        this.DisabledMachineGroups.RemoveAll(p => p.LocationKey == locationKey);
                        this.JunimoMachineGroup.RemoveAll(p => p.LocationKey == locationKey);
                    }

                    this.JunimoMachineGroup.Rebuild();
                }

                // add locations
                this.ReloadQueue.AddMany(e.Added);
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

            if (e.Location is BuildableGameLocation buildableLocation && e.Added.Concat(e.Removed).Any(building => this.Factory.IsAutomatable(buildableLocation, new Vector2(building.tileX.Value, building.tileY.Value), building)))
            {
                this.Monitor.VerboseLog($"Building list changed in {e.Location.Name}, reloading its machines.");
                this.ReloadQueue.Add(e.Location);
            }
        }

        /// <summary>The method invoked when an object is added or removed to a location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnObjectListChanged(object sender, ObjectListChangedEventArgs e)
        {
            if (!this.EnableAutomation)
                return;

            if (e.Added.Concat(e.Removed).Any(obj => this.Factory.IsAutomatable(e.Location, obj.Key, obj.Value)))
            {
                this.Monitor.VerboseLog($"Object list changed in {e.Location.Name}, reloading its machines.");
                this.ReloadQueue.Add(e.Location);
            }
        }

        /// <summary>The method invoked when a terrain feature is added or removed to a location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnTerrainFeatureListChanged(object sender, TerrainFeatureListChangedEventArgs e)
        {
            if (!this.EnableAutomation)
                return;

            if (e.Added.Concat(e.Removed).Any(obj => this.Factory.IsAutomatable(e.Location, obj.Key, obj.Value)))
            {
                this.Monitor.VerboseLog($"Terrain feature list changed in {e.Location.Name}, reloading its machines.");
                this.ReloadQueue.Add(e.Location);
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
                if (this.ReloadQueue.Any())
                {
                    this.ReloadMachinesIn(this.ReloadQueue);
                    this.ReloadQueue.Clear();

                    this.ResetOverlayIfShown();
                }

                // process machines
                foreach (IMachineGroup group in this.GetActiveMachineGroups())
                    group.Automate();
            }
            catch (Exception ex)
            {
                this.HandleError(ex, "processing machines");
            }
        }

        /// <summary>The method invoked when the player presses a button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            try
            {
                // toggle overlay
                if (Context.IsPlayerFree && this.Keys.ToggleOverlay.JustPressedUnique())
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
                    this.ReloadQueue.Add(location);
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

            // migrate legacy fields
            if (config.ExtensionFields != null)
            {
                // migrate AutomateShippingBin (1.10.4–1.17.3)
                try
                {
                    if (config.ExtensionFields.TryGetValue("AutomateShippingBin", out JToken raw))
                        config.GetOrAddMachineOverrides(ShippingBinMachine.ShippingBinId).Enabled = raw.ToObject<bool>();
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"Failed migrating legacy 'AutomateShippingBin' config field, ignoring previous value.\n\n{ex}", LogLevel.Warn);
                }

                // migrate MachinePriority field (1.17–1.17.3) to MachineSettings
                // (and fix wrong "ShippingBinMachine" default value)
                try
                {
                    if (config.ExtensionFields.TryGetValue("MachinePriority", out JToken raw))
                    {
                        var priorities = raw.ToObject<Dictionary<string, int>>() ?? new Dictionary<string, int>();
                        foreach (var pair in priorities)
                        {
                            string key = pair.Key == "ShippingBinMachine" ? ShippingBinMachine.ShippingBinId : pair.Key;
                            config.GetOrAddMachineOverrides(key).Priority = pair.Value;
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"Failed migrating legacy 'MachinePriority' config field, ignoring previous value.\n\n{ex}", LogLevel.Warn);
                }

                config.ExtensionFields.Clear();
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

        /// <summary>Get the machine groups in every location.</summary>
        private IEnumerable<IMachineGroup> GetActiveMachineGroups()
        {
            if (this.JunimoMachineGroup.HasInternalAutomation)
                yield return this.JunimoMachineGroup;

            foreach (IMachineGroup group in this.ActiveMachineGroups)
                yield return group;
        }

        /// <summary>Get the active and disabled machine groups in a specific location for the API.</summary>
        /// <param name="location">The location whose machine groups to fetch.</param>
        private IEnumerable<IMachineGroup> GetForApi(GameLocation location)
        {
            string locationKey = this.Factory.GetLocationKey(location);

            return this
                .ActiveMachineGroups
                .Concat(this.DisabledMachineGroups)
                .Concat(this.JunimoMachineGroup.GetAll())
                .Where(p => p.LocationKey == locationKey);
        }

        /// <summary>Reload the machines in a given location.</summary>
        /// <param name="locations">The locations whose machines to reload.</param>
        private void ReloadMachinesIn(ISet<GameLocation> locations)
        {
            bool junimoGroupChanged;

            // remove old groups
            {
                HashSet<string> locationKeys = new(locations.Select(this.Factory.GetLocationKey));
                this.Monitor.VerboseLog($"Reloading machines in {locationKeys.Count} locations: {string.Join(", ", locationKeys)}...");

                this.DisabledMachineGroups.RemoveAll(p => locationKeys.Contains(p.LocationKey));
                this.ActiveMachineGroups.RemoveAll(p => locationKeys.Contains(p.LocationKey));
                junimoGroupChanged = this.JunimoMachineGroup.RemoveAll(p => locationKeys.Contains(p.LocationKey));
            }

            // add new groups
            foreach (GameLocation location in locations)
            {
                // collect new groups
                List<IMachineGroup> active = new();
                List<IMachineGroup> disabled = new();
                List<IMachineGroup> junimo = new();
                foreach (IMachineGroup group in this.Factory.GetMachineGroups(location))
                {
                    if (!group.HasInternalAutomation)
                        disabled.Add(group);

                    else if (group.IsJunimoGroup)
                        junimo.Add(group);

                    else
                        active.Add(group);
                }

                // add groups
                this.DisabledMachineGroups.AddRange(disabled);
                this.ActiveMachineGroups.AddRange(active);
                if (junimo.Any())
                {
                    this.JunimoMachineGroup.Add(junimo.ToArray());
                    junimoGroupChanged = true;
                }
            }

            // rebuild index
            if (junimoGroupChanged)
                this.JunimoMachineGroup.Rebuild();
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
            this.CurrentOverlay ??= new OverlayMenu(this.Helper.Events, this.Helper.Input, this.Helper.Reflection, this.Factory.GetMachineGroups(Game1.currentLocation));
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
