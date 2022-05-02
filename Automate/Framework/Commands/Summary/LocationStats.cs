using System.Collections.Generic;
using System.Linq;

namespace Pathoschild.Stardew.Automate.Framework.Commands.Summary
{
    /// <summary>Metadata about the machine groups in a location.</summary>
    internal class LocationStats
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The location display name.</summary>
        public string Name { get; }

        /// <summary>Whether this group represents all the chests connected to a Junimo chest.</summary>
        public bool IsJunimoGroup { get; }

        /// <summary>The machine groups in the location.</summary>
        public GroupStats[] MachineGroups { get; }

        /// <summary>The number of automated machines in the location.</summary>
        public int TotalMachines { get; }

        /// <summary>The number of automated containers in the location.</summary>
        public int TotalContainers { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The location display name.</param>
        /// <param name="isJunimoGroup">Whether this group represents all the chests connected to a Junimo chest.</param>
        /// <param name="machineGroups">The machine groups in the location.</param>
        public LocationStats(string name, bool isJunimoGroup, IEnumerable<IMachineGroup> machineGroups)
        {
            this.Name = name;
            this.IsJunimoGroup = isJunimoGroup;
            this.MachineGroups = machineGroups.Select(p => new GroupStats(p)).ToArray();

            this.TotalMachines = this.MachineGroups.Sum(group => group.Machines.Length);
            this.TotalContainers = this.MachineGroups.Sum(group => group.Containers.Length);
        }
    }
}
