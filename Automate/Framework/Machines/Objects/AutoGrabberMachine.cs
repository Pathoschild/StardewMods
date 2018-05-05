using System.Linq;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Objects
{
    /// <summary>An auto-grabber that provides output.</summary>
    /// <remarks>See the game's default logic in <see cref="SObject.DayUpdate"/> and <see cref="SObject.checkForAction"/>.</remarks>
    internal class AutoGrabberMachine : GenericMachine<SObject>
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        public AutoGrabberMachine(SObject machine)
            : base(machine) { }

        /// <summary>Get the machine's processing state.</summary>
        public override MachineState GetState()
        {
            return this.Machine.heldObject.Value is Chest output && output.items.Any()
                ? MachineState.Done
                : MachineState.Processing;
        }

        /// <summary>Get the output item.</summary>
        public override ITrackedStack GetOutput()
        {
            Chest output = (Chest)this.Machine.heldObject.Value;
            Item next = output.items.First();
            return new TrackedItem(next, onEmpty: item => output.items.Remove(item));
        }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool SetInput(IStorage input)
        {
            return false;
        }
    }
}
