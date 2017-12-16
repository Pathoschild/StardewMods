using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using StardewValley;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>Handles logic for building a <see cref="MachineGroup"/>.</summary>
    internal class MachineGroupBuilder
    {
        /*********
        ** Properties
        *********/
        /// <summary>The location containing the group.</summary>
        private readonly GameLocation Location;

        /// <summary>The machines in the group.</summary>
        private readonly HashSet<IMachine> Machines = new HashSet<IMachine>();

        /// <summary>The containers in the group.</summary>
        private readonly HashSet<IContainer> Containers = new HashSet<IContainer>();

        /// <summary>The tiles comprising the group.</summary>
        private readonly HashSet<Vector2> Tiles = new HashSet<Vector2>();


        /*********
        ** Public methods
        *********/
        /// <summary>Create an instance.</summary>
        /// <param name="location">The location containing the group.</param>
        public MachineGroupBuilder(GameLocation location)
        {
            this.Location = location;
        }

        /// <summary>Add a machine to the group.</summary>
        /// <param name="machine">The machine to add.</param>
        public void Add(IMachine machine)
        {
            this.Machines.Add(machine);
        }

        /// <summary>Add a container to the group.</summary>
        /// <param name="container">The container to add.</param>
        public void Add(IContainer container)
        {
            this.Containers.Add(container);
        }

        /// <summary>Add tiles to the group.</summary>
        /// <param name="tileArea">The tile area occupied by the container.</param>
        public void Add(Rectangle tileArea)
        {
            foreach (Vector2 tile in tileArea.GetTiles())
                this.Tiles.Add(tile);
        }

        /// <summary>Get whether any tiles were added to the builder.</summary>
        public bool HasTiles()
        {
            return this.Tiles.Count > 0;
        }

        /// <summary>Create a group from the saved data.</summary>
        public MachineGroup Build()
        {
            return new MachineGroup(this.Location, this.Machines.ToArray(), this.Containers.ToArray(), this.Tiles.ToArray());
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
