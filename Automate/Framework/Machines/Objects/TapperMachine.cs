using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Objects
{
    /// <summary>A tapper that accepts input and provides output.</summary>
    internal class TapperMachine : GenericMachine
    {
        /*********
        ** Properties
        *********/
        /// <summary>The tree type.</summary>
        private readonly int TreeType = -1;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        /// <param name="location">The location to search.</param>
        /// <param name="tile">The machine's position in its location.</param>
        public TapperMachine(SObject machine, GameLocation location, Vector2 tile)
            : base(machine)
        {
            // get tree type
            if (location.terrainFeatures.TryGetValue(tile, out TerrainFeature treeObj))
            {
                if (treeObj is Tree tree)
                    this.TreeType = tree.treeType;
            }
        }

        /// <summary>Get the output item.</summary>
        public override ITrackedStack GetOutput()
        {
            return new TrackedItem(this.Machine.heldObject.getOne(), onEmpty: this.Reset);
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
        /// <summary>Reset the machine so it's ready to accept a new input.</summary>
        /// <param name="item">The output item that was taken.</param>
        private void Reset(Item item)
        {
            SObject tapper = this.Machine;

            switch (this.TreeType)
            {
                case 1:
                    tapper.heldObject = new SObject(725, 1);
                    tapper.minutesUntilReady = 13000 - Game1.timeOfDay;
                    break;
                case 2:
                    tapper.heldObject = new SObject(724, 1);
                    tapper.minutesUntilReady = 16000 - Game1.timeOfDay;
                    break;
                case 3:
                    tapper.heldObject = new SObject(726, 1);
                    tapper.minutesUntilReady = 10000 - Game1.timeOfDay;
                    break;
                case 7:
                    tapper.heldObject = new SObject(420, 1);
                    tapper.minutesUntilReady = 3000 - Game1.timeOfDay;
                    if (!Game1.currentSeason.Equals("fall"))
                    {
                        tapper.heldObject = new SObject(404, 1);
                        tapper.minutesUntilReady = 6000 - Game1.timeOfDay;
                    }
                    break;
            }
            tapper.heldObject = (SObject)tapper.heldObject.getOne();
            tapper.readyForHarvest = false;
        }
    }
}
