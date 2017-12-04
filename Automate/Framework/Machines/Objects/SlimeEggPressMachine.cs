using StardewValley;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Objects
{
    /// <summary>A slime egg-press that accepts input and provides output.</summary>
    internal class SlimeEggPressMachine : GenericMachine
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        public SlimeEggPressMachine(SObject machine)
            : base(machine) { }

        /// <summary>Get the output item.</summary>
        public override ITrackedStack GetOutput()
        {
            return new TrackedItem(this.Machine.heldObject.getOne(), this.GenericReset);
        }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool SetInput(IStorage input)
        {
            // slime => slime egg
            if (input.TryConsume(766, 100))
            {
                int parentSheetIndex = 680;
                if (Game1.random.NextDouble() < 0.05)
                    parentSheetIndex = 439;
                else if (Game1.random.NextDouble() < 0.1)
                    parentSheetIndex = 437;
                else if (Game1.random.NextDouble() < 0.25)
                    parentSheetIndex = 413;
                this.Machine.heldObject = new SObject(parentSheetIndex, 1);
                this.Machine.minutesUntilReady = 1200;
                return true;
            }

            return false;
        }
    }
}
