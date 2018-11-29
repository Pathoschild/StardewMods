using System.Collections.Generic;
using StardewValley;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>The API which lets other mods interact with Automate.</summary>
    internal class AutomateAPI : IAutomateAPI
    {
        /*********
        ** Properties
        *********/
        /// <summary>Constructs machine groups.</summary>
        private readonly MachineGroupFactory MachineGroupFactory;

        /// <summary>The machines to process.</summary>
        private readonly IDictionary<GameLocation, MachineGroup[]> MachineGroups;


        /*********
        ** Public methods
        *********/
        /// <summary>Add an automation factory.</summary>
        /// <param name="factory">An automation factory which construct machines, containers, and connectors.</param>
        public void AddFactory(IAutomationFactory factory)
        {
            this.MachineGroupFactory.Add(factory);
        }


        /*********
        ** Internal methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machineGroupFactory">Constructs machine groups.</param>
        /// <param name="machineGroups">A live view of the machine groups recognised by Automate.</param>
        internal AutomateAPI(MachineGroupFactory machineGroupFactory, IDictionary<GameLocation, MachineGroup[]> machineGroups)
        {
            this.MachineGroupFactory = machineGroupFactory;
            this.MachineGroups = machineGroups;
        }
    }
}
