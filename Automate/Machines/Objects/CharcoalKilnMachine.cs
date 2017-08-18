using Pathoschild.Stardew.Automate.Framework;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Machines.Objects
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

        /// <summary>Pull items from the connected pipes.</summary>
        /// <param name="pipes">The connected IO pipes.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool Pull(IPipe[] pipes)
        {
            if (this.GenericPullRecipe(pipes, this.Recipes))
            {
                this.Machine.showNextIndex = true;
                return true;
            }
            return false;
        }
    }
}
