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
                input: SObject.copper,
                inputCount: 5,
                output: _ => new SObject(SObject.copperBar, 1),
                minutes: 30
            ),

            // iron => iron bar
            new Recipe(
                input: SObject.iron,
                inputCount: 5,
                output: _ => new SObject(SObject.ironBar, 1),
                minutes: 120
            ),

            // gold => gold bar
            new Recipe(
                input: SObject.gold,
                inputCount: 5,
                output: _ => new SObject(SObject.goldBar, 1),
                minutes: 300
            ),

            // iridium => iridium bar
            new Recipe(
                input: SObject.iridium,
                inputCount: 5,
                output: _ => new SObject(SObject.iridiumBar, 1),
                minutes: 480
            ),

            // radioactive ore => radioactive bar
            new Recipe(
                input: 909,
                inputCount: 5,
                output: _ => new SObject(910, 1),
                minutes: 560
            ),

            // quartz => refined quartz
            new Recipe(
                input: SObject.quartzIndex,
                inputCount: 1,
                output: _ => new SObject(338, 1),
                minutes: 90
            ),

            // fire quartz => refined quartz
            new Recipe(
                input: 82,
                inputCount: 1,
                output: _ => new SObject(338, 3),
                minutes: 90
            ),

            // bouquet => wilted bouquet
            new Recipe(
                input: 458,
                inputCount: 1,
                output: _ => new SObject(277, 1),
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
            if (input.TryGetIngredient(SObject.coal, 1, out IConsumable? coal) && this.GenericPullRecipe(input, this.Recipes))
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
