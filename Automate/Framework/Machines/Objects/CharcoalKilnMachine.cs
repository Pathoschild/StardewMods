using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Objects
{
    /// <summary>A charcoal kiln that accepts input and provides output.</summary>
    internal class CharcoalKilnMachine : GenericMachine
    {
        /*********
        ** Properties
        *********/
        /// <summary>The recipes to process.</summary>
        private readonly Recipe[] Recipes =
        {
            // wood => coal
            new Recipe(
                input: 388,
                inputCount: 10,
                output: input => new SObject(382, 1),
                minutes: 30
            )
        };


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        public CharcoalKilnMachine(SObject machine)
            : base(machine) { }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool SetInput(IStorage input)
        {
            if (this.GenericPullRecipe(input, this.Recipes))
            {
                this.Machine.showNextIndex = true;
                return true;
            }
            return false;
        }
    }
}
