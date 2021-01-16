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
        private readonly ModConfig Config;

        /// <summary>The configuration for specific machines by ID.</summary>
        private readonly IDictionary<string, ModConfigMachine> PerMachineSettings;

        /// <summary>An aggregate collection of machine groups linked by Junimo chests.</summary>
        private readonly JunimoMachineGroup JunimoMachineGroup;

        /// <summary>The machines to process.</summary>
        private readonly List<IMachineGroup> ActiveMachineGroups = new();

        /// <summary>The disabled machine groups (e.g. machines not connected to a chest).</summary>
        private readonly List<IMachineGroup> DisabledMachineGroups = new();

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
        public MachineManager(ModConfig config, DataModel data, IAutomationFactory defaultFactory, IMonitor monitor)
        {
            this.Config = config;
            this.Monitor = monitor;

            this.Factory = new(this.GetMachineOverride);
            this.Factory.Add(defaultFactory);

            this.JunimoMachineGroup = new(this.Factory.SortMachines);

            this.PerMachineSettings = this.ParseMachineOverrides(config, data);
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
            return new Dictionary<string, ModConfigMachine>(this.PerMachineSettings);
        }

        /// <summary>Get the settings for a machine.</summary>
        /// <param name="id">The unique machine ID.</param>
        public ModConfigMachine GetMachineOverride(string id)
        {
            return this.PerMachineSettings.TryGetValue(id, out ModConfigMachine config)
                ? config
                : null;
        }

        /****
        ** State management
        ****/
        /// <summary>Clear all registered machines and add all locations to the reload queue.</summary>
        public void Reset()
        {
            this.ActiveMachineGroups.Clear();
            this.DisabledMachineGroups.Clear();

            this.JunimoMachineGroup.RemoveAll(_ => true);
            this.JunimoMachineGroup.Rebuild();

            this.ReloadQueue.AddMany(CommonHelper.GetLocations());
        }

        /// <summary>Queue a location for which to reload machines when <see cref="ReloadQueuedLocations"/> is called.</summary>
        /// <param name="location">The location to reload.</param>
        public void QueueReload(GameLocation location)
        {
            this.ReloadQueue.Add(location);
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
            if (this.ReloadQueue.Any())
            {
                this.ReloadMachinesIn(this.ReloadQueue);
                this.ReloadQueue.Clear();
                return true;
            }

            return false;
        }


        /*********
        ** Private methods
        *********/
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

        /// <summary>Parse per-machine overrides.</summary>
        /// <param name="config">The configured overrides.</param>
        /// <param name="data">The default overrides.</param>
        private IDictionary<string, ModConfigMachine> ParseMachineOverrides(ModConfig config, DataModel data)
        {
            var overrides = new Dictionary<string, ModConfigMachine>(data.DefaultMachineOverrides ?? new Dictionary<string, ModConfigMachine>(), StringComparer.OrdinalIgnoreCase);

            foreach (var pair in config.MachineOverrides)
                overrides[pair.Key] = pair.Value;

            return overrides;
        }
    }
}
