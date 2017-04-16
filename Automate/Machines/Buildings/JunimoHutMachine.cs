using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Automate.Framework;
using StardewValley;
using StardewValley.Buildings;

namespace Pathoschild.Stardew.Automate.Machines.Buildings
{
    /// <summary>A Junimo hut machine that accepts input and provides output.</summary>
    internal class JunimoHutMachine : IMachine
    {
        /*********
        ** Properties
        *********/
        /// <summary>The underlying Junimo hut.</summary>
        private readonly JunimoHut Hut;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="hut">The underlying Junimo hut.</param>
        public JunimoHutMachine(JunimoHut hut)
        {
            this.Hut = hut;
        }

        /// <summary>Get the machine's processing state.</summary>
        public MachineState GetState()
        {
            if (this.Hut.output.items.Any())
                return MachineState.Done;
            return MachineState.Processing;
        }

        /// <summary>Get the output item.</summary>
        public ITrackedStack GetOutput()
        {
            List<Item> inventory = this.Hut.output.items;
            return new TrackedItem(inventory.FirstOrDefault(), onEmpty: item => inventory.Remove(item));
        }

        /// <summary>Pull items from the connected pipes.</summary>
        /// <param name="pipes">The connected IO pipes.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public bool Pull(IPipe[] pipes)
        {
            return false; // no input
        }
    }
}
