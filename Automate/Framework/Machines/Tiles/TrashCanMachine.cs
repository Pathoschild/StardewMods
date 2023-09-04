using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Tiles
{
    /// <summary>A trash can that accepts input and provides output.</summary>
    /// <remarks>Derived from <see cref="Town.checkAction"/>.</remarks>
    internal class TrashCanMachine : BaseMachine
    {
        /*********
        ** Fields
        *********/
        /// <summary>The machine's position in its location.</summary>
        private readonly Vector2 Tile;

        /// <summary>The trash can ID.</summary>
        private readonly string TrashCanId;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="town">The town to search.</param>
        /// <param name="tile">The machine's position in its location.</param>
        /// <param name="trashCanId">The trash can ID.</param>
        public TrashCanMachine(Town town, Vector2 tile, string trashCanId)
            : base(town, BaseMachine.GetTileAreaFor(tile))
        {
            this.Tile = tile;
            this.TrashCanId = trashCanId;
        }

        /// <summary>Get the machine's processing state.</summary>
        public override MachineState GetState()
        {
            if (Game1.netWorldState.Value.CheckedGarbage.Contains(this.TrashCanId))
                return MachineState.Processing;

            return MachineState.Done;
        }

        /// <summary>Get the output item.</summary>
        public override ITrackedStack? GetOutput()
        {
            // get item
            this.Location.TryGetGarbageItem(this.TrashCanId, Game1.MasterPlayer.DailyLuck, out Item? item, out _, out _);
            if (item != null)
                return new TrackedItem(item, onEmpty: _ => this.MarkChecked());

            // if nothing is returned, mark trash can checked
            this.MarkChecked();
            return null;
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
        /// <summary>Reset the machine so it starts processing the next item.</summary>
        private void MarkChecked()
        {
            if (Game1.netWorldState.Value.CheckedGarbage.Add(this.TrashCanId))
                Game1.stats.Increment("trashCansChecked");
        }
    }
}
