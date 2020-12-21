using Microsoft.Xna.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Objects
{
    /// <summary>An ostrich incubator that accepts eggs and spawns ostriches.</summary>
    /// <remarks>Derived from <see cref="SObject.performObjectDropInAction"/> (search for 'ostrich incubator').</remarks>
    internal class OstrichIncubatorMachine : GenericObjectMachine<SObject>
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
        public OstrichIncubatorMachine(SObject machine, GameLocation location, Vector2 tile)
            : base(machine, location, tile)
        {
            int minutesUntilReady = Game1.player.professions.Contains(Farmer.butcher) ? 7500 : 15000; // coopmaster
            this.Recipes = new IRecipe[]
            {
                // ostrich egg => ostrich
                new Recipe(
                    input: 289,
                    inputCount: 1,
                    output: item => new SObject(item.ParentSheetIndex, 1),
                    minutes: minutesUntilReady / 2
                )
            };
        }

        /// <summary>Get the machine's processing state.</summary>
        /// <remarks>The coop incubator never produces an object output so it is never done.</remarks>
        public override MachineState GetState()
        {
            return this.Machine.heldObject.Value != null
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
                this.Machine.ParentSheetIndex++;
            return started;
        }
    }
}
