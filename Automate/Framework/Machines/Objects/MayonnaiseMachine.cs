using Microsoft.Xna.Framework;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Objects
{
    /// <summary>A mayonnaise that accepts input and provides output.</summary>
    internal class MayonnaiseMachine : GenericMachine
    {
        /*********
        ** Properties
        *********/
        /// <summary>The recipes to process.</summary>
        private readonly Recipe[] Recipes =
        {
            // void egg => void mayonnaise
            new Recipe(
                input: 305,
                inputCount: 1,
                output: input => new SObject(Vector2.Zero, 308, null, false, true, false, false),
                minutes: 180
            ),

            // duck egg => duck mayonnaise
            new Recipe(
                input: 442,
                inputCount: 1,
                output: input => new SObject(Vector2.Zero, 307, null, false, true, false, false),
                minutes: 180
            ),

            // white/brown egg => normal mayonnaise
            new Recipe(
                input: 176,
                inputCount: 1,
                output: input => new SObject(Vector2.Zero, 306, null, false, true, false, false),
                minutes: 180
            ),
            new Recipe(
                input: 180,
                inputCount: 1,
                output: input => new SObject(Vector2.Zero, 306, null, false, true, false, false),
                minutes: 180
            ),
            
            // dinosaur or large white/brown egg => gold-quality mayonnaise
            new Recipe(
                input: 107,
                inputCount: 1,
                output: input => new SObject(Vector2.Zero, 306, null, false, true, false, false) { quality = SObject.highQuality },
                minutes: 180
            ),
            new Recipe(
                input: 174,
                inputCount: 1,
                output: input => new SObject(Vector2.Zero, 306, null, false, true, false, false) { quality = SObject.highQuality },
                minutes: 180
            ),
            new Recipe(
                input: 182,
                inputCount: 1,
                output: input => new SObject(Vector2.Zero, 306, null, false, true, false, false) { quality = SObject.highQuality },
                minutes: 180
            )
        };


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        public MayonnaiseMachine(SObject machine)
            : base(machine) { }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool SetInput(IStorage input)
        {
            return this.GenericPullRecipe(input, this.Recipes);
        }
    }
}
