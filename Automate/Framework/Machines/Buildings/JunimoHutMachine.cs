using System.Linq;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Buildings
{
    /// <summary>A Junimo hut machine that accepts input and provides output.</summary>
    internal class JunimoHutMachine : BaseMachineForBuilding<JunimoHut>
    {
        /*********
        ** Fields
        *********/
        /// <summary>Whether seeds should be treated as Junimo hut inputs.<summary>
        private readonly bool AllowSeedInput;

        /// <summary>Whether fertilizer should be treated as Junimo hut inputs.<summary>
        private readonly bool AllowFertilizerInput;

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
        /// <param name="allowSeedInput">Whether seeds are allowed as an input.</param>
        /// <param name="allowFertilizerInput">Whether fertilizers are allowed as an input.</param>
        /// <param name="ignoreSeedOutput">Whether seeds should be ignored when selecting output.</param>
        /// <param name="ignoreFertilizerOutput">Whether fertilizer should be ignored when selecting output.</param>
        /// <param name="pullGemstonesFromJunimoHuts">Whether to pull gemstones out of Junimo huts.</param>
        public JunimoHutMachine(JunimoHut hut, GameLocation location, bool allowSeedInput, bool allowFertilizerInput, bool ignoreSeedOutput, bool ignoreFertilizerOutput, bool pullGemstonesFromJunimoHuts)
            : base(hut, location, BaseMachine.GetTileAreaFor(hut))
        {
            this.AllowSeedInput = allowSeedInput;
            this.AllowFertilizerInput = allowFertilizerInput;
            this.IgnoreSeedOutput = ignoreSeedOutput;
            this.IgnoreFertilizerOutput = ignoreFertilizerOutput;
            this.PullGemstonesFromJunimoHuts = pullGemstonesFromJunimoHuts;
        }

        /// <summary>Get the machine's processing state.</summary>
        public override MachineState GetState()
        {
            if (this.Machine.isUnderConstruction())
                return MachineState.Disabled;
            if (this.AllowSeedInput || this.AllowFertilizerInput)
                return MachineState.Empty;

            return this.GetNextOutput() != null
                ? MachineState.Done
                : MachineState.Processing;
        }

        /// <summary>Get the machine output.</summary>
        public override ITrackedStack? GetOutput()
        {
            return this.GetTracked(this.GetNextOutput(), onEmpty: this.OnOutputTaken);
        }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool SetInput(IStorage input)
        {
            // get next item
            ITrackedStack? tracker = input.GetItems().FirstOrDefault(p => p.Sample is SObject obj && ((this.AllowSeedInput && (obj.Category == SObject.SeedsCategory)) || (this.AllowFertilizerInput && (obj.Category == SObject.fertilizerCategory))));
            if (tracker == null)
                return false;

            // place item in output chest
            SObject item = (SObject)tracker.Take(1)!;
            this.Output.addItem(item);
            return true;
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
        private Item? GetNextOutput()
        {
            foreach (Item item in this.Output.items.Where(p => p != null))
            {
                // ignore gems which change Junimo colors (see JunimoHut:getGemColor)
                if (!this.PullGemstonesFromJunimoHuts && item.Category is SObject.GemCategory or SObject.mineralsCategory)
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
