using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Objects
{
    /// <summary>An auto-grabber that provides output.</summary>
    /// <remarks>Derived from <see cref="SObject.DayUpdate"/> and <see cref="SObject.checkForAction"/> (search for 'case 165').</remarks>
    internal class AutoGrabberMachine : GenericObjectMachine<SObject>
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        /// <param name="location">The in-game location.</param>
        /// <param name="tile">The tile covered by the machine.</param>
        public AutoGrabberMachine(SObject machine, GameLocation location, Vector2 tile)
            : base(machine, location, tile) { }

        /// <summary>Get the machine's processing state.</summary>
        public override MachineState GetState()
        {
            return this.GetNextOutput() != null
                ? MachineState.Done
                : MachineState.Processing;
        }

        /// <summary>Get the output item.</summary>
        public override ITrackedStack? GetOutput()
        {
            return this.GetTracked(this.GetNextOutput(), onEmpty: this.OnOutputTaken);
        }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool SetInput(IStorage input)
        {
            return false;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the output chest.</summary>
        private Chest GetOutputChest()
        {
            return (Chest)this.Machine.heldObject.Value;
        }

        /// <summary>Remove an output item once it's been taken.</summary>
        /// <param name="item">The removed item.</param>
        private void OnOutputTaken(Item item)
        {
            Chest output = this.GetOutputChest();
            output.clearNulls();
            output.Items.Remove(item);
            this.Machine.showNextIndex.Value = !output.isEmpty();
        }

        /// <summary>Get the next output item.</summary>
        private Item? GetNextOutput()
        {
            foreach (Item item in this.GetOutputChest().Items)
            {
                if (item == null)
                    continue;

                return item;
            }

            return null;
        }
    }
}
