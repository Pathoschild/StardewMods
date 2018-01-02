using StardewValley;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Objects
{
    /// <summary>A tapper that accepts input and provides output.</summary>
    /// <remarks>See the game's machine logic in <see cref="SObject.performDropDownAction"/> and <see cref="SObject.checkForAction"/>.</remarks>
    internal class WormBinMachine : GenericMachine
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        public WormBinMachine(SObject machine)
            : base(machine) { }

        /// <summary>Get the output item.</summary>
        public override ITrackedStack GetOutput()
        {
            SObject bin = this.Machine;
            return new TrackedItem(bin.heldObject, item =>
            {
                bin.heldObject = new SObject(685, Game1.random.Next(2, 6));
                bin.minutesUntilReady = 2600 - Game1.timeOfDay;
                bin.readyForHarvest = false;
                bin.showNextIndex = false;
            });
        }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool SetInput(IStorage input)
        {
            return false; // no input
        }
    }
}
