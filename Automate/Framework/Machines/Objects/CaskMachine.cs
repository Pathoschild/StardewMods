using System.Collections.Generic;
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

            foreach (ITrackedStack consumable in input.GetItems().Where(match => match.Type == ItemType.Object && (match.Sample as SObject)?.Quality < SObject.bestQuality))
            {
                if (!this.AgingRates.TryGetValue(consumable.Sample.ParentSheetIndex, out float agingRate))
                    continue;

                SObject ingredient = (SObject)consumable.Take(1);

                cask.heldObject.Value = ingredient;
                cask.agingRate.Value = agingRate;
                cask.daysToMature.Value = ingredient.Quality switch
                {
                    SObject.medQuality => 42,
                    SObject.highQuality => 28,
                    SObject.bestQuality => 0,
                    _ => 56
                };
                cask.MinutesUntilReady = ingredient.Quality switch
                {
                    SObject.bestQuality => 1,
                    _ => 999999
                };

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
