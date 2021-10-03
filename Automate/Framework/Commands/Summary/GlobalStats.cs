using System.Collections.Generic;
using System.Linq;

namespace Pathoschild.Stardew.Automate.Framework.Commands.Summary
{
    /// <summary>Metadata about the machine groups in every location of the game.</summary>
    internal class GlobalStats
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The metadata for machine groups in each location.</summary>
        public LocationStats[] Locations { get; }

        /// <summary>The number of machine groups in the world.</summary>
        public int MachineGroupCount => this.Locations.Sum(p => p.MachineGroups.Length);

        /// <summary>The number of automated machines in the world.</summary>
        public int MachineCount => this.Locations.Sum(p => p.TotalMachines);

        /// <summary>The number of automated containers in the world.</summary>
        public int ContainerCount => this.Locations.Sum(p => p.TotalContainers);


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machineGroups">The machine groups for which to build stats.</param>
        public GlobalStats(IEnumerable<IMachineGroup> machineGroups)
        {
            this.Locations = this.GetLocations(machineGroups).ToArray();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get metadata about the automated machines and containers by location.</summary>
        /// <param name="machineGroups">The machine groups for which to build stats.</param>
        private IEnumerable<LocationStats> GetLocations(IEnumerable<IMachineGroup> machineGroups)
        {
            var groupsByLocation = machineGroups
                .GroupBy(p => p.LocationKey)
                .OrderByDescending(p => p.Key == null) // Junimo group
                .ThenBy(p => p.Key)
                .ToArray();

            foreach (IGrouping<string, IMachineGroup> locationGroup in groupsByLocation)
            {
                bool isJunimoGroup = locationGroup.Key == null;
                string label = locationGroup.Key ?? "Machines connected to a Junimo chest";

                yield return new LocationStats(label, isJunimoGroup, locationGroup);
            }
        }
    }
}
