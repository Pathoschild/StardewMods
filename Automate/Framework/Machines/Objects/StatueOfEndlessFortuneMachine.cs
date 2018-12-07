using StardewValley;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Objects
{
    /// <summary>A statue of endless fortune that accepts input and provides output.</summary>
    internal class StatueOfEndlessFortuneMachine : GenericObjectMachine<SObject>
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        /// <param name="location">The location containing the machine.</param>
        public StatueOfEndlessFortuneMachine(SObject machine, GameLocation location)
            : base(machine, location) { }

        /// <summary>Get the machine's processing state.</summary>
        public override MachineState GetState()
        {
            return this.Machine.heldObject.Value != null
                ? MachineState.Done
                : MachineState.Processing;
        }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool SetInput(IStorage input)
        {
            return false; // no input
        }
    }
}
