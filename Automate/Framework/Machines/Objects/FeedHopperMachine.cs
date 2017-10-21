using System;
using System.Linq;
using StardewValley;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Objects
{
    /// <summary>A hay hopper that accepts input and provides output.</summary>
    internal class FeedHopperMachine : IMachine
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get the machine's processing state.</summary>
        public MachineState GetState()
        {
            Farm farm = Game1.getFarm();
            return this.GetFreeSpace(farm) > 0
                ? MachineState.Empty // 'empty' insofar as it will accept more input, not necessarily empty
                : MachineState.Disabled;
        }

        /// <summary>Get the output item.</summary>
        public ITrackedStack GetOutput()
        {
            return null;
        }

        /// <summary>Reset the machine so it's ready to accept a new input.</summary>
        /// <param name="outputTaken">Whether the current output was taken.</param>
        public void Reset(bool outputTaken)
        {
            // not applicable
        }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public bool SetInput(IStorage input)
        {
            Farm farm = Game1.getFarm();

            // skip if full
            if (this.GetFreeSpace(farm) <= 0)
                return false;

            // try to add hay (178) until full
            bool anyPulled = false;
            foreach (ITrackedStack stack in input.GetItems().Where(p => p.Sample.parentSheetIndex == 178))
            {
                // get free space
                int space = this.GetFreeSpace(farm);
                if (space <= 0)
                    return anyPulled;

                // pull hay
                int maxToAdd = Math.Min(stack.Count, space);
                int added = maxToAdd - farm.tryToAddHay(maxToAdd);
                stack.Reduce(added);
                if (added > 0)
                    anyPulled = true;
            }

            return anyPulled;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the amount of hay the hopper can still accept before it's full.</summary>
        /// <param name="farm">The farm to check.</param>
        /// <remarks>Derived from <see cref="Farm.tryToAddHay"/>.</remarks>
        private int GetFreeSpace(Farm farm)
        {
            return Utility.numSilos() * 240 - farm.piecesOfHay;
        }
    }
}
