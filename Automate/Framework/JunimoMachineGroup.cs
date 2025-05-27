using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;

namespace Pathoschild.Stardew.Automate.Framework;

/// <summary>An aggregate collection of machine groups linked by Junimo chests.</summary>
internal class JunimoMachineGroup : MachineGroup
{
    /*********
    ** Fields
    *********/
    /// <summary>Sort machines by priority.</summary>
    private readonly Func<IEnumerable<IMachine>, IEnumerable<IMachine>> SortMachines;

    /// <summary>The underlying machine groups.</summary>
    private readonly List<IMachineGroup> MachineGroups = [];

    /// <summary>A map of covered tiles by location key, if loaded.</summary>
    private Dictionary<string, IReadOnlySet<Vector2>>? Tiles;


    /*********
    ** Accessors
    *********/
    /// <inheritdoc />
    public override bool HasInternalAutomation => this.Machines.Length > 0;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="sortMachines">Sort machines by priority.</param>
    /// <param name="buildStorage">Build a storage manager for the given containers.</param>
    /// <param name="monitor">Encapsulates monitoring and logging.</param>
    public JunimoMachineGroup(Func<IEnumerable<IMachine>, IEnumerable<IMachine>> sortMachines, Func<IContainer[], StorageManager> buildStorage, IMonitor monitor)
        : base(
            locationKey: null,
            machines: [],
            containers: [],
            tiles: [],
            buildStorage: buildStorage,
            monitor: monitor
        )
    {
        this.IsJunimoGroup = true;
        this.SortMachines = sortMachines;
    }

    /// <summary>Get the underlying machine groups.</summary>
    public IEnumerable<IMachineGroup> GetAll()
    {
        return this.MachineGroups;
    }

    /// <summary>Add machine groups to the collection.</summary>
    /// <param name="groups">The groups to add.</param>
    /// <remarks>Make sure to call <see cref="Rebuild"/> after making changes.</remarks>
    public void Add(IList<IMachineGroup> groups)
    {
        this.MachineGroups.AddRange(groups);
    }

    /// <summary>Remove all machine groups in the collection.</summary>
    public void Clear()
    {
        this.MachineGroups.Clear();

        this.StorageManager.SetContainers([]);

        this.Containers = [];
        this.Machines = [];
        this.Tiles = null;
    }

    /// <summary>Remove all machine groups within the given locations.</summary>
    /// <param name="locationKeys">The location keys as formatted by <see cref="MachineGroupFactory.GetLocationKey"/>.</param>
    public bool RemoveLocations(ISet<string> locationKeys)
    {
        return this.MachineGroups.RemoveAll(
            group => locationKeys.Contains(group.LocationKey!)
        ) > 0;
    }

    /// <summary>Rebuild the aggregate group for changes to the underlying machine groups.</summary>
    public void Rebuild()
    {
        this.Containers = this.GetUniqueContainers(this.MachineGroups.SelectMany(p => p.Containers));
        this.Machines = this.SortMachines(this.MachineGroups.SelectMany(p => p.Machines)).ToArray();
        this.Tiles = null;

        this.StorageManager.SetContainers(this.Containers);
    }

    /// <inheritdoc />
    public override IReadOnlySet<Vector2> GetTiles(string locationKey)
    {
        this.Tiles ??= this.BuildTileMap();

        return this.Tiles.TryGetValue(locationKey, out IReadOnlySet<Vector2>? tiles)
            ? tiles
            : ImmutableHashSet<Vector2>.Empty;
    }

    /// <summary>Get whether the tile area intersects this machine group.</summary>
    /// <param name="locationKey">The location key as formatted by <see cref="MachineGroupFactory.GetLocationKey"/>.</param>
    /// <param name="tileArea">The tile area to check.</param>
    /// <remarks>This is the Junimo Chest equivalent of <see cref="MachineDataForLocation.IntersectsAutomatedGroup"/>.</remarks>
    public bool IntersectsAutomatedGroup(string locationKey, Rectangle tileArea)
    {
        IReadOnlySet<Vector2> tiles = this.GetTiles(locationKey);
        if (tiles.Count == 0)
            return false;

        foreach (Vector2 tile in tileArea.GetTiles())
        {
            if (tiles.Contains(tile))
                return true;
        }

        return false;
    }

    /// <summary>Get whether a tile area contains or is adjacent to a tracked automateable.</summary>
    /// <param name="locationKey">The location key as formatted by <see cref="MachineGroupFactory.GetLocationKey"/>.</param>
    /// <param name="tileArea">The tile area to check.</param>
    /// <remarks>This is the Junimo Chest equivalent of <see cref="MachineDataForLocation.ContainsOrAdjacent"/>.</remarks>
    public bool ContainsOrAdjacent(string locationKey, Rectangle tileArea)
    {
        IReadOnlySet<Vector2> tiles = this.GetTiles(locationKey);
        if (tiles.Count == 0)
            return false;

        foreach (Vector2 tile in tileArea.GetSurroundingTiles())
        {
            if (tiles.Contains(tile))
                return true;
        }

        return false;
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Build a map of covered tiles by location key.</summary>
    private Dictionary<string, IReadOnlySet<Vector2>> BuildTileMap()
    {
        Dictionary<string, IReadOnlySet<Vector2>> tiles = [];

        foreach (IGrouping<string?, IMachineGroup> groupByLocation in this.MachineGroups.GroupBy(p => p.LocationKey))
        {
            string? locationKey = groupByLocation.Key;
            if (locationKey is null)
                continue; // ???

            tiles[locationKey] = new HashSet<Vector2>(groupByLocation.SelectMany(p => p.GetTiles(locationKey)));
        }

        return tiles;
    }
}
