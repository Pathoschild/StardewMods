using System.Linq;
using Microsoft.Xna.Framework;

namespace Pathoschild.Stardew.Automate.Framework.Commands.Summary
{
    /// <summary>Metadata about a machine group in a location.</summary>
    internal class GroupStats
    {
        /*********
        ** Accessors
        *********/
        /// <summary>A human-readable name for the group.</summary>
        public string Name { get; }

        /// <summary>The machine types in the group.</summary>
        public GroupMachineStats[] Machines { get; }

        /// <summary>The container types in the group.</summary>
        public GroupContainerStats[] Containers { get; }

        /// <summary>Whether this group represents all the chests connected to a Junimo chest.</summary>
        public bool IsJunimoGroup { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machineGroup">The machine group to analyze.</param>
        public GroupStats(IMachineGroup machineGroup)
        {
            if (machineGroup.IsJunimoGroup)
                this.Name = "Distributed group";
            else
            {
                Vector2 tile = machineGroup.Tiles.First();
                this.Name = $"Group at ({tile.X}, {tile.Y})";
            }

            this.IsJunimoGroup = machineGroup.IsJunimoGroup;

            this.Machines = machineGroup.Machines
                .GroupBy(p => p.MachineTypeID)
                .Select(p => new GroupMachineStats(p.Key, p))
                .ToArray();

            this.Containers = machineGroup.Containers
                .GroupBy(p => p.Name)
                .Select(p => new GroupContainerStats(p.Key, p))
                .ToArray();
        }
    }
}
