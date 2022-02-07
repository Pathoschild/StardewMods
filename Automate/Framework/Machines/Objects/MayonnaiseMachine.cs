using Microsoft.Xna.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Objects
{
    /// <summary>A mayonnaise that accepts input and provides output.</summary>
    /// <remarks>Derived from <see cref="SObject.performObjectDropInAction"/> (search for 'Mayonnaise Machine').</remarks>
    internal class MayonnaiseMachine : GenericObjectMachine<SObject>
    {
        /*********
        ** Fields
        *********/
        /// <summary>The recipes to process.</summary>
        private readonly IRecipe[] Recipes =
        {
            // void egg => void mayonnaise
            new Recipe(
                input: "(O)305",
                inputCount: 1,
                output: _ => ItemRegistry.Create("(O)308"),
                minutes: 180
            ),

            // duck egg => duck mayonnaise
            new Recipe(
                input: "(O)442",
                inputCount: 1,
                output: _ => ItemRegistry.Create("(O)307"),
                minutes: 180
            ),

            // dinosaur egg => dinosaur mayonnaise
            new Recipe(
                input: "(O)107",
                inputCount: 1,
                output: _ => ItemRegistry.Create("(O)807"),
                minutes: 180
            ),

            // ostrich egg => 10x same-quality mayonnaise
            new Recipe(
                input: "(O)289",
                inputCount: 1,
                output: input => ItemRegistry.Create("(O)306", 10, input.Quality),
                minutes: 180
            ),

            // white/brown egg => normal mayonnaise
            new Recipe(
                input: item => item.QualifiedItemId is "(O)176" or "(O)180",
                inputCount: 1,
                output: _ => ItemRegistry.Create("(O)306"),
                minutes: _ => 180
            ),
            
            // dinosaur or large white/brown egg => gold-quality mayonnaise
            new Recipe(
                input: item => item.QualifiedItemId is "(O)107" or "(O)174" or "(O)182",
                inputCount: 1,
                output: _ => ItemRegistry.Create("(O)306", quality: SObject.highQuality),
                minutes: _ => 180
            ),

            // golden egg => 3x gold-quality mayonnaise
            new Recipe(
                input: "(O)928",
                inputCount: 1,
                output: _ => ItemRegistry.Create("(O)306", 3, SObject.highQuality),
                minutes: 180
            ),
        };


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        /// <param name="location">The location containing the machine.</param>
        /// <param name="tile">The tile covered by the machine.</param>
        public MayonnaiseMachine(SObject machine, GameLocation location, Vector2 tile)
            : base(machine, location, tile, machineTypeId: "MayonnaiseMachine") { }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool SetInput(IStorage input)
        {
            return this.GenericPullRecipe(input, this.Recipes);
        }
    }
}
