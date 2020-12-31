using Microsoft.Xna.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Objects
{
    /// <summary>A slime incubator that accepts slime eggs and spawns slime monsters.</summary>
    /// <remarks>Derived from <see cref="SObject.performObjectDropInAction"/> (search for 'Slime Incubator').</remarks>
    internal class SlimeIncubatorMachine : GenericObjectMachine<SObject>
    {
        /*********
        ** Fields
        *********/
        /// <summary>The recipes to process.</summary>
        private readonly IRecipe[] Recipes;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        /// <param name="location">The location containing the machine.</param>
        /// <param name="tile">The tile covered by the machine.</param>
        public SlimeIncubatorMachine(SObject machine, GameLocation location, Vector2 tile)
            : base(machine, location, tile)
        {
            int minutesUntilReady = Game1.player.professions.Contains(2) ? 2000 : 4000;
            this.Recipes = new IRecipe[] {
                new Recipe(
                    input: p =>
                        p.ParentSheetIndex == 413 // blue slime egg
                        || p.ParentSheetIndex == 437 // red slime egg
                        || p.ParentSheetIndex == 439 // purple slime egg
                        || p.ParentSheetIndex == 680 // green slime egg
                        || p.ParentSheetIndex == 857, // tiger slime egg
                    inputCount: 1,
                    output: input => new SObject(input.ParentSheetIndex, 1),
                    minutes: _ => minutesUntilReady
                )
            };
        }

        /// <summary>Get the machine's processing state.</summary>
        /// <remarks>The slime incubator does not produce an output object, so it is never done.</remarks>
        public override MachineState GetState()
        {
            return this.Machine.heldObject.Value != null
                ? MachineState.Processing
                : MachineState.Empty;
        }

        /// <summary>Get the output item.</summary>
        /// <remarks>The slime incubator does not produce an output object.</remarks>
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
                this.Machine.ParentSheetIndex = 157;
            return started;
        }
    }
}
