using Pathoschild.Stardew.Automate.Framework;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Machines.Objects
{
    /// <summary>A statue of endless fortune that accepts input and provides output.</summary>
    internal class StatueOfEndlessFortuneMachine : GenericMachine
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        public StatueOfEndlessFortuneMachine(SObject machine)
            : base(machine) { }

        /// <summary>Get the machine's processing state.</summary>
        public override MachineState GetState()
        {
            return this.Machine.heldObject != null
                ? MachineState.Done
                : MachineState.Processing;
        }

        /// <summary>Pull items from the connected chests.</summary>
        /// <param name="chests">The connected chests.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool Pull(Chest[] chests)
        {
            return false; // no input
        }
    }
}
