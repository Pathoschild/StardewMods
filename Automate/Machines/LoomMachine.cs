using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Automate.Framework;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Machines
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

        /// <summary>Reset the machine so it's ready to accept a new input.</summary>
        /// <param name="outputTaken">Whether the current output was taken.</param>
        public override void Reset(bool outputTaken)
        {
            this.Machine.heldObject = null;
            this.Machine.readyForHarvest = false;
            this.Machine.showNextIndex = false;
        }

        /// <summary>Pull items from the connected chests.</summary>
        /// <param name="chests">The connected chests.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool Pull(Chest[] chests)
        {
            SObject loom = this.Machine;

            // wool => cloth
            if (chests.TryConsume(440, 1))
            {
                loom.heldObject = new SObject(Vector2.Zero, 428, null, false, true, false, false);
                loom.minutesUntilReady = 240;
                return true;
            }

            return false;
        }
    }
}
