using Microsoft.Xna.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Objects
{
    /// <summary>A furnace that accepts input and provides output.</summary>
    /// <remarks>Derived from <see cref="SObject.performObjectDropInAction"/> (search for 'Furnace').</remarks>
    internal class FurnaceMachine : GenericObjectMachine<SObject>
    {
        /*********
        ** Fields
        *********/
        /// <summary>The recipes to process.</summary>
        private readonly IRecipe[] Recipes =
        {
            // copper => copper bar
            new Recipe(
                input: $"(O){SObject.copper}",
                inputCount: 5,
                output: _ => ItemRegistry.Create(SObject.copperBarQID),
                minutes: 30
            ),

            // iron => iron bar
            new Recipe(
                input: $"(O){SObject.iron}",
                inputCount: 5,
                output: _ => ItemRegistry.Create(SObject.ironBarQID),
                minutes: 120
            ),

            // gold => gold bar
            new Recipe(
                input: $"(O){SObject.gold}",
                inputCount: 5,
                output: _ => ItemRegistry.Create(SObject.goldBarQID),
                minutes: 300
            ),

            // iridium => iridium bar
            new Recipe(
                input: $"(O){SObject.iridium}",
                inputCount: 5,
                output: _ => ItemRegistry.Create(SObject.iridiumBarQID),
                minutes: 480
            ),

            // radioactive ore => radioactive bar
            new Recipe(
                input: "(O)909",
                inputCount: 5,
                output: _ => ItemRegistry.Create("(O)910"),
                minutes: 560
            ),

            // quartz => refined quartz
            new Recipe(
                input: $"(O){SObject.quartzID}",
                inputCount: 1,
                output: _ => ItemRegistry.Create("(O)338"),
                minutes: 90
            ),

            // fire quartz => refined quartz
            new Recipe(
                input: "(O)82",
                inputCount: 1,
                output: _ => ItemRegistry.Create("(O)338", 3),
                minutes: 90
            ),

            // bouquet => wilted bouquet
            new Recipe(
                input: "(O)458",
                inputCount: 1,
                output: _ => ItemRegistry.Create("(O)277"),
                minutes: 10
            )
        };


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        /// <param name="location">The location containing the machine.</param>
        /// <param name="tile">The tile covered by the machine.</param>
        public FurnaceMachine(SObject machine, GameLocation location, Vector2 tile)
            : base(machine, location, tile) { }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool SetInput(IStorage input)
        {
            if (input.TryGetIngredient(p => p.Sample.QualifiedItemId == SObject.coalQID, 1, out IConsumable? coal) && this.GenericPullRecipe(input, this.Recipes))
            {
                coal.Reduce();
                this.Machine.initializeLightSource(this.Machine.TileLocation);
                this.Machine.showNextIndex.Value = true;
                return true;
            }
            return false;
        }
    }
}
