using System.Collections.Generic;
using Pathoschild.Stardew.Automate.Framework;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Machines.Objects
{
    // <summary>A cask that accepts input and provides output.</summary>
    internal class CaskMachine : GenericMachine<Cask>
    {
        /*********
        ** Properties
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
        public CaskMachine(Cask machine)
            : base(machine) { }

        /// <summary>Get the machine's processing state.</summary>
        public override MachineState GetState()
        {
            if (this.Machine.heldObject == null)
                return MachineState.Empty;

            return this.Machine.heldObject.quality >= 4
                ? MachineState.Done
                : MachineState.Processing;
        }

        /// <summary>Get the output item.</summary>
        public override ITrackedStack GetOutput()
        {
            Cask cask = this.Machine;
            return new TrackedItem(cask.heldObject.getOne(), item =>
            {
                cask.heldObject = null;
                cask.minutesUntilReady = 0;
                cask.readyForHarvest = false;
                cask.agingRate = 0;
                cask.daysToMature = 0;
            });
        }

        /// <summary>Pull items from the connected pipes.</summary>
        /// <param name="pipes">The connected IO pipes.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool Pull(IPipe[] pipes)
        {
            Cask cask = this.Machine;

            if (pipes.TryGetIngredient(match => (match.Sample as SObject)?.quality < 4 && this.AgingRates.ContainsKey(match.Sample.parentSheetIndex), 1, out Requirement consumable))
            {
                SObject input = (SObject)consumable.Take();

                cask.heldObject = input;
                cask.agingRate = this.AgingRates[input.parentSheetIndex];
                cask.daysToMature = 56;
                cask.minutesUntilReady = 999999;
                switch (input.quality)
                {
                    case SObject.medQuality:
                        cask.daysToMature = 42;
                        break;
                    case SObject.highQuality:
                        cask.daysToMature = 28;
                        break;
                    case SObject.bestQuality:
                        cask.daysToMature = 0;
                        cask.minutesUntilReady = 1;
                        break;
                }

                return true;
            }

            return false;
        }
    }
}
