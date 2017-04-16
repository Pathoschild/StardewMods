using Pathoschild.Stardew.Automate.Framework;
using StardewModdingAPI;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Machines.Objects
{
    /// <summary>A crystalarium that accepts input and provides output.</summary>
    internal class CrystalariumMachine : GenericMachine
    {
        /*********
        ** Properties
        *********/
        /// <summary>Simplifies access to private game code.</summary>
        private readonly IReflectionHelper Reflection;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        /// <param name="reflection">Simplifies access to private game code.</param>
        public CrystalariumMachine(SObject machine, IReflectionHelper reflection)
            : base(machine)
        {
            this.Reflection = reflection;
        }

        /// <summary>Get the machine's processing state.</summary>
        public override MachineState GetState()
        {
            if (this.Machine.heldObject == null)
                return MachineState.Disabled;

            return this.Machine.readyForHarvest
                ? MachineState.Done
                : MachineState.Processing;
        }

        /// <summary>Get the output item.</summary>
        public override ITrackedStack GetOutput()
        {
            SObject machine = this.Machine;
            return new TrackedItem(machine.heldObject.getOne(), item =>
            {
                machine.minutesUntilReady = this.Reflection.GetPrivateMethod(machine, "getMinutesForCrystalarium").Invoke<int>(machine.heldObject.parentSheetIndex);
                machine.readyForHarvest = false;
            });
        }

        /// <summary>Pull items from the connected pipes.</summary>
        /// <param name="pipes">The connected IO pipes.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool Pull(IPipe[] pipes)
        {
            return false; // started manually
        }
    }
}
