using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Automate.Framework;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Machines.Objects
{
    /// <summary>A loom that accepts input and provides output.</summary>
    internal class LoomMachine : GenericMachine
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        public LoomMachine(SObject machine)
            : base(machine) { }

        /// <summary>Get the output item.</summary>
        public override ITrackedStack GetOutput()
        {
            SObject machine = this.Machine;
            return new TrackedItem(machine.heldObject.heldObject, item =>
            {
                machine.heldObject = null;
                machine.readyForHarvest = false;
                machine.showNextIndex = false;
            });
        }

        /// <summary>Pull items from the connected pipes.</summary>
        /// <param name="pipes">The connected IO pipes.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool Pull(IPipe[] pipes)
        {
            SObject loom = this.Machine;

            // wool => cloth
            if (pipes.TryConsume(440, 1))
            {
                loom.heldObject = new SObject(Vector2.Zero, 428, null, false, true, false, false);
                loom.minutesUntilReady = 240;
                return true;
            }

            return false;
        }
    }
}
