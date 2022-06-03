using System;
using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Automate.Framework.Models;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewValley;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>Manages machine groups.</summary>
    internal class MachineManager
    {
        /*********
        ** Fields
        *********/
        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>The mod configuration.</summary>
        private readonly Func<ModConfig> Config;

        /// <summary>The internal mod data.</summary>
        private readonly DataModel Data;

        /// <summary>An aggregate collection of machine groups linked by Junimo chests.</summary>
        private readonly JunimoMachineGroup JunimoMachineGroup;

        /// <summary>The machine data for each location.</summary>
        private readonly Dictionary<GameLocation, MachineDataForLocation> MachineData = new();

        /// <summary>The cached machines to process.</summary>
        private readonly List<IMachineGroup> ActiveMachineGroups = new();

        /// <summary>The cached disabled machine groups (e.g. machines not connected to a chest).</summary>
        private readonly List<IMachineGroup> DisabledMachineGroups = new();

        /// <summary>The locations that should be removed on the next update tick.</summary>
        private readonly HashSet<GameLocation> RemoveQueue = new(new ObjectReferenceComparer<GameLocation>());

        /// <summary>The locations that should be reloaded on the next update tick.</summary>
        private readonly HashSet<GameLocation> ReloadQueue = new(new ObjectReferenceComparer<GameLocation>());


        /*********
        ** Accessors
        *********/
        /// <summary>Constructs machine groups.</summary>
        public MachineGroupFactory Factory { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="config">The mod configuration.</param>
        /// <param name="data">The internal mod data.</param>
        /// <param name="defaultFactory">The default automation factory to registry.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        public MachineManager(Func<ModConfig> config, DataModel data, IAutomationFactory defaultFactory, IMonitor monitor)
        {
            this.Config = config;
            this.Data = data;
            this.Monitor = monitor;

            this.Factory = new(this.GetMachineOverride, this.BuildStorage);
            this.Factory.Add(defaultFactory);

            this.JunimoMachineGroup = new(this.Factory.SortMachines, this.BuildStorage);
        }

        /****
        ** Machine search
        ****/
        /// <summary>Get the machine groups in every location.</summary>
        public IEnumerable<IMachineGroup> GetActiveMachineGroups()
        {
            if (this.JunimoMachineGroup.HasInternalAutomation)
                yield return this.JunimoMachineGroup;

            foreach (IMachineGroup group in this.ActiveMachineGroups)
                yield return group;
        }

        /// <summary>Get the active and disabled machine groups in a specific location for the API.</summary>
        /// <param name="location">The location whose machine groups to fetch.</param>
        public IEnumerable<IMachineGroup> GetForApi(GameLocation location)
        {
            string locationKey = this.Factory.GetLocationKey(location);

            return this
                .ActiveMachineGroups
                .Concat(this.DisabledMachineGroups)
                .Concat(this.JunimoMachineGroup.GetAll())
                .Where(p => p.LocationKey == locationKey);
        }

        /// <summary>Get the registered override settings.</summary>
        public IDictionary<string, ModConfigMachine> GetMachineOverrides()
        {
            ModConfig config = this.Config();

            Dictionary<string, ModConfigMachine> overrides = new(this.Data.DefaultMachineOverrides, StringComparer.OrdinalIgnoreCase);

            foreach ((string id, ModConfigMachine machineConfig) in config.MachineOverrides)
                overrides[id] = machineConfig;

            return overrides;
        }

        /// <summary>Get the settings for a machine.</summary>
        /// <param name="id">The unique machine ID.</param>
        public ModConfigMachine? GetMachineOverride(string id)
        {
            return this.Config().MachineOverrides.TryGetValue(id, out ModConfigMachine? config) || this.Data.DefaultMachineOverrides.TryGetValue(id, out config)
                ? config
                : null;
        }

        /****
        ** Machine state
        ****/
        /// <summary>Get the machine state for a location, if any.</summary>
        /// <param name="location">The location to check.</param>
        public MachineDataForLocation? GetMachineDataFor(GameLocation location)
        {
            return this.MachineData.TryGetValue(location, out MachineDataForLocation? data)
                ? data
                : null;
        }

        /****
        ** State management
        ****/
        /// <summary>Clear all registered machines.</summary>
        public void Clear()
        {
            this.MachineData.Clear();
            this.ActiveMachineGroups.Clear();
            this.DisabledMachineGroups.Clear();
            this.JunimoMachineGroup.RemoveAll(_ => true);
        }

        /// <summary>Clear all registered machines and add all locations to the reload queue.</summary>
        public void Reset()
        {
            this.Clear();

            this.JunimoMachineGroup.Rebuild();

            this.ReloadQueue.AddMany(CommonHelper.GetLocations());
        }

        /// <summary>Queue locations to remove and whose machines should be reloaded when <see cref="ReloadQueuedLocations"/> is called.</summary>
        /// <param name="locations">The locations to remove.</param>
        public void QueueRemove(IEnumerable<GameLocation> locations)
        {
            this.RemoveQueue.AddMany(locations);
        }

        /// <summary>Queue a location for which to reload machines when <see cref="ReloadQueuedLocations"/> is called.</summary>
        /// <param name="location">The location to reload.</param>
        public void QueueReload(GameLocation location)
        {
            this.ReloadQueue.Add(location);
        }

        /// <summary>Get whether a reload is already queued for a location.</summary>
        /// <param name="location">The location to reload.</param>
        public bool IsReloadQueued(GameLocation location)
        {
            return this.ReloadQueue.Contains(location);
        }

        /// <summary>Queue locations for which to reload machines when <see cref="ReloadQueuedLocations"/> is called.</summary>
        /// <param name="locations">The locations to reload.</param>
        public void QueueReload(IEnumerable<GameLocation> locations)
        {
            this.ReloadQueue.AddMany(locations);
        }

        /// <summary>Reload any locations queued for reload.</summary>
        /// <returns>Returns whether any locations were reloaded.</returns>
        public bool ReloadQueuedLocations()
        {
            if (this.ReloadQueue.Any() || this.RemoveQueue.Any())
            {
                this.ReloadMachinesIn(this.ReloadQueue, this.RemoveQueue);
                this.ReloadQueue.Clear();
                this.RemoveQueue.Clear();
                return true;
            }

            return false;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Build a storage manager for the given containers.</summary>
        /// <param name="containers">The storage containers.</param>
        private StorageManager BuildStorage(IContainer[] containers)
        {
            return new StorageManager(containers);
        }

        /// <summary>Reload the machines in a given location.</summary>
        /// <param name="locations">The locations whose machines to reload.</param>
        /// <param name="removedLocations">The locations which have been removed, and whose machines should be reloaded if they still exist.</param>
        private void ReloadMachinesIn(ISet<GameLocation> locations, ISet<GameLocation> removedLocations)
        {
            bool junimoGroupChanged;

            // remove old groups
            {
                HashSet<string> locationKeys = new(locations.Concat(removedLocations).Select(this.Factory.GetLocationKey));
                this.Monitor.VerboseLog($"Reloading machines in {locationKeys.Count} locations: {string.Join(", ", locationKeys)}...");

                this.MachineData.RemoveAll((_, data) => locationKeys.Contains(data.LocationKey));
                this.DisabledMachineGroups.RemoveAll(p => locationKeys.Contains(p.LocationKey!));
                this.ActiveMachineGroups.RemoveAll(p => locationKeys.Contains(p.LocationKey!));
                junimoGroupChanged = this.JunimoMachineGroup.RemoveAll(p => locationKeys.Contains(p.LocationKey!));
            }

            // add new groups
            foreach (GameLocation location in locations)
            {
                string locationKey = this.Factory.GetLocationKey(location);

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
                this.MachineData.Add(location, new MachineDataForLocation(locationKey, active, disabled));
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
    }
}
