using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.GameData.Crops;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Objects
{
    /// <summary>A seed maker that accepts input and provides output.</summary>
    /// <remarks>Derived from <see cref="SObject.performObjectDropInAction"/> (search for 'Seed Maker').</remarks>
    internal class SeedMakerMachine : GenericObjectMachine<SObject>
    {
        /*********
        ** Fields
        *********/
        /// <summary>A lookup which maps item IDs produced by crops to their seed ID.</summary>
        private static readonly IDictionary<string, string> SeedLookup = new Dictionary<string, string>();

        /// <summary>The number of defined crops when <see cref="SeedLookup"/> was last updated.</summary>
        private static int LastCropsCount = -1;

        /// <summary>The game tick when <see cref="SeedLookup"/> was last updated.</summary>
        private static int LastCacheTick = -1;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        /// <param name="location">The location containing the machine.</param>
        /// <param name="tile">The tile covered by the machine.</param>
        public SeedMakerMachine(SObject machine, GameLocation location, Vector2 tile)
            : base(machine, location, tile) { }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool SetInput(IStorage input)
        {
            this.UpdateSeedLookup();

            SObject machine = this.Machine;

            // crop => seeds
            if (input.TryGetIngredient(this.IsValidCrop, 1, out IConsumable? crop))
            {
                crop.Reduce();
                string seedID = SeedMakerMachine.SeedLookup[crop.Sample.ItemId];

                Random random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2 + (int)machine.TileLocation.X + (int)machine.TileLocation.Y * 77 + Game1.timeOfDay);

                if (random.NextDouble() < 0.005)
                    machine.heldObject.Value = ItemRegistry.Create<SObject>("(O)499");
                else if (random.NextDouble() < 0.02)
                    machine.heldObject.Value = ItemRegistry.Create<SObject>("(O)770", random.Next(1, 5));
                else
                    machine.heldObject.Value = ItemRegistry.Create<SObject>(seedID, random.Next(1, 4));

                machine.MinutesUntilReady = 20;
                return true;
            }

            return false;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get whether a given item is a crop compatible with the seed marker.</summary>
        /// <param name="item">The item to check.</param>
        private bool IsValidCrop(ITrackedStack item)
        {
            return
                item.Type == ItemRegistry.type_object
                && item.Sample.QualifiedItemId is not "(O)433"/* coffee beans */ or "(O)771" // fiber
                && SeedMakerMachine.SeedLookup.ContainsKey(item.Sample.ItemId);
        }

        /// <summary>Update the cached item => seed ID lookup.</summary>
        /// <remarks>Derived from <see cref="SObject.performObjectDropInAction"/> .</remarks>
        private void UpdateSeedLookup()
        {
            if (Game1.ticks > SeedMakerMachine.LastCacheTick)
            {
                var cache = SeedMakerMachine.SeedLookup;
                var crops = Game1.cropData;
                if (crops.Count != SeedMakerMachine.LastCropsCount)
                {
                    cache.Clear();

                    foreach ((string seedId, CropData data) in crops)
                    {
                        string produceId = data.HarvestItemId;
                        if (!cache.ContainsKey(produceId)) // use first crop found per game logic
                            cache[produceId] = seedId;
                    }
                }

                SeedMakerMachine.LastCacheTick = Game1.ticks;
                SeedMakerMachine.LastCropsCount = crops.Count;
            }
        }
    }
}
