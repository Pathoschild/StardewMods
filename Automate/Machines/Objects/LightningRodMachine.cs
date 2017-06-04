using Pathoschild.Stardew.Automate.Framework;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Machines.Objects
{
    /// <summary>A lightning rod that accepts input and provides output.</summary>
    internal class LightningRodMachine : GenericMachine
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        public LightningRodMachine(SObject machine)
            : base(machine) { }

        /// <summary>Get the output item.</summary>
        public override ITrackedStack GetOutput()
        {
            return new TrackedItem(this.Machine.heldObject.getOne(), onEmpty: this.GenericReset);
        }

        /// <summary>Pull items from the connected pipes.</summary>
        /// <param name="pipes">The connected IO pipes.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool Pull(IPipe[] pipes)
        {
            return false; // no input
        }
    }
}
