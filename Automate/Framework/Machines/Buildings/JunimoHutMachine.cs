using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Objects;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Buildings
{
    /// <summary>A Junimo hut machine that accepts input and provides output.</summary>
    internal class JunimoHutMachine : BaseMachine<JunimoHut>
    {
        /*********
        ** Properties
        *********/
        /// <summary>The Junimo hut's output chest.</summary>
        private Chest Output => this.Machine.output.Value;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="hut">The underlying Junimo hut.</param>
        /// <param name="location">The location which contains the machine.</param>
        public JunimoHutMachine(JunimoHut hut, GameLocation location)
            : base(hut, location, BaseMachine.GetTileAreaFor(hut)) { }

        /// <summary>Get the machine's processing state.</summary>
        public override MachineState GetState()
        {
            if (this.Output.items.Any(item => item != null))
                return MachineState.Done;
            return MachineState.Processing;
        }

        /// <summary>Get the machine output.</summary>
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
            return false; // no input
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
