using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Buildings
{
    /// <summary>A mill machine that accepts input and provides output.</summary>
    internal class MillMachine : IMachine
    {
        /*********
        ** Properties
        *********/
        /// <summary>The underlying mill.</summary>
        private readonly Mill Mill;

        /// <summary>The mill's input chest.</summary>
        private Chest Input => this.Mill.input.Value;

        /// <summary>The mill's output chest.</summary>
        private Chest Output => this.Mill.output.Value;

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
                [262] = new SObject(262, 1).maximumStackSize(), // wheat => flour
                [284] = new SObject(284, 1).maximumStackSize() / 3 // beet => 3 sugar (reduce stack to avoid overfilling output)
            };
        }

        /// <summary>Get the machine's processing state.</summary>
        public MachineState GetState()
        {
            if (this.Output.items.Any(item => item != null))
                return MachineState.Done;
            return this.InputFull()
                ? MachineState.Processing
                : MachineState.Empty; // 'empty' insofar as it will accept more input, not necessarily empty
        }

        /// <summary>Get the output item.</summary>
        public ITrackedStack GetOutput()
        {
            IList<Item> inventory = this.Output.items;
            return new TrackedItem(inventory.FirstOrDefault(item => item != null), onEmpty: this.OnOutputTaken);
        }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public bool SetInput(IStorage input)
        {
            if (this.InputFull())
                return false;

            // fill input with wheat (262) and beets (284)
            bool anyPulled = false;
            foreach (ITrackedStack stack in input.GetItems().Where(i => i.Sample.ParentSheetIndex == 262 || i.Sample.ParentSheetIndex == 284))
            {
                // add item
                bool anyAdded = this.TryAddInput(stack);
                if (!anyAdded)
                    continue;
                anyPulled = true;

                // stop if full
                if (this.InputFull())
                    return true;
            }

            return anyPulled;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Try to add an item to the input queue, and adjust its stack size accordingly.</summary>
        /// <param name="item">The item stack to add.</param>
        /// <returns>Returns whether any items were taken from the stack.</returns>
        private bool TryAddInput(ITrackedStack item)
        {
            // nothing to add
            if (item.Count <= 0)
                return false;

            // clean up input bin
            this.Input.clearNulls();

            // try adding to input
            int originalSize = item.Count;
            IList<Item> slots = this.Input.items;
            int maxStackSize = this.MaxStackSize[item.Sample.ParentSheetIndex];
            for (int i = 0; i < Chest.capacity; i++)
            {
                // done
                if (item.Count <= 0)
                    break;

                // add to existing slot
                if (slots.Count > i)
                {
                    Item slot = slots[i];
                    if (item.Sample.canStackWith(slot) && slot.Stack < maxStackSize)
                    {
                        int maxToAdd = Math.Min(item.Count, maxStackSize - slot.Stack); // the most items we can add to the stack (in theory)
                        int actualAdded = maxToAdd - slot.addToStack(maxToAdd); // how many items were actually added to the stack
                        item.Reduce(actualAdded);
                    }
                    continue;
                }

                // add to new slot
                slots.Add(item.Take(Math.Min(item.Count, maxStackSize)));
            }

            return item.Count < originalSize;
        }

        /// <summary>Get whether the mill's input bin is full.</summary>
        private bool InputFull()
        {
            var slots = this.Input.items;

            // free slots
            if (slots.Count < Chest.capacity)
                return false;

            // free space in stacks
            foreach (Item slot in slots)
            {
                if (slot == null || slot.Stack < this.MaxStackSize[slot.ParentSheetIndex])
                    return false;
            }
            return true;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Remove an output item once it's been taken.</summary>
        /// <param name="item">The removed item.</param>
        private void OnOutputTaken(Item item)
        {
            this.Output.clearNulls();
            this.Output.items.Remove(item);
        }
    }
}
