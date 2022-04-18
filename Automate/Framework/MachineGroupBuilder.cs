#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>Handles logic for building an <see cref="IMachineGroup"/>.</summary>
    internal class MachineGroupBuilder
    {
        /*********
        ** Fields
        *********/
        /// <summary>The location containing the group, as formatted by <see cref="MachineGroupFactory.GetLocationKey"/>.</summary>
        private readonly string LocationKey;

        /// <summary>The machines in the group.</summary>
        private readonly HashSet<IMachine> Machines = new();

        /// <summary>The containers in the group.</summary>
        private readonly HashSet<IContainer> Containers = new();

        /// <summary>The tiles comprising the group.</summary>
        private readonly HashSet<Vector2> Tiles = new();

        /// <summary>Sort machines by priority.</summary>
        private readonly Func<IEnumerable<IMachine>, IEnumerable<IMachine>> SortMachines;

        /// <summary>Build a storage manager for the given containers.</summary>
        private readonly Func<IContainer[], StorageManager> BuildStorage;


        /*********
        ** Accessors
        *********/
        /// <summary>The tile areas added to the machine group since the queue was last cleared.</summary>
        internal IList<Rectangle> NewTileAreas { get; } = new List<Rectangle>();


        /*********
        ** Public methods
        *********/
        /// <summary>Create an instance.</summary>
        /// <param name="locationKey">The location containing the group, as formatted by <see cref="MachineGroupFactory.GetLocationKey"/>.</param>
        /// <param name="sortMachines">Sort machines by priority.</param>
        /// <param name="buildStorage">Build a storage manager for the given containers.</param>
        public MachineGroupBuilder(string locationKey, Func<IEnumerable<IMachine>, IEnumerable<IMachine>> sortMachines, Func<IContainer[], StorageManager> buildStorage)
        {
            this.LocationKey = locationKey;
            this.SortMachines = sortMachines;
            this.BuildStorage = buildStorage;
        }

        /// <summary>Add a machine to the group.</summary>
        /// <param name="machine">The machine to add.</param>
        public void Add(IMachine machine)
        {
            this.Machines.Add(machine);
            this.Add(machine.TileArea);
        }

        /// <summary>Add a container to the group.</summary>
        /// <param name="container">The container to add.</param>
        public void Add(IContainer container)
        {
            this.Containers.Add(container);
            this.Add(container.TileArea);
        }

        /// <summary>Add connector tiles to the group.</summary>
        /// <param name="tileArea">The tile area to add.</param>
        public void Add(Rectangle tileArea)
        {
            foreach (Vector2 tile in tileArea.GetTiles())
                this.Tiles.Add(tile);
            this.NewTileAreas.Add(tileArea);
        }

        /// <summary>Get whether any tiles were added to the builder.</summary>
        public bool HasTiles()
        {
            return this.Tiles.Count > 0;
        }

        /// <summary>Create a group from the saved data.</summary>
        public IMachineGroup Build()
        {
            var machines = this.SortMachines(this.Machines.Select(p => new MachineWrapper(p)));
            return new MachineGroup(this.LocationKey, machines, this.Containers, this.Tiles, this.BuildStorage);
        }

        /// <summary>Clear the saved data.</summary>
        public void Reset()
        {
            this.Machines.Clear();
            this.Containers.Clear();
            this.Tiles.Clear();
        }
    }
}
