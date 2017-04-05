using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>A generic machine instance.</summary>
    internal abstract class GenericMachine<TMachine> : IMachine where TMachine : SObject
    {
        /*********
        ** Properties
        *********/
        /// <summary>The underlying machine.</summary>
        protected TMachine Machine { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get the machine's processing state.</summary>
        public virtual MachineState GetState()
        {
            if (this.Machine.heldObject == null)
                return MachineState.Empty;

            return this.Machine.readyForHarvest
                ? MachineState.Done
                : MachineState.Processing;
        }

        /// <summary>Get the output item.</summary>
        /// <remarks>This should have no effect on the machine state, since the chests may not have room for the item.</remarks>
        public virtual Item GetOutput()
        {
            return this.Machine.heldObject;
        }

        /// <summary>Reset the machine so it's ready to accept a new input.</summary>
        /// <param name="outputTaken">Whether the current output was taken.</param>
        public virtual void Reset(bool outputTaken)
        {
            this.Machine.heldObject = null;
            this.Machine.readyForHarvest = false;
        }

        /// <summary>Pull items from the connected chests.</summary>
        /// <param name="chests">The connected chests.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public abstract bool Pull(Chest[] chests);


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        protected GenericMachine(TMachine machine)
        {
            this.Machine = machine;
        }
    }

    /// <summary>A generic machine instance.</summary>
    internal abstract class GenericMachine : GenericMachine<SObject>
    {
        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        protected GenericMachine(SObject machine)
            : base(machine) { }
    }
}
