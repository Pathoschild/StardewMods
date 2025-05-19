using System;
using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Automate.Framework.Models;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;

namespace Pathoschild.Stardew.Automate.Framework;

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

    /// <summary>The machine data for each location.</summary>
    private readonly Dictionary<string, MachineDataForLocation> MachineData = new();

    /// <summary>The cached machines to process.</summary>
    private IMachineGroup[] ActiveMachineGroups = [];

    /// <summary>The cached disabled machine groups (e.g. machines not connected to a chest).</summary>
    private IMachineGroup[] DisabledMachineGroups = [];

    /// <summary>The locations that should be removed on the next update tick.</summary>
    private readonly HashSet<GameLocation> RemoveQueue = new(new GameLocationNameComparer());

    /// <summary>The locations that should be reloaded on the next update tick.</summary>
    private readonly HashSet<GameLocation> ReloadQueue = new(new GameLocationNameComparer());


    /*********
    ** Accessors
    *********/
    /// <summary>Constructs machine groups.</summary>
    public MachineGroupFactory Factory { get; }

    /// <summary>An aggregate collection of machine groups linked by Junimo chests.</summary>
    public List<GlobalMachineGroup> GlobalMachineGroups { get; } = new();


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

        this.Factory = new(this.GetMachineOverride, this.BuildStorage, monitor);
        this.Factory.Add(defaultFactory);

        //this.GlobalMachineGroups = new(this.Factory.SortMachines, this.BuildStorage, this.Monitor);
    }

    /****
    ** Machine search
    ****/
    /// <summary>Get the machine groups in every location.</summary>
    public IEnumerable<IMachineGroup> GetActiveMachineGroups()
    {
        foreach (IMachineGroup group in this.GlobalMachineGroups)
            if (group.HasInternalAutomation)
                yield return group;

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
            .Concat(this.GlobalMachineGroups.SelectMany(group => group.GetAll()))
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
        string locationKey = this.Factory.GetLocationKey(location);

        return this.MachineData.GetValueOrDefault(locationKey);
    }

    /****
    ** State management
    ****/
    /// <summary>Clear all registered machines.</summary>
    public void Clear()
    {
        this.MachineData.Clear();
        this.ActiveMachineGroups = [];
        this.DisabledMachineGroups = [];
        this.GlobalMachineGroups.Clear();
    }

    /// <summary>Clear all registered machines and add all locations to the reload queue.</summary>
    public void Reset()
    {
        this.Clear();

        foreach (GlobalMachineGroup group in this.GlobalMachineGroups)
            group.Rebuild();

        this.ReloadQueue.AddRange(CommonHelper.GetLocations());
    }

    /// <summary>Queue locations to remove and whose machines should be reloaded when <see cref="ReloadQueuedLocations"/> is called.</summary>
    /// <param name="locations">The locations to remove.</param>
    public void QueueRemove(IEnumerable<GameLocation> locations)
    {
        this.RemoveQueue.AddRange(locations);
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
        this.ReloadQueue.AddRange(locations);
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
        List<IMachineGroup> globalAdded = [];
        HashSet<IMachineGroup> globalChanged = [];
        bool anyChanged = false;

        // remove old groups
        {
            HashSet<string> locationKeys = [.. locations.Concat(removedLocations).Select(this.Factory.GetLocationKey)];
            if (this.Monitor.IsVerbose)
                this.Monitor.Log($"Reloading machines in {locationKeys.Count} locations: {string.Join(", ", locationKeys)}...");

            foreach (string locationKey in locationKeys)
                anyChanged |= this.MachineData.Remove(locationKey);

            foreach (GlobalMachineGroup globalGroup in this.GlobalMachineGroups)
            {
                if (globalGroup.RemoveLocations(locationKeys))
                {
                    anyChanged = true;
                    globalChanged.Add(globalGroup);
                }
            }
        }

        // add new groups
        foreach (GameLocation location in locations)
        {
            string locationKey = this.Factory.GetLocationKey(location);

            // collect new groups
            List<IMachineGroup> active = [];
            List<IMachineGroup> disabled = [];
            foreach (IMachineGroup group in this.Factory.GetMachineGroups(location, this.Monitor))
            {
                if (!group.HasInternalAutomation)
                    disabled.Add(group);

                else if (!group.IsGlobalGroup)
                    active.Add(group);

                else
                {
                    globalAdded.Add(group);
                    globalChanged.Add(group);
                }
            }

            // add groups
            this.MachineData[locationKey] = new MachineDataForLocation(locationKey, active, disabled);

            // track change
            anyChanged |= active.Any();
        }

        // rebuild caches
        if (anyChanged)
        {
            List<IMachineGroup> active = [];
            List<IMachineGroup> disabled = [];

            foreach (MachineDataForLocation locationData in this.MachineData.Values)
            {
                active.AddRange(locationData.ActiveMachineGroups);
                disabled.AddRange(locationData.DisabledMachineGroups);
            }

            this.ActiveMachineGroups = active.ToArray();
            this.DisabledMachineGroups = disabled.ToArray();
        }

        if (!globalChanged.Any())
            return;

        // determine distinct groups
        List<HashSet<string>> distinctGlobalGroups = [];
        foreach (HashSet<string> groupKeys in globalChanged.Select(p => p.GlobalContainerKeys))
        {
            HashSet<string>? existing = distinctGlobalGroups.FirstOrDefault(p => p.Overlaps(groupKeys));
            if (existing != null)
                existing.UnionWith(groupKeys);
            else
                distinctGlobalGroups.Add(groupKeys);
        }

        foreach (HashSet<string> groupKeys in distinctGlobalGroups)
        {
            GlobalMachineGroup? selectedGroup = null;
            int total = this.GlobalMachineGroups.Count;

            for (int i = 0; i < total; i++)
            {
                GlobalMachineGroup globalGroup = this.GlobalMachineGroups[i];
                if (!globalGroup.GlobalContainerKeys.Overlaps(groupKeys))
                    continue;

                selectedGroup ??= globalGroup;
                if (selectedGroup == globalGroup)
                    globalChanged.Add(selectedGroup);

                else
                {
                    selectedGroup.Add([.. globalGroup.GetAll()]);
                    this.GlobalMachineGroups.Remove(globalGroup);
                    total--;
                }
            }

            // create new group
            if (selectedGroup == null)
            {
                selectedGroup = new GlobalMachineGroup(this.Factory.SortMachines, this.BuildStorage, this.Monitor);
                this.GlobalMachineGroups.Add(selectedGroup);
            }

            // add groups to selected
            IList<IMachineGroup> groups = [.. globalAdded.Where(p => p.GlobalContainerKeys.Overlaps(groupKeys))];
            if (groups.Any())
                selectedGroup.Add(groups);
        }

        // rebuild groups
        foreach (GlobalMachineGroup globalGroup in globalChanged.OfType<GlobalMachineGroup>())
        {
            globalGroup.Rebuild();
        }
    }
}
