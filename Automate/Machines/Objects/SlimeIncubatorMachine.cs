using Pathoschild.Stardew.Automate.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Machines.Objects
{
    /// <summary>A slime incubator that accepts slime eggs and spawns slime monsters.</summary>
    internal class SlimeIncubatorMachine : GenericMachine
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
        public SlimeIncubatorMachine(SObject machine) : base(machine)
        {
            int minutesUntilReady = Game1.player.professions.Contains(2) ? 2000 : 4000;


            this.Recipes = new Recipe[]{
                // blue slime egg => object with parentSheetIndex of blue slime egg
                new Recipe(
                    input: 413,
                    inputCount: 1,
                    output: input => new SObject(413,1,false,-1,0),
                    minutes: minutesUntilReady
                ),

                // red slime egg => object with parentSheetIndex of red slime egg
                new Recipe(
                    input: 437,
                    inputCount: 1,
                    output: input => new SObject(437,1,false,-1,0),
                    minutes: minutesUntilReady
                ),

                // purple slime egg => object with parentSheetIndex of purple slime egg
                new Recipe(
                    input: 439,
                    inputCount: 1,
                    output: input => new SObject(439,1),
                    minutes: minutesUntilReady
                ),

                // green slime egg => object with parentSheetIndex of green slime egg
                new Recipe(
                    input: 680,
                    inputCount: 1,
                    output: input => new SObject(680,1),
                    minutes: minutesUntilReady
                )
            };
        }

        /// <summary>Get the machine's processing state.</summary>
        /// <remarks>The slime incubator does not produce an output object, so it is never done.</remarks>
        public override MachineState GetState()
        {
            if (this.Machine.heldObject == null)
                return MachineState.Empty;

            return MachineState.Processing;
        }

        /// <summary>Get the output item.</summary>
        /// <remarks>The slime incubator does not produce an output object.</remarks>
        public override ITrackedStack GetOutput()
        {
            return null;
        }

        /// <summary>Pull items from the connected pipes.</summary>
        /// <param name="pipes">The connected IO pipes.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool Pull(IPipe[] pipes)
        {
            bool result = this.GenericPullRecipe(pipes, this.Recipes);
            if (result)
                this.Machine.parentSheetIndex = 157;
            return result;
        }
    }
}
