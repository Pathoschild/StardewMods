using Pathoschild.Stardew.Automate.Framework;
using StardewValley;
using StardewValley.Objects;

namespace Pathoschild.Stardew.Automate.Machines.Objects
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
            return this.GetHaySpace(farm) > 0
                ? MachineState.Empty // 'empty' insofar as it will accept more input, not necessarily empty
                : MachineState.Disabled;
        }

        /// <summary>Get the output item.</summary>
        /// <remarks>This should have no effect on the machine state, since the chests may not have room for the item.</remarks>
        public Item GetOutput()
        {
            return null;
        }

        /// <summary>Reset the machine so it's ready to accept a new input.</summary>
        /// <param name="outputTaken">Whether the current output was taken.</param>
        public void Reset(bool outputTaken)
        {
            // not applicable
        }

        /// <summary>Pull items from the connected chests.</summary>
        /// <param name="chests">The connected chests.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public bool Pull(Chest[] chests)
        {
            Farm farm = Game1.getFarm();

            // skip if full
            if (this.GetHaySpace(farm) <= 0)
                return false;

            // try to add hay until full
            bool anyPulled = false;
            foreach (Chest chest in chests)
            {
                foreach (Item item in chest.items.ToArray())
                {
                    // stop if full
                    if (this.GetHaySpace(farm) <= 0)
                        return anyPulled;

                    // pull hay
                    if (item.parentSheetIndex == 178)
                    {
                        int added = item.Stack - farm.tryToAddHay(item.Stack);
                        if (added == 0)
                            return anyPulled;

                        item.Stack -= added;
                        if (item.Stack <= 0)
                            chest.items.Remove(item);
                        anyPulled = true;
                    }
                }
            }

            return anyPulled;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the amount of hay the hopper can still accept before it's full.</summary>
        /// <param name="farm">The farm to check.</param>
        /// <remarks>Derived from <see cref="Farm.tryToAddHay"/>.</remarks>
        private int GetHaySpace(Farm farm)
        {
            return Utility.numSilos() * 240 - farm.piecesOfHay;
        }
    }
}
