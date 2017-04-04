using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Automate.Framework;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Machines
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

        /// <summary>Pull items from the connected chests.</summary>
        /// <param name="chests">The connected chests.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool Pull(Chest[] chests)
        {
            if (this.TryPull(chests))
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
        /// <summary>Pull items from the connected chests.</summary>
        /// <param name="chests">The connected chests.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        private bool TryPull(Chest[] chests)
        {
            SObject furnace = this.Machine;

            if (chests.TryGetIngredient(SObject.coal, 1, out Requirement coal))
            {
                // iron bar
                if (chests.TryConsume(SObject.iron, 5))
                {
                    coal.Consume();
                    furnace.heldObject = new SObject(Vector2.Zero, SObject.ironBar, 1);
                    furnace.minutesUntilReady = 120;
                    return true;
                }

                // gold bar
                if (chests.TryConsume(SObject.gold, 5))
                {
                    coal.Consume();
                    furnace.heldObject = new SObject(Vector2.Zero, SObject.goldBar, 1);
                    furnace.minutesUntilReady = 300;
                    return true;
                }

                // iridium bar
                if (chests.TryConsume(SObject.iridium, 5))
                {
                    coal.Consume();
                    furnace.heldObject = new SObject(Vector2.Zero, SObject.iridiumBar, 1);
                    furnace.minutesUntilReady = 480;
                    return true;
                }

                // refined quartz
                if (chests.TryConsume(SObject.quartzIndex, 1))
                {
                    coal.Consume();
                    furnace.heldObject = new SObject(Vector2.Zero, 338, "Refined Quartz", false, true, false, false);
                    furnace.minutesUntilReady = 90;
                    return true;
                }

                // copper bar
                if (chests.TryConsume(SObject.copper, 5))
                {
                    coal.Consume();
                    furnace.heldObject = new SObject(Vector2.Zero, SObject.copperBar, 1);
                    furnace.minutesUntilReady = 30;
                    return true;
                }
            }

            return false;
        }
    }
}
