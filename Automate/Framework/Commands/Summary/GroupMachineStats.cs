using System.Collections.Generic;
using System.Linq;

namespace Pathoschild.Stardew.Automate.Framework.Commands.Summary
{
    /// <summary>Metadata about machines of the same type within a machine group.</summary>
    internal class GroupMachineStats
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The machine type name.</summary>
        public string Name { get; }

        /// <summary>The number of machines in the group.</summary>
        public int Count { get; }

        /// <summary>The number of machines by state.</summary>
        public IDictionary<MachineState, int> States { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The machine type name.</param>
        /// <param name="machines">The machines in the group.</param>
        public GroupMachineStats(string name, IEnumerable<IMachine> machines)
        {
            this.Name = name;
            this.States = machines
                .GroupBy(p => p.GetState())
                .ToDictionary(p => p.Key, p => p.Count());
            this.Count = this.States.Values.Sum();
        }
    }
}
