using Microsoft.Xna.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Objects
{
    /// <summary>A cheese press that accepts input and provides output.</summary>
    /// <remarks>Derived from <see cref="SObject.performObjectDropInAction"/>.</remarks>
    internal class CheesePressMachine : GenericObjectMachine<SObject>
    {
        /*********
        ** Fields
        *********/
        /// <summary>The recipes processed by this machine (input => output).</summary>
        private readonly IRecipe[] Recipes =
        {
            // goat milk => goat cheese
            new Recipe(
                input: 436,
                inputCount: 1,
                output: input => new SObject(Vector2.Zero, 426, null, false, true, false, false),
                minutes: 200
            ),

            // large goat milk => gold-quality goat cheese
            new Recipe(
                input: 438,
                inputCount: 1,
                output: input => new SObject(Vector2.Zero, 426, null, false, true, false, false) { Quality = SObject.highQuality },
                minutes: 200
            ),

            // milk => cheese
            new Recipe(
                input: 184,
                inputCount: 1,
                output: input => new SObject(Vector2.Zero, 424, null, false, true, false, false),
                minutes: 200
            ),

            // large milk => gold-quality cheese
            new Recipe(
                input: 186,
                inputCount: 1,
                output: input => new SObject(Vector2.Zero, 424, "Cheese (=)", false, true, false, false) { Quality = SObject.highQuality },
                minutes: 200
            )
        };


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        /// <param name="location">The location containing the machine.</param>
        /// <param name="tile">The tile covered by the machine.</param>
        public CheesePressMachine(SObject machine, GameLocation location, Vector2 tile)
            : base(machine, location, tile) { }

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
