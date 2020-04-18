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
    /// <remarks>See the game's default logic in <see cref="Mill.doAction"/>.</remarks>
    internal class MillMachine : BaseMachine<Mill>
    {
        /*********
        ** Fields
        *********/
        /// <summary>The mill's input chest.</summary>
        private Chest Input => this.Machine.input.Value;

        /// <summary>The mill's output chest.</summary>
        private Chest Output => this.Machine.output.Value;

        /// <summary>The maximum input stack size to allow per item ID, if different from <see cref="Item.maximumStackSize"/>.</summary>
        private readonly IDictionary<int, int> MaxInputStackSize;

        /// <summary>Minimum number of each item to keep.</summary>
        private readonly int KeepAtLeast;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="mill">The underlying mill.</param>
        /// <param name="location">The location which contains the machine.</param>
        /// <param name="keepAtLeast">Minimum number of each item to keep.</param>
        public MillMachine(Mill mill, GameLocation location, int keepAtLeast)
            : base(mill, location, BaseMachine.GetTileAreaFor(mill))
        {
            this.KeepAtLeast = keepAtLeast;
            this.MaxInputStackSize = new Dictionary<int, int>
            {
                [284] = new SObject(284, 1).maximumStackSize() / 3 // beet => 3 sugar (reduce stack to avoid overfilling output)
            };
        }

        /// <summary>Get the machine's processing state.</summary>
        public override MachineState GetState()
        {
            if (this.Machine.isUnderConstruction())
                return MachineState.Disabled;

            if (this.Output.items.Any(item => item != null))
                return MachineState.Done;

            return this.InputFull()
                ? MachineState.Processing
                : MachineState.Empty; // 'empty' insofar as it will accept more input, not necessarily empty
        }

        /// <summary>Get the output item.</summary>
        public override ITrackedStack GetOutput()
        {
            IList<Item> inventory = this.Output.items;
            return new TrackedItem(inventory.FirstOrDefault(item => item != null), onEmpty: this.OnOutputTaken);
        }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool SetInput(IStorage input)
        {
            IDictionary<int, int> itemCounter = new SortedList<int, int>();
            if (this.InputFull())
                return false;

            // fill input with wheat (262), beets (284), and rice (271)
            bool anyPulled = false;
            foreach (ITrackedStack stack in input.GetItemsAscendingQuality(itemCounter, i => i.Type == ItemType.Object && (i.Sample.ParentSheetIndex == 262 || i.Sample.ParentSheetIndex == 284 || i.Sample.ParentSheetIndex == 271)))
            {
                int itemCount=0;
                if (KeepAtLeast > 0 && !itemCounter.TryGetValue(stack.Sample.ParentSheetIndex, out itemCount))
                {
                    string error = $"Failed to retrieve item count #{stack.Sample.ParentSheetIndex} ('{stack.Sample.Name}')";
                    throw new InvalidOperationException(error);
                }

                // add item
                int nbAdded = this.TryAddInput(stack, Math.Min(stack.Count, itemCount-KeepAtLeast));
                if (nbAdded <= 0)
                    continue;
                if (KeepAtLeast > 0)
                  itemCounter[stack.Sample.ParentSheetIndex] -= nbAdded;
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
        /// <param name="maxCount">Max number to add.</param>
        /// <returns>Returns the number of items that were taken from the stack.</returns>
        private int TryAddInput(ITrackedStack item, int maxCount)
        {
            // nothing to add
            if (item.Count <= 0)
                return 0;

            // clean up input bin
            this.Input.clearNulls();

            // try adding to input
            int originalSize = item.Count;
            IList<Item> slots = this.Input.items;
            int maxStackSize = this.GetMaxInputStackSize(item.Sample);
            for (int i = 0; i < Chest.capacity; i++)
            {
                // done
                if (item.Count <= 0 || originalSize-item.Count >= maxCount)
                    break;

                // add to existing slot
                if (slots.Count > i)
                {
                    Item slot = slots[i];
                    if (item.Sample.canStackWith(slot) && slot.Stack < maxStackSize)
                    {
                        var sample = item.Sample.getOne();
                        sample.Stack = Math.Min(maxCount, maxStackSize - slot.Stack); // the most items we can add to the stack (in theory)
                        int actualAdded = sample.Stack - slot.addToStack(sample); // how many items were actually added to the stack
                        item.Reduce(actualAdded);
                    }
                    continue;
                }

                // add to new slot
                slots.Add(item.Take(Math.Min(maxCount, maxStackSize)));
            }

            return originalSize - item.Count;
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
                if (slot == null || slot.Stack < this.GetMaxInputStackSize(slot))
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

        /// <summary>Get the maximum input stack size to allow for an item.</summary>
        /// <param name="item">The input item to check.</param>
        private int GetMaxInputStackSize(Item item)
        {
            if (item == null)
                return 0;

            return this.MaxInputStackSize.TryGetValue(item.ParentSheetIndex, out int max)
                ? max
                : item.maximumStackSize();
        }
    }
}
