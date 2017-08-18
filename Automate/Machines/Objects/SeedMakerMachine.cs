using System;
using System.Collections.Generic;
using Pathoschild.Stardew.Automate.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Machines.Objects
{
    /// <summary>A seed maker that accepts input and provides output.</summary>
    internal class SeedMakerMachine : GenericMachine
    {
        /*********
        ** Properties
        *********/
        /// <summary>A crop ID => seed ID lookup.</summary>
        private static readonly IDictionary<int, int> CropSeedIDs = SeedMakerMachine.GetCropSeedIDs();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        public SeedMakerMachine(SObject machine)
            : base(machine) { }

        /// <summary>Pull items from the connected pipes.</summary>
        /// <param name="pipes">The connected IO pipes.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool Pull(IPipe[] pipes)
        {
            SObject machine = this.Machine;

            // crop => seeds
            if (pipes.TryGetIngredient(this.IsValidCrop, 1, out Consumable crop))
            {
                crop.Reduce();
                int seedID = SeedMakerMachine.CropSeedIDs[crop.Sample.parentSheetIndex];

                Random random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2 + (int)machine.tileLocation.X + (int)machine.tileLocation.Y * 77 + Game1.timeOfDay);
                machine.heldObject = new SObject(seedID, random.Next(1, 4));
                if (random.NextDouble() < 0.005)
                    machine.heldObject = new SObject(499, 1);
                else if (random.NextDouble() < 0.02)
                    machine.heldObject = new SObject(770, random.Next(1, 5));
                machine.minutesUntilReady = 20;
                return true;
            }

            return false;
        }


        /*********
        ** Public methods
        *********/
        /// <summary>Get whether a given item is a crop compatible with the seed marker.</summary>
        /// <param name="item">The item to check.</param>
        public bool IsValidCrop(ITrackedStack item)
        {
            return
                item.Sample.parentSheetIndex != 433 // seed maker doesn't allow coffee beans
                && SeedMakerMachine.CropSeedIDs.ContainsKey(item.Sample.parentSheetIndex);
        }

        /// <summary>Get a crop ID => seed ID lookup.</summary>
        public static IDictionary<int, int> GetCropSeedIDs()
        {
            IDictionary<int, int> lookup = new Dictionary<int, int>();

            IDictionary<int, string> cropData = Game1.content.Load<Dictionary<int, string>>("Data\\Crops");
            foreach (KeyValuePair<int, string> entry in cropData)
            {
                int dataCropID = Convert.ToInt32(entry.Value.Split('/')[3]);
                int dataSeedID = entry.Key;
                if (!lookup.ContainsKey(dataCropID)) // if multiple crops have the same seed, use the earliest one (which is more likely the vanilla seed)
                    lookup[dataCropID] = dataSeedID;
            }

            return lookup;
        }
    }
}
