using Microsoft.Xna.Framework;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Objects
{
    /// <summary>A preserves jar that accepts input and provides output.</summary>
    /// <remarks>See the game's machine logic in <see cref="SObject.performObjectDropInAction"/> and <see cref="SObject.checkForAction"/>.</remarks>
    internal class PreservesJarMachine : GenericMachine
    {
        /*********
        ** Properties
        *********/
        /// <summary>The recipes to process.</summary>
        private readonly Recipe[] Recipes =
        {
            // fruit => jelly
            new Recipe(
                input: SObject.FruitsCategory,
                inputCount: 1,
                output: input => new SObject(Vector2.Zero, 344, input.Name + " Jelly", false, true, false, false)
                {
                    Price = 50 + ((SObject) input).Price * 2,
                    name = input.Name + " Jelly",
                    preserve = SObject.PreserveType.Jelly,
                    preservedParentSheetIndex = input.parentSheetIndex
                },
                minutes: 4000
            ),

            // vegetable => pickled vegetable
            new Recipe(
                input: SObject.VegetableCategory,
                inputCount: 1,
                output: input => new SObject(Vector2.Zero, 342, "Pickled " + input.Name, false, true, false, false)
                {
                    Price = 50 + ((SObject)input).Price * 2,
                    name = "Pickled " + input.Name,
                    preserve = SObject.PreserveType.Pickle,
                    preservedParentSheetIndex = input.parentSheetIndex
                },
                minutes: 4000
            )
        };


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        public PreservesJarMachine(SObject machine)
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
