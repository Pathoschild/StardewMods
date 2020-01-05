using System.Linq;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Buildings
{
    /// <summary>A Junimo hut machine that accepts input and provides output.</summary>
    internal class JunimoHutMachine : BaseMachine<JunimoHut>
    {
        /*********
        ** Fields
        *********/
        /// <summary>Whether seeds should be ignored when selecting output.</summary>
        private readonly bool IgnoreSeedOutput;

        /// <summary>Whether fertilizer should be ignored when selecting output.</summary>
        private readonly bool IgnoreFertilizerOutput;

        /// <summary>Whether to pull gemstones out of Junimo huts.</summary>
        public bool PullGemstonesFromJunimoHuts { get; set; }

        /// <summary>The Junimo hut's output chest.</summary>
        private Chest Output => this.Machine.output.Value;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="hut">The underlying Junimo hut.</param>
        /// <param name="location">The location which contains the machine.</param>
        /// <param name="ignoreSeedOutput">Whether seeds should be ignored when selecting output.</param>
        /// <param name="ignoreFertilizerOutput">Whether fertilizer should be ignored when selecting output.</param>
        /// <param name="pullGemstonesFromJunimoHuts">Whether to pull gemstones out of Junimo huts.</param>
        public JunimoHutMachine(JunimoHut hut, GameLocation location, bool ignoreSeedOutput, bool ignoreFertilizerOutput, bool pullGemstonesFromJunimoHuts)
            : base(hut, location, BaseMachine.GetTileAreaFor(hut))
        {
            this.IgnoreSeedOutput = ignoreSeedOutput;
            this.IgnoreFertilizerOutput = ignoreFertilizerOutput;
            this.PullGemstonesFromJunimoHuts = pullGemstonesFromJunimoHuts;
        }

        /// <summary>Get the machine's processing state.</summary>
        public override MachineState GetState()
        {
            if (this.Machine.isUnderConstruction())
                return MachineState.Disabled;

            return this.GetNextOutput() != null
                ? MachineState.Done
                : MachineState.Processing;
        }

        /// <summary>Get the machine output.</summary>
        public override ITrackedStack GetOutput()
        {
            return new TrackedItem(this.GetNextOutput(), onEmpty: this.OnOutputTaken);
        }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool SetInput(IStorage input)
        {
            return false; // no input
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Remove an output item once it's been taken.</summary>
        /// <param name="item">The removed item.</param>
        private void OnOutputTaken(Item item)
        {
            this.Output.clearNulls();
            this.Output.items.Remove(item);
        }

        /// <summary>Get the next output item.</summary>
        private Item GetNextOutput()
        {
            foreach (Item item in this.Output.items.Where(p => p != null))
            {
                // ignore gems which change Junimo colors (see JunimoHut:getGemColor)
                if (!this.PullGemstonesFromJunimoHuts && (item.Category == SObject.GemCategory || item.Category == SObject.mineralsCategory))
                    continue;

                // ignore items used by another mod
                if (this.IgnoreSeedOutput && item.Category == SObject.SeedsCategory)
                    continue;
                if (this.IgnoreFertilizerOutput && item.Category == SObject.fertilizerCategory)
                    continue;

                return item;
            }

            return null;
        }
    }
}
