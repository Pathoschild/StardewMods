using Pathoschild.Stardew.Automate.Framework;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Machines
{
    /// <summary>A tapper that accepts input and provides output.</summary>
    internal class WormBinMachine : GenericMachine
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        public WormBinMachine(SObject machine)
            : base(machine) { }

        /// <summary>Reset the machine so it's ready to accept a new input.</summary>
        /// <param name="outputTaken">Whether the current output was taken.</param>
        public override void Reset(bool outputTaken)
        {
            SObject bin = this.Machine;

            bin.heldObject = new SObject(685, Game1.random.Next(2, 6));
            bin.minutesUntilReady = 2600 - Game1.timeOfDay;
            //bin.readyForHarvest = false;
            //bin.showNextIndex = false;
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
