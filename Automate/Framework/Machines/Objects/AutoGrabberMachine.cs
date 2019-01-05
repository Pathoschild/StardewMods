using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Objects
{
    /// <summary>An auto-grabber that provides output.</summary>
    /// <remarks>See the game's default logic in <see cref="SObject.DayUpdate"/> and <see cref="SObject.checkForAction"/>.</remarks>
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
            return this.Machine.heldObject.Value is Chest output && output.items.Any(item => item != null)
                ? MachineState.Done
                : MachineState.Processing;
        }

        /// <summary>Get the output item.</summary>
        public override ITrackedStack GetOutput()
        {
            Item next = this.GetOutputChest().items.First(p => p != null);
            return new TrackedItem(next, onEmpty: this.OnOutputTaken);
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
            output.items.Remove(item);
        }
    }
}
