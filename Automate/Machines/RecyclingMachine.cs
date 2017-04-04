using System;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Automate.Framework;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Machines
{
    /// <summary>A recycling maching that accepts input and provides output.</summary>
    internal class RecyclingMachine : GenericMachine
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        public RecyclingMachine(SObject machine)
            : base(machine) { }

        /// <summary>Pull items from the connected chests.</summary>
        /// <param name="chests">The connected chests.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool Pull(Chest[] chests)
        {
            SObject machine = this.Machine;
            Random random = new Random((int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed + Game1.timeOfDay + (int)machine.tileLocation.X * 200 + (int)machine.tileLocation.Y);

            // trash => coal/iron ore/stone
            if (chests.TryConsume(168, 1))
            {
                machine.heldObject = new SObject(random.NextDouble() < 0.3 ? 382 : (random.NextDouble() < 0.3 ? 380 : 390), random.Next(1, 4));
                machine.minutesUntilReady = 60;
                Game1.stats.PiecesOfTrashRecycled += 1;
                return true;
            }

            // driftwood => coal/wood
            if (chests.TryConsume(169, 1))
            {
                machine.heldObject = new SObject(random.NextDouble() < 0.25 ? 382 : 388, random.Next(1, 4));
                machine.minutesUntilReady = 60;
                Game1.stats.PiecesOfTrashRecycled += 1;
                return true;
            }

            // broken glasses or broken CD => refined quartz
            if (chests.TryConsume(170, 1) || chests.TryConsume(171, 1))
            {
                machine.heldObject = new SObject(338, 1);
                machine.minutesUntilReady = 60;
                Game1.stats.PiecesOfTrashRecycled += 1;
                return true;
            }

            // soggy newspaper => cloth/torch
            if (chests.TryConsume(172, 1))
            {
                machine.heldObject = random.NextDouble() < 0.1 ? new SObject(428, 1) : new Torch(Vector2.Zero, 3);
                machine.minutesUntilReady = 60;
                Game1.stats.PiecesOfTrashRecycled += 1;
                return true;
            }

            return false;
        }
    }
}
