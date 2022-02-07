using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
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
                input: SObject.FruitsCategory.ToString(),
                inputCount: 1,
                output: input => ItemRegistry.RequireTypeDefinition<ObjectDataDefinition>(ItemRegistry.type_object).CreateFlavoredJelly((SObject)input),
                minutes: 4000
            ),

            // vegetable or ginger => pickled item
            new Recipe(
                input: item => item.Category == SObject.VegetableCategory || item.QualifiedItemId == "(O)829",
                inputCount: 1,
                output: input => ItemRegistry.RequireTypeDefinition<ObjectDataDefinition>(ItemRegistry.type_object).CreateFlavoredPickle((SObject)input),
                minutes: _ => 4000
            ),

            // roe => aged roe || sturgeon roe => caviar
            new Recipe(
                input: "(O)812", // Roe
                inputCount: 1,
                output: input => ItemRegistry.RequireTypeDefinition<ObjectDataDefinition>(ItemRegistry.type_object).CreateFlavoredAgedRoe((SObject)input),
                minutes: input => input is SObject obj && obj.preservedParentSheetIndex.Value == "698"
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
