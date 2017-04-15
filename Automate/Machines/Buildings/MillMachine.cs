using System;
using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Automate.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Objects;
using Object = StardewValley.Object;

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

        /// <summary>The maximum stack size to allow for accepted items.</summary>
        private readonly IDictionary<int, int> MaxStackSize;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="mill">The underlying mill.</param>
        public MillMachine(Mill mill)
        {
            this.Mill = mill;
            this.MaxStackSize = new Dictionary<int, int>
            {
                [262] = new Object(262, 1).maximumStackSize(), // wheat => flour
                [284] = new Object(284, 1).maximumStackSize() / 3 // beet => 3 sugar (reduce stack to avoid overfilling output)
            };
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
                foreach (Item item in chest.items.Where(i => i.parentSheetIndex == 262 || i.parentSheetIndex == 284).ToArray())
                {
                    // add item
                    bool anyAdded = this.TryAddInput(item);
                    if (!anyAdded)
                        continue;
                    anyPulled = true;

                    // remove stack from chest if emptied
                    if (item.Stack <= 0)
                        chest.items.Remove(item);

                    // stop if full
                    if (this.InputFull())
                        return true;
                }
            }

            return anyPulled;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Try to add an item to the input queue, and adjust its stack size accordingly.</summary>
        /// <param name="item">The item stack to add.</param>
        /// <returns>Returns whether any items were taken from the stack.</returns>
        private bool TryAddInput(Item item)
        {
            // nothing to add
            if (item.Stack <= 0)
                return false;

            // clean up input bin
            this.Mill.input.clearNulls();

            // try adding to input
            int originalSize = item.Stack;
            List<Item> slots = this.Mill.input.items;
            int maxStackSize = this.MaxStackSize[item.parentSheetIndex];
            for (int i = 0; i < Chest.capacity; i++)
            {
                // done
                if (item.Stack <= 0)
                    break;

                // add to existing slot
                if (slots.Count > i)
                {
                    Item slot = slots[i];
                    if (item.canStackWith(slot) && slot.Stack < maxStackSize)
                    {
                        int maxToAdd = Math.Min(item.Stack, maxStackSize - slot.Stack); // the most items we can add to the stack (in theory)
                        int actualAdded = maxToAdd - slot.addToStack(maxToAdd); // how many items were actually added to the stack
                        item.Stack -= actualAdded;
                    }
                    continue;
                }

                // add to new slot
                Item newSlot = item.getOne();
                item.Stack = newSlot.addToStack(Math.Min(item.Stack - 1, maxStackSize));
                slots.Add(newSlot);
            }

            return item.Stack < originalSize;
        }

        /// <summary>Get whether the mill's input bin is full.</summary>
        private bool InputFull()
        {
            var slots = this.Mill.input.items;

            // free slots
            if (slots.Count < Chest.capacity)
                return false;

            // free space in stacks
            foreach (Item slot in slots)
            {
                if (slot == null || slot.Stack < this.MaxStackSize[slot.parentSheetIndex])
                    return false;
            }
            return true;
        }
    }
}
