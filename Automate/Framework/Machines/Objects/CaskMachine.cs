using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Objects
{
    /// <summary>A cask that accepts input and provides output.</summary>
    internal class CaskMachine : GenericObjectMachine<Cask>
    {
        /*********
        ** Fields
        *********/
        /// <summary>The items which can be aged in a cask with their aging rates.</summary>
        private readonly IDictionary<int, float> AgingRates = new Dictionary<int, float>
        {
            [424] = 4, // cheese
            [426] = 4, // goat cheese
            [459] = 2, // mead
            [303] = 1.66f, // pale ale
            [346] = 2, // beer
            [348] = 1 // wine
        };


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
        public override ITrackedStack GetOutput()
        {
            Cask cask = this.Machine;

            SObject heldObject = cask.heldObject.Value;
            return new TrackedItem(heldObject.getOne(), this.Reset);
        }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool SetInput(IStorage input)
        {
            Cask cask = this.Machine;

            if (input.TryGetIngredient(match => match.Type == ItemType.Object && (match.Sample as SObject)?.Quality < 4 && this.AgingRates.ContainsKey(match.Sample.ParentSheetIndex), 1, out IConsumable consumable))
            {
                SObject ingredient = (SObject)consumable.Take();

                cask.heldObject.Value = ingredient;
                cask.agingRate.Value = this.AgingRates[ingredient.ParentSheetIndex];
                cask.daysToMature.Value = 56;
                cask.MinutesUntilReady = 999999;
                switch (ingredient.Quality)
                {
                    case SObject.medQuality:
                        cask.daysToMature.Value = 42;
                        break;
                    case SObject.highQuality:
                        cask.daysToMature.Value = 28;
                        break;
                    case SObject.bestQuality:
                        cask.daysToMature.Value = 0;
                        cask.MinutesUntilReady = 1;
                        break;
                }

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
    }
}
