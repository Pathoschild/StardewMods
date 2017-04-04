using Pathoschild.Stardew.Automate.Framework;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Machines
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
        /// <remarks>This should have no effect on the machine state, since the chests may not have room for the item.</remarks>
        public override Item GetOutput()
        {
            return this.Machine.heldObject.getOne();
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
