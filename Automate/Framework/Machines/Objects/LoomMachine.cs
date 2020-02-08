using Microsoft.Xna.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Objects
{
    /// <summary>A loom that accepts input and provides output.</summary>
    /// <remarks>Derived from <see cref="SObject.performObjectDropInAction"/>.</remarks>
    internal class LoomMachine : GenericObjectMachine<SObject>
    {
        /*********
        ** Fields
        *********/
        /// <summary>The recipes to process.</summary>
        private readonly IRecipe[] Recipes =
        {
            // wool => cloth
            new Recipe(
                input: 440,
                inputCount: 1,
                output: item => new SObject(Vector2.Zero, 428, null, false, true, false, false),
                minutes: 240
            )
        };


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        /// <param name="location">The location containing the machine.</param>
        /// <param name="tile">The tile covered by the machine.</param>
        public LoomMachine(SObject machine, GameLocation location, Vector2 tile)
            : base(machine, location, tile) { }

        /// <summary>Get the output item.</summary>
        public override ITrackedStack GetOutput()
        {
            SObject machine = this.Machine;

            return new TrackedItem(machine.heldObject.Value, this.Reset);
        }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool SetInput(IStorage input)
        {
            SObject machine = this.Machine;

            if (input.TryGetIngredient(this.Recipes, out IConsumable consumable, out IRecipe recipe))
            {
                // get output
                var inputStack = consumable.Take();
                SObject output = recipe.Output(inputStack);
                if (consumable.Sample is SObject sampleInput)
                {
                    if (Game1.random.NextDouble() <= this.GetProbabilityOfDoubleOutput(sampleInput.Quality))
                        output.Stack = 2;
                }

                machine.heldObject.Value = output;
                machine.MinutesUntilReady = recipe.Minutes(inputStack);
                return true;
            }

            return false;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the probability that the cheese press outputs two items for a given input quality.</summary>
        /// <param name="quality">The input quality.</param>
        private float GetProbabilityOfDoubleOutput(int quality)
        {
            switch (quality)
            {
                case SObject.lowQuality:
                    return 0;

                case SObject.highQuality:
                    return 0.25f;

                case SObject.bestQuality:
                    return 0.5f;

                default:
                    return 0.1f;
            }
        }

        /// <summary>Reset the machine so it's ready to accept a new input.</summary>
        /// <param name="item">The output item that was taken.</param>
        private void Reset(Item item)
        {
            SObject machine = this.Machine;

            this.GenericReset(item);
            machine.showNextIndex.Value = false;
        }
    }
}
