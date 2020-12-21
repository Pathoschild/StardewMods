using Microsoft.Xna.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Objects
{
    /// <summary>A solar panel that provides output.</summary>
    /// <remarks>Derived from <see cref="SObject.performDropDownAction"/> (search for 'Solar Panel') and <see cref="SObject.checkForAction"/> (search for 'case 231').</remarks>
    internal class SolarPanelMachine : GenericObjectMachine<SObject>
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        /// <param name="location">The location containing the machine.</param>
        /// <param name="tile">The tile covered by the machine.</param>
        public SolarPanelMachine(SObject machine, GameLocation location, Vector2 tile)
            : base(machine, location, tile) { }

        /// <summary>Get the output item.</summary>
        public override ITrackedStack GetOutput()
        {
            return new TrackedItem(this.Machine.heldObject.Value, onEmpty: this.Reset);
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
        /// <summary>Reset the solar panel.</summary>
        /// <param name="item">The output item that was taken.</param>
        private void Reset(Item item)
        {
            this.GenericReset(item);

            this.Machine.heldObject.Value = new Object(Vector2.Zero, 787, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
            this.Machine.MinutesUntilReady = 16800;
        }
    }
}
