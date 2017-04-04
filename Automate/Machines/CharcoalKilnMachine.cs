using Pathoschild.Stardew.Automate.Framework;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Machines
{
    /// <summary>A charcoal kiln that accepts input and provides output.</summary>
    internal class CharcoalKilnMachine : GenericMachine
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        public CharcoalKilnMachine(SObject machine)
            : base(machine) { }

        /// <summary>Pull items from the connected chests.</summary>
        /// <param name="chests">The connected chests.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool Pull(Chest[] chests)
        {
            // wood => coal
            if (chests.TryConsume(388, 10))
            {
                this.Machine.heldObject = new SObject(382, 1);
                this.Machine.minutesUntilReady = 30;
                this.Machine.showNextIndex = true;
                return true;
            }
            return false;
        }
    }
}
