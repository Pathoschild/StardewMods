using Microsoft.Xna.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Objects
{
    /// <summary>A geode crusher that accepts input and provides output.</summary>
    /// <remarks>Derived from <see cref="SObject.performObjectDropInAction"/> (search for 'Geode Crusher').</remarks>
    internal class GeodeCrusherMachine : GenericObjectMachine<SObject>
    {
        /*********
        ** Fields
        *********/
        /// <summary>The recipes to process.</summary>
        private readonly IRecipe[] Recipes =
        {
            new Recipe(
                input: item => Utility.IsGeode(item, disallow_special_geodes: true),
                inputCount: 1,
                output: input => (SObject)Utility.getTreasureFromGeode(input),
                minutes: _ => 60
            )
        };


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        /// <param name="location">The location containing the machine.</param>
        /// <param name="tile">The tile covered by the machine.</param>
        public GeodeCrusherMachine(SObject machine, GameLocation location, Vector2 tile)
            : base(machine, location, tile) { }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool SetInput(IStorage input)
        {
            if (input.TryGetIngredient(SObject.coal, 1, out IConsumable coal) && this.GenericPullRecipe(input, this.Recipes))
            {
                coal.Reduce();
                this.Machine.showNextIndex.Value = true;
                Game1.stats.GeodesCracked++;
                return true;
            }
            return false;
        }
    }
}
