using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Automate.Framework;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Machines.Objects
{
    /// <summary>A cheese press that accepts input and provides output.</summary>
    internal class CheesePressMachine : GenericMachine
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        public CheesePressMachine(SObject machine)
            : base(machine) { }

        /// <summary>Pull items from the connected pipes.</summary>
        /// <param name="pipes">The connected IO pipes.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool Pull(IPipe[] pipes)
        {
            // goat milk => goat cheese
            if (pipes.TryConsume(436, 1))
            {
                this.Machine.heldObject = new SObject(Vector2.Zero, 426, null, false, true, false, false);
                this.Machine.minutesUntilReady = 200;
                return true;
            }

            // large goat milk => gold-quality goat cheese
            if (pipes.TryConsume(438, 1))
            {
                this.Machine.heldObject = new SObject(Vector2.Zero, 426, null, false, true, false, false) { quality = SObject.highQuality };
                this.Machine.minutesUntilReady = 200;
                return true;
            }

            // milk => cheese
            if (pipes.TryConsume(184, 1))
            {
                this.Machine.heldObject = new SObject(Vector2.Zero, 424, null, false, true, false, false);
                this.Machine.minutesUntilReady = 200;
                return true;
            }

            // large milk => gold-quality cheese
            if (pipes.TryConsume(186, 1))
            {
                this.Machine.heldObject = new SObject(Vector2.Zero, 424, "Cheese (=)", false, true, false, false) { quality = SObject.highQuality };
                this.Machine.minutesUntilReady = 200;
                return true;
            }

            return false;
        }
    }
}
