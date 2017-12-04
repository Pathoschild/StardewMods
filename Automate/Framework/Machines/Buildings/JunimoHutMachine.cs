using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewValley.Buildings;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Buildings
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

        /// <summary>Get the machine output.</summary>
        public ITrackedStack GetOutput()
        {
            List<Item> inventory = this.Hut.output.items;
            return new TrackedItem(inventory.FirstOrDefault(), onEmpty: item => inventory.Remove(item));
        }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public bool SetInput(IStorage input)
        {
            return false; // no input
        }
    }
}
