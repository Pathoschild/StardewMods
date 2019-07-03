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
            return new TrackedItem(machine.heldObject.Value, item =>
            {
                machine.heldObject.Value = null;
                machine.readyForHarvest.Value = false;
                machine.showNextIndex.Value = false;
            });
        }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool SetInput(IStorage input)
        {
            if (input.TryGetIngredient(this.Recipes, out IConsumable consumable, out IRecipe recipe))
            {
                // get output
                SObject output = recipe.Output(consumable.Take());
                if (consumable.Sample is SObject sampleInput)
                {
                    if (Game1.random.NextDouble() <= this.GetProbabilityOfDoubleOutput(sampleInput.Quality))
                        output.Stack = 2;
                }

                this.Machine.heldObject.Value = output;
                this.Machine.MinutesUntilReady = recipe.Minutes;
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
    }
}
