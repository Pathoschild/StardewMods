using System;
using System.Collections.Generic;
using Pathoschild.Stardew.Automate.Framework;
using StardewValley;
using StardewValley.Objects;
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

        /// <summary>Pull items from the connected chests.</summary>
        /// <param name="chests">The connected chests.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool Pull(Chest[] chests)
        {
            SObject machine = this.Machine;

            // crop => seeds
            if (chests.TryGetIngredient(this.IsValidCrop, 1, out Requirement crop))
            {
                crop.Consume();
                int seedID = SeedMakerMachine.CropSeedIDs[crop.GetOne().parentSheetIndex];

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
        public bool IsValidCrop(Item item)
        {
            return
                item.parentSheetIndex != 433 // seed maker doesn't allow coffee beans
                && SeedMakerMachine.CropSeedIDs.ContainsKey(item.parentSheetIndex);
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
                lookup.Add(dataCropID, dataSeedID);
            }

            return lookup;
        }
    }
}
