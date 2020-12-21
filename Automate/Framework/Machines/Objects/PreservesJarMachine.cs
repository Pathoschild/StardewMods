using System;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Objects
{
    /// <summary>A preserves jar that accepts input and provides output.</summary>
    /// <remarks>Derived from <see cref="SObject.performObjectDropInAction"/> and <see cref="SObject.checkForAction"/> (search for 'Preserves Jar').</remarks>
    internal class PreservesJarMachine : GenericObjectMachine<SObject>
    {
        /*********
        ** Fields
        *********/
        /// <summary>The recipes to process.</summary>
        private readonly IRecipe[] Recipes =
        {
            // fruit => jelly
            new Recipe(
                input: SObject.FruitsCategory,
                inputCount: 1,
                output: input =>
                {
                    SObject jelly = new SObject(Vector2.Zero, 344, input.Name + " Jelly", false, true, false, false)
                    {
                        Price = 50 + ((SObject) input).Price * 2,
                        name = input.Name + " Jelly"
                    };
                    jelly.preserve.Value = SObject.PreserveType.Jelly;
                    jelly.preservedParentSheetIndex.Value = input.ParentSheetIndex;
                    return jelly;
                },
                minutes: 4000
            ),

            // vegetable or ginger => pickled item
            new Recipe(
                input: item => item.Category == SObject.VegetableCategory || item.ParentSheetIndex == 829,
                inputCount: 1,
                output: input =>
                {
                    SObject item = new SObject(Vector2.Zero, 342, "Pickled " + input.Name, false, true, false, false)
                    {
                        Price = 50 + ((SObject) input).Price * 2,
                        name = "Pickled " + input.Name
                    };
                    item.preserve.Value = SObject.PreserveType.Pickle;
                    item.preservedParentSheetIndex.Value = input.ParentSheetIndex;
                    return item;

                },
                minutes: _ => 4000
            ),

            // roe => aged roe || sturgeon roe => caviar
            new Recipe(
                input: 812, // Roe
                inputCount: 1,
                output: input =>
                {
                    if (!(input is SObject inputObj))
                        throw new InvalidOperationException($"Unexpected recipe input: expected {typeof(SObject).FullName} instance.");

                    // sturgeon roe => caviar
                    if (inputObj.preservedParentSheetIndex.Value == 698)
                        return new SObject(445, 1);

                    // roe => aged roe
                    var result = (input is ColoredObject coloredInput) ? new ColoredObject(447, 1, coloredInput.color.Value) : new SObject(447, 1);
                    result.name = $"Aged {input.Name}";
                    result.preserve.Value = SObject.PreserveType.AgedRoe;
                    result.preservedParentSheetIndex.Value = inputObj.preservedParentSheetIndex.Value;
                    result.Category = -26;
                    result.Price = inputObj.Price * 2;
                    return result;
                },
                minutes: input => input is SObject obj && obj.preservedParentSheetIndex.Value == 698
                    ? 6000 // caviar
                    : 4000 // aged roe
            )
        };


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        /// <param name="location">The location containing the machine.</param>
        /// <param name="tile">The tile covered by the machine.</param>
        public PreservesJarMachine(SObject machine, GameLocation location, Vector2 tile)
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
