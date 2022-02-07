using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Objects
{
    /// <summary>A keg that accepts input and provides output.</summary>
    /// <remarks>Derived from <see cref="SObject.performObjectDropInAction"/> and <see cref="SObject.checkForAction"/> (search for 'Keg').</remarks>
    internal class KegMachine : GenericObjectMachine<SObject>
    {
        /*********
        ** Fields
        *********/
        /// <summary>The recipes to process.</summary>
        private readonly IRecipe[] Recipes =
        {
            // honey => mead
            new Recipe(
                input: "(O)340",
                inputCount: 1,
                output: _ => ItemRegistry.Create("(O)459"),
                minutes: 600
            ),

            // coffee bean => coffee
            new Recipe(
                input: "(O)433",
                inputCount: 5,
                output: _ => ItemRegistry.Create("(O)395"),
                minutes: 120
            ),

            // tea leaves => green tea
            new Recipe(
                input: "(O)815",
                inputCount: 1,
                output: _ => ItemRegistry.Create("(O)614"),
                minutes: 180
            ),

            // wheat => beer
            new Recipe(
                input: "(O)262",
                inputCount: 1,
                output: _ => ItemRegistry.Create("(O)346"),
                minutes: 1750
            ),

            // hops => pale ale
            new Recipe(
                input: "(O)304",
                inputCount: 1,
                output: _ => ItemRegistry.Create("(O)303"),
                minutes: 2250
            ),

            // fruit => wine
            new Recipe(
                input: $"{SObject.FruitsCategory}",
                inputCount: 1,
                output: input => ItemRegistry.RequireTypeDefinition<ObjectDataDefinition>(ItemRegistry.type_object).CreateFlavoredWine((SObject)input),
                minutes: 10000
            ),
            new Recipe(
                input: $"{SObject.VegetableCategory}",
                inputCount: 1,
                output: input => ItemRegistry.RequireTypeDefinition<ObjectDataDefinition>(ItemRegistry.type_object).CreateFlavoredJuice((SObject)input),
                minutes: 6000
            )
        };


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        /// <param name="location">The location containing the machine.</param>
        /// <param name="tile">The tile covered by the machine.</param>
        public KegMachine(SObject machine, GameLocation location, Vector2 tile)
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
