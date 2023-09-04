using System;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Objects
{
    /// <summary>A cask that accepts input and provides output.</summary>
    /// <remarks>Derived from <see cref="Cask"/>.</remarks>
    internal class CaskMachine : GenericObjectMachine<Cask>
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        /// <param name="location">The location containing the machine.</param>
        /// <param name="tile">The tile covered by the machine.</param>
        public CaskMachine(Cask machine, GameLocation location, Vector2 tile)
            : base(machine, location, tile) { }

        /// <summary>Get the machine's processing state.</summary>
        public override MachineState GetState()
        {
            SObject heldObject = this.Machine.heldObject.Value;
            if (heldObject == null)
                return MachineState.Empty;

            return heldObject.Quality >= 4
                ? MachineState.Done
                : MachineState.Processing;
        }

        /// <summary>Get the output item.</summary>
        public override ITrackedStack? GetOutput()
        {
            Cask cask = this.Machine;

            return this.GetTracked(cask.heldObject.Value, this.Reset);
        }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool SetInput(IStorage input)
        {
            Cask cask = this.Machine;

            foreach (ITrackedStack consumable in input.GetItems().Where(match => match.Type == ItemRegistry.type_object && match.Sample.Quality < SObject.bestQuality))
            {
                float agingRate = this.GetAgingMultiplierForItem(consumable.Sample);
                if (agingRate <= 0)
                    continue;

                SObject ingredient = (SObject)consumable.Take(1)!;

                cask.heldObject.Value = ingredient;
                cask.MinutesUntilReady = 999999;
                cask.agingRate.Value = agingRate;
                cask.daysToMature.Value = cask.GetDaysForQuality(ingredient.Quality);

                return true;
            }

            return false;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Reset the machine so it's ready to accept a new input.</summary>
        /// <param name="item">The output item that was taken.</param>
        private void Reset(Item item)
        {
            Cask cask = this.Machine;

            this.GenericReset(item);
            cask.MinutesUntilReady = 0;
            cask.agingRate.Value = 0;
            cask.daysToMature.Value = 0;
        }

        /// <summary>Get the aging rate multiplier for an item.</summary>
        /// <param name="item">The item instance.</param>
        [Obsolete("This is copied from Stardew Valley 1.5.6 for the initial 1.6 compatibility update. This should be replaced with a <c>Data/Machines</c>-based machine.")]
        private float GetAgingMultiplierForItem(Item? item)
        {
            return item?.QualifiedItemId switch
            {
                "(O)426" => 4, // goat cheese
                "(O)424" => 4, // cheese
                "(O)348" => 1, // wine
                "(O)459" => 2, // mead
                "(O)303" => 1.66f, // pale ale
                "(O)346" => 2, // beer
                _ => 0
            };
        }
    }
}
