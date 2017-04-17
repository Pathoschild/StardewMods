using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Automate.Framework;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Machines.Objects
{
    /// <summary>A furnace that accepts input and provides output.</summary>
    internal class FurnaceMachine : GenericMachine
    {
        /*********
        ** Properties
        *********/
        /// <summary>The machine's position in its location.</summary>
        private readonly Vector2 Tile;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        /// <param name="tile">The machine's position in its location.</param>
        public FurnaceMachine(SObject machine, Vector2 tile)
            : base(machine)
        {
            this.Tile = tile;
        }

        /// <summary>Pull items from the connected pipes.</summary>
        /// <param name="pipes">The connected IO pipes.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool Pull(IPipe[] pipes)
        {
            if (this.TryPull(pipes))
            {
                // update furnace sprite
                this.Machine.initializeLightSource(this.Tile);
                this.Machine.showNextIndex = true;
                return true;
            }
            return false;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Pull items from the connected pipes.</summary>
        /// <param name="pipes">The connected pipes.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        private bool TryPull(IPipe[] pipes)
        {
            SObject furnace = this.Machine;

            if (pipes.TryGetIngredient(SObject.coal, 1, out Requirement coal))
            {
                // iron bar
                if (pipes.TryConsume(SObject.iron, 5))
                {
                    coal.Reduce();
                    furnace.heldObject = new SObject(Vector2.Zero, SObject.ironBar, 1);
                    furnace.minutesUntilReady = 120;
                    return true;
                }

                // gold bar
                if (pipes.TryConsume(SObject.gold, 5))
                {
                    coal.Reduce();
                    furnace.heldObject = new SObject(Vector2.Zero, SObject.goldBar, 1);
                    furnace.minutesUntilReady = 300;
                    return true;
                }

                // iridium bar
                if (pipes.TryConsume(SObject.iridium, 5))
                {
                    coal.Reduce();
                    furnace.heldObject = new SObject(Vector2.Zero, SObject.iridiumBar, 1);
                    furnace.minutesUntilReady = 480;
                    return true;
                }

                // refined quartz
                if (pipes.TryConsume(SObject.quartzIndex, 1))
                {
                    coal.Reduce();
                    furnace.heldObject = new SObject(Vector2.Zero, 338, "Refined Quartz", false, true, false, false);
                    furnace.minutesUntilReady = 90;
                    return true;
                }

                // copper bar
                if (pipes.TryConsume(SObject.copper, 5))
                {
                    coal.Reduce();
                    furnace.heldObject = new SObject(Vector2.Zero, SObject.copperBar, 1);
                    furnace.minutesUntilReady = 30;
                    return true;
                }
            }

            return false;
        }
    }
}
