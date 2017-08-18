using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Automate.Framework;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Machines.Objects
{
    /// <summary>A cheese press that accepts input and provides output.</summary>
    internal class CheesePressMachine : GenericMachine
    {
        /*********
        ** Properties
        *********/
        /// <summary>The recipes processed by this machine (input => output).</summary>
        private readonly Recipe[] Recipes =
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
                output: input => new SObject(Vector2.Zero, 426, null, false, true, false, false) { quality = SObject.highQuality },
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
                output: input => new SObject(Vector2.Zero, 424, "Cheese (=)", false, true, false, false) { quality = SObject.highQuality },
                minutes: 200
            )
        };


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        public CheesePressMachine(SObject machine)
            : base(machine) { }

        /// <summary>Pull items from the connected pipes.</summary>
        /// <param name="pipes">The connected IO pipes.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool Pull(IPipe[] pipes)
        {
            if (pipes.TryGetIngredient(this.Recipes, out Consumable consumable, out Recipe recipe))
            {
                this.Machine.heldObject = recipe.Output(consumable.Take());
                this.Machine.minutesUntilReady = recipe.Minutes;
                return true;
            }

            return false;
        }
    }
}
