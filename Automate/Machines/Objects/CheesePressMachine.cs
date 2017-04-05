using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Automate.Framework;
using StardewValley.Objects;
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

        /// <summary>Pull items from the connected chests.</summary>
        /// <param name="chests">The connected chests.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool Pull(Chest[] chests)
        {
            // goat milk => goat cheese
            if (chests.TryConsume(436, 1))
            {
                this.Machine.heldObject = new SObject(Vector2.Zero, 426, null, false, true, false, false);
                this.Machine.minutesUntilReady = 200;
                return true;
            }

            // large goat milk => gold-quality goat cheese
            if (chests.TryConsume(438, 1))
            {
                this.Machine.heldObject = new SObject(Vector2.Zero, 426, null, false, true, false, false) { quality = SObject.highQuality };
                this.Machine.minutesUntilReady = 200;
                return true;
            }

            // milk => cheese
            if (chests.TryConsume(184, 1))
            {
                this.Machine.heldObject = new SObject(Vector2.Zero, 424, null, false, true, false, false);
                this.Machine.minutesUntilReady = 200;
                return true;
            }

            // large milk => gold-quality cheese
            if (chests.TryConsume(186, 1))
            {
                this.Machine.heldObject = new SObject(Vector2.Zero, 424, "Cheese (=)", false, true, false, false) { quality = SObject.highQuality };
                this.Machine.minutesUntilReady = 200;
                return true;
            }

            return false;
        }
    }
}
