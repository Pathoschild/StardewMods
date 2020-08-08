using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.TerrainFeatures
{
    /// <summary>A bush machine that provides output.</summary>
    /// <remarks>Derived from <see cref="Bush.shake"/>.</remarks>
    internal class BushMachine : BaseMachine<Bush>
    {
        /*********
        ** Fields
        *********/
        /// <summary>The season for which <see cref="BushMachine.InSeasonImpl"/> applies.</summary>
        private string LastSeason;

        /// <summary>The day of month for which <see cref="BushMachine.InSeasonImpl"/> applies.</summary>
        private int LastDay;

        /// <summary>Whether the bush was in-season as of the last check.</summary>
        private bool InSeasonImpl;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="bush">The underlying bush.</param>
        /// <param name="location">The machine's in-game location.</param>
        public BushMachine(Bush bush, GameLocation location)
            : base(bush, location, GetTileAreaFor(bush)) { }

        /// <summary>Get the output item.</summary>
        public override ITrackedStack GetOutput()
        {
            // tea bush
            if (this.Machine.size.Value == Bush.greenTeaBush)
                return new TrackedItem(new SObject(815, 1), onReduced: this.OnOutputReduced);

            // berry bush
            int itemId = Game1.currentSeason == "fall" ? 410 : 296; // blackberry or salmonberry
            int quality = Game1.player.professions.Contains(Farmer.botanist) ? SObject.bestQuality : SObject.lowQuality;
            int count = 1 + Game1.player.ForagingLevel / 4;
            return new TrackedItem(new SObject(itemId, initialStack: count, quality: quality), onReduced: this.OnOutputReduced);
        }

        /// <summary>Get the machine's processing state.</summary>
        public override MachineState GetState()
        {
            if (!this.InSeason())
                return MachineState.Disabled;

            return this.Machine.tileSheetOffset.Value == 1
                ? MachineState.Done
                : MachineState.Processing;
        }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool SetInput(IStorage input)
        {
            return false; // no input required
        }

        /// <summary>Get whether a bush is an automateable type.</summary>
        /// <param name="bush">The bush to check.</param>
        public static bool CanAutomate(Bush bush)
        {
            return
                (bush.size.Value == Bush.mediumBush && !bush.townBush.Value) // berry bush
                || bush.size.Value == Bush.greenTeaBush; // tea bush
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

        /// <summary>Get the tile area covered by a bush.</summary>
        /// <param name="bush">The bush whose area to get.</param>
        private static Rectangle GetTileAreaFor(Bush bush)
        {
            var box = bush.getBoundingBox();
            return new Rectangle(
                x: box.X / Game1.tileSize,
                y: box.Y / Game1.tileSize,
                width: box.Width / Game1.tileSize,
                height: box.Height / Game1.tileSize
            );
        }

        /// <summary>Get whether the bush is currently in-season to produce berries or tea leaves.</summary>
        private bool InSeason()
        {
            if (this.LastSeason != Game1.currentSeason || this.LastDay != Game1.dayOfMonth)
            {
                this.InSeasonImpl = this.Machine.inBloom(Game1.currentSeason, Game1.dayOfMonth);
                this.LastSeason = Game1.currentSeason;
                this.LastDay = Game1.dayOfMonth;
            }

            return this.InSeasonImpl;
        }
    }
}
