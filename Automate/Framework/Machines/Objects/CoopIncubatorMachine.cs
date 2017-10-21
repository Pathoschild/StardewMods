using StardewValley;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Objects
{
    /// <summary>A coop incubator that accepts eggs and spawns chickens.</summary>
    internal class CoopIncubatorMachine : GenericMachine
    {
        /*********
        ** Properties
        *********/
        /// <summary>The recipes to process.</summary>
        private readonly Recipe[] Recipes;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        public CoopIncubatorMachine(SObject machine)
            : base(machine)
        {
            int minutesUntilReady = Game1.player.professions.Contains(2) ? 9000 : 18000;
            this.Recipes = new[]
            {
                // egg => chicken
                new Recipe(
                    input: -5,
                    inputCount: 1,
                    output: item => new SObject(item.parentSheetIndex, 1),
                    minutes: minutesUntilReady / 2
                ),

                // dinosaur egg => dinosaur
                new Recipe(
                    input: 107,
                    inputCount: 1,
                    output: item => new SObject(107, 1),
                    minutes: minutesUntilReady
                )
            };
        }

        /// <summary>Get the machine's processing state.</summary>
        /// <remarks>The coop incubator never produces an object output so it is never done.</remarks>
        public override MachineState GetState()
        {
            return this.Machine.heldObject != null
                ? MachineState.Processing
                : MachineState.Empty;
        }

        /// <summary>Get the output item.</summary>
        /// <remarks>The coop incubator never produces an object output.</remarks>
        public override ITrackedStack GetOutput()
        {
            return null;
        }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool SetInput(IStorage input)
        {
            bool started = this.GenericPullRecipe(input, this.Recipes);
            if (started)
            {
                int eggID = this.Machine.heldObject.parentSheetIndex;
                this.Machine.parentSheetIndex = eggID == 180 || eggID == 182 || eggID == 305
                    ? this.Machine.parentSheetIndex + 2
                    : this.Machine.parentSheetIndex + 1;
            }
            return started;
        }
    }
}
