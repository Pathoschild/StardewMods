using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.TerrainFeatures
{
    /// <summary>A tea bush machine that provides output.</summary>
    /// <remarks>Derived from <see cref="Bush.shake"/>.</remarks>
    internal class TeaBushMachine : BaseMachine<Bush>
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="bush">The underlying bush.</param>
        /// <param name="location">The machine's in-game location.</param>
        /// <param name="tile">The bush's tile position.</param>
        public TeaBushMachine(Bush bush, GameLocation location, Vector2 tile)
            : base(bush, location, GetTileAreaFor(tile)) { }

        /// <summary>Get the output item.</summary>
        public override ITrackedStack GetOutput()
        {
            return new TrackedItem(new SObject(815, 1), onReduced: this.OnOutputReduced);
        }

        /// <summary>Get the machine's processing state.</summary>
        public override MachineState GetState()
        {
            if (this.Machine.tileSheetOffset.Value == 1)
                return MachineState.Done;

            return this.Machine.inBloom(Game1.currentSeason, Game1.dayOfMonth)
                ? MachineState.Processing
                : MachineState.Disabled;
        }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool SetInput(IStorage input)
        {
            return false; // no input required
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Reset the machine so it's ready to accept a new input.</summary>
        /// <param name="item">The output item that was taken.</param>
        private void OnOutputReduced(Item item)
        {
            this.Machine.tileSheetOffset.Value = 0;
            this.Machine.setUpSourceRect();
        }
    }
}
