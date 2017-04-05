using System.Linq;
using Pathoschild.Stardew.Automate.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Objects;

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
        /// <remarks>This should have no effect on the machine state, since the chests may not have room for the item.</remarks>
        public Item GetOutput()
        {
            return this.Hut.output.items.FirstOrDefault();
        }

        /// <summary>Reset the machine so it's ready to accept a new input.</summary>
        /// <param name="outputTaken">Whether the current output was taken.</param>
        public void Reset(bool outputTaken)
        {
            if (this.Hut.output.items.Any())
                this.Hut.output.items.RemoveAt(0);
        }

        /// <summary>Pull items from the connected chests.</summary>
        /// <param name="chests">The connected chests.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public bool Pull(Chest[] chests)
        {
            return false; // no input
        }
    }
}
