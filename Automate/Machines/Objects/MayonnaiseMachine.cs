using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Automate.Framework;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Machines.Objects
{
    /// <summary>A mayonnaise that accepts input and provides output.</summary>
    internal class MayonnaiseMachine : GenericMachine
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        public MayonnaiseMachine(SObject machine)
            : base(machine) { }

        /// <summary>Pull items from the connected pipes.</summary>
        /// <param name="pipes">The connected IO pipes.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool Pull(IPipe[] pipes)
        {
            SObject machine = this.Machine;

            // void egg => void mayonnaise
            if (pipes.TryConsume(305, 1))
            {
                machine.heldObject = new SObject(Vector2.Zero, 308, null, false, true, false, false);
                machine.minutesUntilReady = 180;
                return true;
            }

            // duck egg => duck mayonnaise
            if (pipes.TryConsume(442, 1))
            {
                machine.heldObject = new SObject(Vector2.Zero, 307, null, false, true, false, false);
                machine.minutesUntilReady = 180;
                return true;
            }

            // white/brown egg => normal mayonnaise
            if (pipes.TryConsume(176, 1) || pipes.TryConsume(180, 1))
            {
                machine.heldObject = new SObject(Vector2.Zero, 306, null, false, true, false, false);
                machine.minutesUntilReady = 180;
                return true;
            }

            // dinosaur or large white/brown egg => gold-quality mayonnaise
            if (pipes.TryConsume(107, 1) || pipes.TryConsume(174, 1) || pipes.TryConsume(182, 1))
            {
                machine.heldObject = new SObject(Vector2.Zero, 306, null, false, true, false, false) { quality = SObject.highQuality };
                machine.minutesUntilReady = 180;
                return true;
            }

            return false;
        }
    }
}
