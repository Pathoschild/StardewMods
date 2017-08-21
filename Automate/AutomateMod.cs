using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pathoschild.Stardew.Automate.Framework;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;

namespace Pathoschild.Stardew.Automate
{
    /// <summary>The mod entry point.</summary>
    public class AutomateMod : Mod
    {
        /*********
        ** Properties
        *********/
        /// <summary>The mod configuration.</summary>
        private ModConfig Config;

        /// <summary>Constructs machine instances.</summary>
        private readonly MachineFactory Factory = new MachineFactory();

        /// <summary>The machines to process.</summary>
        private readonly IDictionary<GameLocation, MachineMetadata[]> Machines = new Dictionary<GameLocation, MachineMetadata[]>();

        /// <summary>The locations that should be reloaded on the next update tick.</summary>
        private readonly HashSet<GameLocation> ReloadQueue = new HashSet<GameLocation>();

        /// <summary>The number of ticks until the next automation cycle.</summary>
        private int AutomateCountdown;


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
            LocationEvents.LocationsChanged += this.LocationEvents_LocationsChanged;
            LocationEvents.LocationObjectsChanged += this.LocationEvents_LocationObjectsChanged;
            GameEvents.UpdateTick += this.GameEvents_UpdateTick;

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
            // check for updates
            this.AutomateCountdown = this.Config.AutomationInterval;
            if (this.Config.CheckForUpdates)
                UpdateHelper.LogVersionCheckAsync(this.Monitor, this.ModManifest, "Automate");
        }

        /// <summary>The method invoked when a location is added or removed.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void LocationEvents_LocationsChanged(object sender, EventArgsGameLocationsChanged e)
        {
            this.VerboseLog("Location list changed, reloading all machines.");

            try
            {
                this.Machines.Clear();
                foreach (GameLocation location in e.NewLocations)
                {
                    // location
                    this.ReloadQueue.Add(location);

                    // buildings
                    if (location is BuildableGameLocation buildableLocation)
                    {
                        foreach (Building building in buildableLocation.buildings)
                        {
                            if (building.indoors != null)
                                this.ReloadQueue.Add(building.indoors);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.HandleError(ex, "updating locations");
            }
        }

        /// <summary>The method invoked when an object is added or removed to a location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void LocationEvents_LocationObjectsChanged(object sender, EventArgsLocationObjectsChanged e)
        {
            this.VerboseLog("Object list changed, reloading machines in current location.");

            try
            {
                this.ReloadQueue.Add(Game1.currentLocation);
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
            if (!Context.IsWorldReady)
                return;

            try
            {
                // handle delay
                this.AutomateCountdown--;
                if (this.AutomateCountdown > 0)
                    return;
                this.AutomateCountdown = this.Config.AutomationInterval;

                // reload machines if needed
                foreach (GameLocation location in this.ReloadQueue)
                    this.ReloadMachinesIn(location);
                this.ReloadQueue.Clear();

                // process machines
                this.ProcessMachines(this.GetAllMachines());
            }
            catch (Exception ex)
            {
                this.HandleError(ex, "processing machines");
            }
        }

        /****
        ** Methods
        ****/
        /// <summary>Get the machines in every location.</summary>
        private IEnumerable<MachineMetadata> GetAllMachines()
        {
            foreach (KeyValuePair<GameLocation, MachineMetadata[]> group in this.Machines)
            {
                foreach (MachineMetadata machine in group.Value)
                    yield return machine;
            }
        }

        /// <summary>Reload the machines in a given location.</summary>
        /// <param name="location">The location whose machines to reload.</param>
        private void ReloadMachinesIn(GameLocation location)
        {
            this.VerboseLog($"Reloading machines in {location.Name}...");

            this.Machines[location] = this.Factory.GetMachinesIn(location, this.Helper.Reflection).ToArray();
        }

        /// <summary>Process a set of machines.</summary>
        /// <param name="machines">The machines to process.</param>
        private void ProcessMachines(IEnumerable<MachineMetadata> machines)
        {
            machines = machines.ToArray();

            this.VerboseLog($"Automating {machines.Count()} machines...");
            foreach (MachineMetadata metadata in machines)
            {
                IMachine machine = metadata.Machine;
                string summary = $"Automating {metadata.Location.Name} > {machine.GetType().Name} ({metadata.Connected.Length} pipes)...";

                MachineState state = machine.GetState();
                switch (state)
                {
                    case MachineState.Empty:
                        bool pulled = machine.Pull(metadata.Connected);
                        summary += pulled ? " accepted new input." : " no input found.";
                        break;

                    case MachineState.Done:
                        bool pushed = metadata.Connected.TryPush(machine.GetOutput());
                        summary += pushed ? " pushed output." : " done, but no pipes can accept its output.";
                        break;

                    default:
                        summary += $" machine is {state.ToString().ToLower()}.";
                        break;
                }

                this.VerboseLog($"   {summary}");
            }
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
    }
}
