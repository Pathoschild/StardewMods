using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pathoschild.Stardew.Automate.Framework;
using Pathoschild.Stardew.Automate.Framework.Models;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace Pathoschild.Stardew.Automate
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod
    {
        /*********
        ** Properties
        *********/
        /// <summary>The mod configuration.</summary>
        private ModConfig Config;

        /// <summary>Constructs machine instances.</summary>
        private readonly MachineFactory Factory = new MachineFactory();

        /// <summary>Whether to enable automation for the current save.</summary>
        private bool EnableAutomation => Context.IsMainPlayer;

        /// <summary>The machines to process.</summary>
        private readonly IDictionary<GameLocation, MachineGroup[]> MachineGroups = new Dictionary<GameLocation, MachineGroup[]>();

        /// <summary>The locations that should be reloaded on the next update tick.</summary>
        private readonly HashSet<GameLocation> ReloadQueue = new HashSet<GameLocation>();

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
            // read config
            this.Config = helper.ReadConfig<ModConfig>();

            // hook events
            SaveEvents.AfterLoad += this.SaveEvents_AfterLoad;
            PlayerEvents.Warped += this.PlayerEvents_Warped;
            LocationEvents.LocationsChanged += this.LocationEvents_LocationsChanged;
            LocationEvents.ObjectsChanged += this.LocationEvents_ObjectsChanged;
            GameEvents.UpdateTick += this.GameEvents_UpdateTick;

            // handle player interaction
            InputEvents.ButtonPressed += this.InputEvents_ButtonPressed;

            // log info
            if (this.Config.VerboseLogging)
                this.Monitor.Log($"Verbose logging is enabled. This is useful when troubleshooting but can impact performance. It should be disabled if you don't explicitly need it. You can delete {Path.Combine(this.Helper.DirectoryPath, "config.json")} and restart the game to disable it.", LogLevel.Warn);
            this.VerboseLog($"Initialised with automation every {this.Config.AutomationInterval} ticks.");
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
        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            // disable if secondary player
            if (!this.EnableAutomation)
                this.Monitor.Log("Disabled automation (only the main player can automate machines in multiplayer mode).", LogLevel.Warn);

            // reset
            this.MachineGroups.Clear();
            this.AutomateCountdown = this.Config.AutomationInterval;
            this.DisableOverlay();
            foreach (GameLocation location in CommonHelper.GetLocations())
                this.ReloadQueue.Add(location);
        }

        /// <summary>The method invoked when the player warps to a new location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void PlayerEvents_Warped(object sender, EventArgsPlayerWarped e)
        {
            this.ResetOverlayIfShown();
        }

        /// <summary>The method invoked when a location is added or removed.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void LocationEvents_LocationsChanged(object sender, EventArgsLocationsChanged e)
        {
            if (!this.EnableAutomation)
                return;

            this.VerboseLog("Location list changed, reloading all machines.");

            try
            {
                this.MachineGroups.Clear();
                foreach (GameLocation location in CommonHelper.GetLocations())
                    this.ReloadQueue.Add(location);
            }
            catch (Exception ex)
            {
                this.HandleError(ex, "updating locations");
            }
        }

        /// <summary>The method invoked when an object is added or removed to a location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void LocationEvents_ObjectsChanged(object sender, EventArgsLocationObjectsChanged e)
        {
            if (!this.EnableAutomation)
                return;

            this.VerboseLog($"Object list changed in {e.Location.Name}, reloading machines in current location.");

            try
            {
                this.ReloadQueue.Add(e.Location);
            }
            catch (Exception ex)
            {
                this.HandleError(ex, "updating the current location");
            }
        }

        /// <summary>The method invoked when the in-game clock time changes.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void GameEvents_UpdateTick(object sender, EventArgs e)
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
                    foreach (GameLocation location in this.ReloadQueue)
                        this.ReloadMachinesIn(location);
                    this.ReloadQueue.Clear();

                    this.ResetOverlayIfShown();
                }

                // process machines
                foreach (MachineGroup group in this.GetAllMachineGroups())
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
        private void InputEvents_ButtonPressed(object sender, EventArgsInput e)
        {
            try
            {
                // toggle overlay
                if (Context.IsPlayerFree && this.Config.Controls.ToggleOverlay.Contains(e.Button))
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

        /****
        ** Methods
        ****/
        /// <summary>Get the machine groups in every location.</summary>
        private IEnumerable<MachineGroup> GetAllMachineGroups()
        {
            foreach (KeyValuePair<GameLocation, MachineGroup[]> group in this.MachineGroups)
            {
                foreach (MachineGroup machineGroup in group.Value)
                    yield return machineGroup;
            }
        }

        /// <summary>Reload the machines in a given location.</summary>
        /// <param name="location">The location whose machines to reload.</param>
        private void ReloadMachinesIn(GameLocation location)
        {
            this.VerboseLog($"Reloading machines in {location.Name}...");

            this.MachineGroups[location] = this.Factory.GetActiveMachinesGroups(location, this.Helper.Reflection).ToArray();
        }

        /// <summary>Log an error and warn the user.</summary>
        /// <param name="ex">The exception to handle.</param>
        /// <param name="verb">The verb describing where the error occurred (e.g. "looking that up").</param>
        private void HandleError(Exception ex, string verb)
        {
            this.Monitor.Log($"Something went wrong {verb}:\n{ex}", LogLevel.Error);
            CommonHelper.ShowErrorMessage($"Huh. Something went wrong {verb}. The error log has the technical details.");
        }

        /// <summary>Log a trace message if verbose logging is enabled.</summary>
        /// <param name="message">The message to log.</param>
        private void VerboseLog(string message)
        {
            if (this.Config.VerboseLogging)
                this.Monitor.Log(message, LogLevel.Trace);
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
            if (this.CurrentOverlay == null)
                this.CurrentOverlay = new OverlayMenu(this.Factory.GetMachineGroups(Game1.currentLocation, this.Helper.Reflection));
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
