using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>An aggregate collection of machine groups linked by Junimo chests.</summary>
    internal class JunimoMachineGroup : MachineGroup
    {
        /*********
        ** Fields
        *********/
        /// <summary>Sort machines by priority.</summary>
        private readonly Func<IEnumerable<IMachine>, IEnumerable<IMachine>> SortMachines;

        /// <summary>The underlying machine groups.</summary>
        private readonly List<IMachineGroup> MachineGroups = new();

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
        public JunimoMachineGroup(Func<IEnumerable<IMachine>, IEnumerable<IMachine>> sortMachines, Func<IContainer[], StorageManager> buildStorage)
            : base(
                locationKey: null,
                machines: Array.Empty<IMachine>(),
                containers: Array.Empty<IContainer>(),
                tiles: Array.Empty<Vector2>(),
                buildStorage: buildStorage
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

            this.StorageManager.SetContainers(Array.Empty<IContainer>());

            this.Containers = Array.Empty<IContainer>();
            this.Machines = Array.Empty<IMachine>();
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
            this.StorageManager.SetContainers(this.GetUniqueContainers());

            int junimoChests = 0;
            this.Containers = this.MachineGroups.SelectMany(p => p.Containers).Where(p => !p.IsJunimoChest || ++junimoChests == 1).ToArray();
            this.Machines = this.SortMachines(this.MachineGroups.SelectMany(p => p.Machines)).ToArray();
            this.Tiles = null;
        }

        /// <inheritdoc />
        public override IReadOnlySet<Vector2> GetTiles(string locationKey)
        {
            this.Tiles ??= this.BuildTileMap();

            return this.Tiles.TryGetValue(locationKey, out IReadOnlySet<Vector2>? tiles)
                ? tiles
                : ImmutableHashSet<Vector2>.Empty;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the unique containers from the underlying machine groups.</summary>
        private IEnumerable<IContainer> GetUniqueContainers()
        {
            int junimoChests = 0;
            foreach (IContainer container in this.MachineGroups.SelectMany(p => p.Containers))
            {
                if (!container.IsJunimoChest || ++junimoChests == 1)
                    yield return container;
            }
        }

        /// <summary>Build a map of covered tiles by location key.</summary>
        private Dictionary<string, IReadOnlySet<Vector2>> BuildTileMap()
        {
            Dictionary<string, IReadOnlySet<Vector2>> tiles = new();

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
}
