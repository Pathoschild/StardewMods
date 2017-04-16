using Pathoschild.Stardew.Automate.Framework;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Machines.Objects
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

        /// <summary>Pull items from the connected pipes.</summary>
        /// <param name="pipes">The connected IO pipes.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool Pull(IPipe[] pipes)
        {
            // wood => coal
            if (pipes.TryConsume(388, 10))
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
