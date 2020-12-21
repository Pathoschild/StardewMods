using System;
using Microsoft.Xna.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Objects
{
    /// <summary>A bee house that accepts input and provides output.</summary>
    /// <remarks>Derived from <see cref="SObject.performDropDownAction"/>, <see cref="SObject.checkForAction"/>, and <see cref="SObject.minutesElapsed"/> (search for 'Bee House').</remarks>
    internal class BeeHouseMachine : GenericObjectMachine<SObject>
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        /// <param name="location">The location containing the machine.</param>
        /// <param name="tile">The tile covered by the machine.</param>
        public BeeHouseMachine(SObject machine, GameLocation location, Vector2 tile)
            : base(machine, location, tile) { }

        /// <summary>Get the machine's processing state.</summary>
        public override MachineState GetState()
        {
            return this.Location.GetSeasonForLocation() == "winter"
                ? MachineState.Disabled
                : base.GetState();
        }

        /// <summary>Get the output item.</summary>
        public override ITrackedStack GetOutput()
        {
            // get raw output
            SObject output = this.Machine.heldObject.Value;
            if (output == null)
                return null;

            // get flower data
            int flowerId = -1;
            string flowerName = null;
            int addedPrice = 0;
            Crop flower = Utility.findCloseFlower(this.Location, this.Machine.TileLocation, 5, crop => !crop.forageCrop.Value);
            if (flower != null)
            {
                flowerId = flower.indexOfHarvest.Value;
                string[] fields = Game1.objectInformation[flowerId].Split('/');
                flowerName = fields[0];
                addedPrice = Convert.ToInt32(fields[1]) * 2;
            }

            // build object
            SObject result = new SObject(output.ParentSheetIndex, output.Stack)
            {
                name = $"{flowerName ?? "Wild"} Honey",
                Price = output.Price + addedPrice
            };
            result.preservedParentSheetIndex.Value = flowerId;
            return new TrackedItem(result, onEmpty: this.Reset);
        }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool SetInput(IStorage input)
        {
            return false; // no input needed
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Reset the machine so it's ready to accept a new input.</summary>
        /// <param name="item">The output item that was taken.</param>
        private void Reset(Item item)
        {
            SObject machine = this.Machine;

            machine.heldObject.Value = new SObject(Vector2.Zero, 340, null, false, true, false, false);
            machine.MinutesUntilReady = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay, 4);
            machine.readyForHarvest.Value = false;
            machine.showNextIndex.Value = false;
        }
    }
}
