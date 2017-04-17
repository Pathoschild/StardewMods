using System;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Automate.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Machines.Objects
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

        /// <summary>Pull items from the connected pipes.</summary>
        /// <param name="pipes">The connected IO pipes.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool Pull(IPipe[] pipes)
        {
            SObject machine = this.Machine;
            Random random = new Random((int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed + Game1.timeOfDay + (int)machine.tileLocation.X * 200 + (int)machine.tileLocation.Y);

            // trash => coal/iron ore/stone
            if (pipes.TryConsume(168, 1))
            {
                machine.heldObject = new SObject(random.NextDouble() < 0.3 ? 382 : (random.NextDouble() < 0.3 ? 380 : 390), random.Next(1, 4));
                machine.minutesUntilReady = 60;
                Game1.stats.PiecesOfTrashRecycled += 1;
                return true;
            }

            // driftwood => coal/wood
            if (pipes.TryConsume(169, 1))
            {
                machine.heldObject = new SObject(random.NextDouble() < 0.25 ? 382 : 388, random.Next(1, 4));
                machine.minutesUntilReady = 60;
                Game1.stats.PiecesOfTrashRecycled += 1;
                return true;
            }

            // broken glasses or broken CD => refined quartz
            if (pipes.TryConsume(170, 1) || pipes.TryConsume(171, 1))
            {
                machine.heldObject = new SObject(338, 1);
                machine.minutesUntilReady = 60;
                Game1.stats.PiecesOfTrashRecycled += 1;
                return true;
            }

            // soggy newspaper => cloth/torch
            if (pipes.TryConsume(172, 1))
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
