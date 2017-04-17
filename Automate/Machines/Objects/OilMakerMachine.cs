using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Automate.Framework;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Machines.Objects
{
    /// <summary>An oil maker that accepts input and provides output.</summary>
    internal class OilMakerMachine : GenericMachine
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        public OilMakerMachine(SObject machine)
            : base(machine) { }

        /// <summary>Pull items from the connected pipes.</summary>
        /// <param name="pipes">The connected IO pipes.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool Pull(IPipe[] pipes)
        {
            SObject machine = this.Machine;

            // truffle => truffle oil
            if (pipes.TryConsume(430, 1))
            {
                machine.heldObject = new SObject(Vector2.Zero, 432, null, false, true, false, false);
                machine.minutesUntilReady = 360;
                return true;
            }

            // sunflower seed => oil
            if (pipes.TryConsume(431, 1))
            {
                machine.heldObject = new SObject(247, 1);
                machine.minutesUntilReady = 3200;
                return true;
            }

            // corn => oil
            if (pipes.TryConsume(270, 1))
            {
                machine.heldObject = new SObject(Vector2.Zero, 247, null, false, true, false, false);
                machine.minutesUntilReady = 1000;
                return true;
            }

            // sunflower => oil
            if (pipes.TryConsume(421, 1))
            {
                machine.heldObject = new SObject(Vector2.Zero, 247, null, false, true, false, false);
                machine.minutesUntilReady = 60;
                return true;
            }

            return false;
        }
    }
}
