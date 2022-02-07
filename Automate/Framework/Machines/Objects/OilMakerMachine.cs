using Microsoft.Xna.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Objects
{
    /// <summary>An oil maker that accepts input and provides output.</summary>
    /// <remarks>Derived from <see cref="SObject.performObjectDropInAction"/> (search for 'Oil Maker').</remarks>
    internal class OilMakerMachine : GenericObjectMachine<SObject>
    {
        /*********
        ** Fields
        *********/
        /// <summary>The recipes to process.</summary>
        private readonly IRecipe[] Recipes =
        {
            // truffle => truffle oil
            new Recipe(
                input: "(O)430",
                inputCount: 1,
                output: _ => ItemRegistry.Create("(O)432"),
                minutes: 360
            ),

            // sunflower seed => oil
            new Recipe(
                input: "(O)431",
                inputCount: 1,
                output: _ => ItemRegistry.Create("(O)247"),
                minutes: 3200
            ),

            // corn => oil
            new Recipe(
                input: "(O)270",
                inputCount: 1,
                output: _ => ItemRegistry.Create("(O)247"),
                minutes: 1000
            ), 

            // sunflower => oil
            new Recipe(
                input: "(O)421",
                inputCount: 1,
                output: _ => ItemRegistry.Create("(O)247"),
                minutes: 60
            )
        };


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        /// <param name="location">The location containing the machine.</param>
        /// <param name="tile">The tile covered by the machine.</param>
        public OilMakerMachine(SObject machine, GameLocation location, Vector2 tile)
            : base(machine, location, tile) { }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool SetInput(IStorage input)
        {
            return this.GenericPullRecipe(input, this.Recipes);
        }
    }
}
