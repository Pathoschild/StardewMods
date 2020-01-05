using StardewValley;
using StardewValley.Buildings;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Buildings
{
    /// <summary>A fish pond that accepts input and provides output.</summary>
    /// <remarks>Derived from <see cref="FishPond.doAction"/>.</remarks>
    internal class FishPondMachine : BaseMachine<FishPond>
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="pond">The underlying fish pond.</param>
        /// <param name="location">The location which contains the machine.</param>
        public FishPondMachine(FishPond pond, GameLocation location)
            : base(pond, location, BaseMachine.GetTileAreaFor(pond)) { }

        /// <summary>Get the machine's processing state.</summary>
        public override MachineState GetState()
        {
            if (this.Machine.isUnderConstruction())
                return MachineState.Disabled;

            return this.Machine.output.Value != null
                ? MachineState.Done
                : MachineState.Processing;
        }

        /// <summary>Get the machine output.</summary>
        public override ITrackedStack GetOutput()
        {
            return new TrackedItem(this.Machine.output.Value, onEmpty: this.OnOutputTaken);
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
            // clear output
            this.Machine.output.Value = null;

            // add fishing XP
            int addedExperience = item is SObject obj
                ? (int)(obj.sellToStorePrice() * (double)FishPond.HARVEST_OUTPUT_EXP_MULTIPLIER)
                : 0;
            Game1.player.gainExperience(Farmer.fishingSkill, addedExperience + FishPond.HARVEST_BASE_EXP);

        }
    }
}
