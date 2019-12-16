using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Objects
{
    /// <summary>A wood chipper that accepts input and provides output.</summary>
    /// <remarks>Derived from <see cref="WoodChipper.performObjectDropInAction"/> and <see cref="WoodChipper.checkForAction"/>.</remarks>
    internal class WoodChipperMachine : GenericObjectMachine<WoodChipper>
    {
        /*********
        ** Fields
        *********/
        /// <summary>The recipes to process.</summary>
        private readonly IRecipe[] Recipes;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        /// <param name="location">The location containing the machine.</param>
        /// <param name="tile">The tile covered by the machine.</param>
        public WoodChipperMachine(WoodChipper machine, GameLocation location, Vector2 tile)
            : base(machine, location, tile)
        {
            this.Recipes = new IRecipe[]
            {
                // hardwood => maple syrup, oak resin, pine tar, wood
                new Recipe(
                    input: 709,
                    inputCount: 1,
                    output: input =>
                    {
                        KeyValuePair<int, int> result = this.Machine.GetResultItem(input as SObject);
                        return new SObject(result.Key, result.Value);
                    },
                    minutes: 180
                ),

                // driftwood => wood
                new Recipe(
                    input: 169,
                    inputCount: 1,
                    output: input =>
                    {
                        KeyValuePair<int, int> result = this.Machine.GetResultItem(input as SObject);
                        return new SObject(result.Key, result.Value);
                    },
                    minutes: 180
                )
            };
        }

        /// <summary>Get the output item.</summary>
        public override ITrackedStack GetOutput()
        {
            WoodChipper machine = this.Machine;

            return new TrackedItem(machine.heldObject.Value, this.Reset);
        }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool SetInput(IStorage input)
        {
            if (this.GenericPullRecipe(input, this.Recipes, out Item inputItem))
            {
                this.Machine.depositedItem.Value = (SObject)inputItem;
                return true;
            }
            return false;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Reset the machine so it's ready to accept a new input.</summary>
        /// <param name="item">The output item that was taken.</param>
        private void Reset(Item item)
        {
            WoodChipper machine = this.Machine;

            this.GenericReset(item);
            machine.depositedItem.Value = null;
        }
    }
}
