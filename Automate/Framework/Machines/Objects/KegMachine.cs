using Microsoft.Xna.Framework;
using StardewValley;
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
                input: 340,
                inputCount: 1,
                output: _ => new SObject(Vector2.Zero, 459, "Mead", false, true, false, false) { name = "Mead" },
                minutes: 600
            ),

            // coffee bean => coffee
            new Recipe(
                input: 433,
                inputCount: 5,
                output: _ => new SObject(Vector2.Zero, 395, "Coffee", false, true, false, false) { name = "Coffee" },
                minutes: 120
            ),

            // tea leaves => green tea
            new Recipe(
                input: 815,
                inputCount: 1,
                output: _ => new SObject(Vector2.Zero, 614, "Green Tea", false, true, false, false) { name = "Green Tea" },
                minutes: 180
            ),

            // wheat => beer
            new Recipe(
                input: 262,
                inputCount: 1,
                output: input => {
                    SObject beer = new SObject(Vector2.Zero, 346, "Beer", false, true, false, false)
                    {
                        name = "Beer",
                        Price = ((SObject)input).Price * 3 + (((SObject)input).Quality * 50),
                        Quality = ((SObject)input).Quality
                    };
                    return beer;
                },
                minutes: 1750
            ),

            // hops => pale ale
            new Recipe(
                input: 304,
                inputCount: 1,
                output: input => {
                    SObject ale = new SObject(Vector2.Zero, 303, "Pale Ale", false, true, false, false)
                    {
                        name = "Pale Ale",
                        Quality = ((SObject)input).Quality
                    };
                    ale.Price+= (((SObject)input).Quality * 50);
                    return ale;
                },
                minutes: 2250
            ),

            // fruit => wine
            new Recipe(
                input: SObject.FruitsCategory,
                inputCount: 1,
                output: input =>
                {
                    SObject wine = new SObject(Vector2.Zero, 348, input.Name + " Wine", false, true, false, false)
                    {
                        name = input.Name + " Wine",
                        Price = ((SObject)input).Price * 3 + (((SObject)input).Quality * 50),
                        preserve = { Value = SObject.PreserveType.Wine },
                        preservedParentSheetIndex = { Value = input.ParentSheetIndex },
                        Quality = ((SObject)input).Quality
                    };
                    return wine;
                },
                minutes: 10000
            ),
            new Recipe(
                input: SObject.VegetableCategory,
                inputCount: 1,
                output: input =>
                {
                    SObject juice = new SObject(Vector2.Zero, 350, input.Name + " Juice", false, true, false, false)
                    {
                        name = input.Name + " Juice",
                        Price = (int)(((SObject)input).Price * 2.25) + (((SObject)input).Quality * 50),
                        preserve = { Value = SObject.PreserveType.Juice },
                        preservedParentSheetIndex = { Value = input.ParentSheetIndex },
                        Quality = ((SObject)input).Quality
                    };
                    return juice;
                },
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
