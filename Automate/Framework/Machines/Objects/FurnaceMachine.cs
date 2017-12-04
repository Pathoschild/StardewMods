using Microsoft.Xna.Framework;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Objects
{
    /// <summary>A furnace that accepts input and provides output.</summary>
    internal class FurnaceMachine : GenericMachine
    {
        /*********
        ** Properties
        *********/
        /// <summary>The machine's position in its location.</summary>
        private readonly Vector2 Tile;

        /// <summary>The recipes to process.</summary>
        private readonly Recipe[] Recipes =
        {
            // copper => copper bar
            new Recipe(
                input: SObject.copper,
                inputCount: 5,
                output: input => new SObject(Vector2.Zero, SObject.copperBar, 1),
                minutes: 30
            ),

            // iron => iron bar
            new Recipe(
                input: SObject.iron,
                inputCount: 5,
                output: input => new SObject(Vector2.Zero, SObject.ironBar, 1),
                minutes: 120
            ),

            // gold => gold bar
            new Recipe(
                input: SObject.gold,
                inputCount: 5,
                output: input => new SObject(Vector2.Zero, SObject.goldBar, 1),
                minutes: 300
            ),

            // iridium => iridium bar
            new Recipe(
                input: SObject.iridium,
                inputCount: 5,
                output: input => new SObject(Vector2.Zero, SObject.iridiumBar, 1),
                minutes: 480
            ),

            // quartz => refined quartz
            new Recipe(
                input: SObject.quartzIndex,
                inputCount: 1,
                output: input => new SObject(Vector2.Zero, 338, "Refined Quartz", false, true, false, false),
                minutes: 90
            )
        };


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        /// <param name="tile">The machine's position in its location.</param>
        public FurnaceMachine(SObject machine, Vector2 tile)
            : base(machine)
        {
            this.Tile = tile;
        }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool SetInput(IStorage input)
        {
            if (input.TryGetIngredient(SObject.coal, 1, out IConsumable coal) && this.GenericPullRecipe(input, this.Recipes))
            {
                coal.Reduce();
                this.Machine.initializeLightSource(this.Tile);
                this.Machine.showNextIndex = true;
                return true;
            }
            return false;
        }
    }
}
