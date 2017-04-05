using System.Linq;
using Pathoschild.Stardew.Automate.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Objects;

namespace Pathoschild.Stardew.Automate.Machines.Buildings
{
    /// <summary>A mill machine that accepts input and provides output.</summary>
    internal class MillMachine : IMachine
    {
        /*********
        ** Properties
        *********/
        /// <summary>The underlying mill.</summary>
        private readonly Mill Mill;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="mill">The underlying mill.</param>
        public MillMachine(Mill mill)
        {
            this.Mill = mill;
        }

        /// <summary>Get the machine's processing state.</summary>
        public MachineState GetState()
        {
            if (this.Mill.output.items.Any())
                return MachineState.Done;
            return this.InputFull()
                ? MachineState.Processing
                : MachineState.Empty; // 'empty' insofar as it will accept more input, not necessarily empty
        }

        /// <summary>Get the output item.</summary>
        /// <remarks>This should have no effect on the machine state, since the chests may not have room for the item.</remarks>
        public Item GetOutput()
        {
            return this.Mill.output.items.FirstOrDefault();
        }

        /// <summary>Reset the machine so it's ready to accept a new input.</summary>
        /// <param name="outputTaken">Whether the current output was taken.</param>
        public void Reset(bool outputTaken)
        {
            if (this.Mill.output.items.Any())
                this.Mill.output.items.RemoveAt(0);
        }

        /// <summary>Pull items from the connected chests.</summary>
        /// <param name="chests">The connected chests.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public bool Pull(Chest[] chests)
        {
            if (this.InputFull())
                return false;

            // fill input with wheat (262) and beets (284)
            bool anyPulled = false;
            foreach (Chest chest in chests)
            {
                foreach (Item item in chest.items.ToArray())
                {
                    if (this.InputFull())
                        return anyPulled;

                    if (item.parentSheetIndex == 262 || item.parentSheetIndex == 284)
                    {
                        this.Mill.input.items.Add(item);
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
        /// <summary>Get whether the mill's input bin is full.</summary>
        private bool InputFull()
        {
            return this.Mill.input.items.Count >= Chest.capacity;
        }
    }
}
