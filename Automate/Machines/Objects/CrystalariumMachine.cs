using Pathoschild.Stardew.Automate.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
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
        /// <remarks>This should have no effect on the machine state, since the chests may not have room for the item.</remarks>
        public override Item GetOutput()
        {
            return this.Machine.heldObject.getOne();
        }

        /// <summary>Reset the machine so it's ready to accept a new input.</summary>
        /// <param name="outputTaken">Whether the current output was taken.</param>
        public override void Reset(bool outputTaken)
        {
            SObject machine = this.Machine;

            machine.minutesUntilReady = this.Reflection.GetPrivateMethod(machine, "getMinutesForCrystalarium").Invoke<int>(machine.heldObject.parentSheetIndex);
            machine.readyForHarvest = false;
        }

        /// <summary>Pull items from the connected chests.</summary>
        /// <param name="chests">The connected chests.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool Pull(Chest[] chests)
        {
            return false; // started manually
        }
    }
}
