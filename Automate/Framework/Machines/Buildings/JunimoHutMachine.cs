using System.Linq;
using Pathoschild.Stardew.Automate.Framework.Models;
using StardewValley;
using StardewValley.Buildings;
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
        /// <summary>How to handle gems in the hut or connected chests.</summary>
        private readonly JunimoHutBehavior GemBehavior;

        /// <summary>How to handle fertilizer in the hut or connected chests.</summary>
        private readonly JunimoHutBehavior FertilizerBehavior;

        /// <summary>How to handle seeds in the hut or connected chests.</summary>
        private readonly JunimoHutBehavior SeedBehavior;

        /// <summary>Whether the Junimo hut can automate input.</summary>
        private readonly bool HasInput;

        /// <summary>Whether any items are configured to be skipped when outputting.</summary>
        private readonly bool HasIgnoredOutput;

        /// <summary>The Junimo hut's output chest.</summary>
        private Chest Output => this.Machine.GetOutputChest();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="hut">The underlying Junimo hut.</param>
        /// <param name="location">The location which contains the machine.</param>
        /// <param name="gemBehavior">How to handle gems in the hut or connected chests.</param>
        /// <param name="fertilizerBehavior">How to handle fertilizer in the hut or connected chests.</param>
        /// <param name="seedBehavior">How to handle seeds in the hut or connected chests.</param>
        public JunimoHutMachine(JunimoHut hut, GameLocation location, JunimoHutBehavior gemBehavior, JunimoHutBehavior fertilizerBehavior, JunimoHutBehavior seedBehavior)
            : base(hut, location, BaseMachine.GetTileAreaFor(hut))
        {
            this.GemBehavior = gemBehavior;
            this.FertilizerBehavior = fertilizerBehavior;
            this.SeedBehavior = seedBehavior;

            this.HasInput =
                gemBehavior is JunimoHutBehavior.MoveIntoHut
                || fertilizerBehavior is JunimoHutBehavior.MoveIntoHut
                || seedBehavior is JunimoHutBehavior.MoveIntoHut;
            this.HasIgnoredOutput =
                gemBehavior is not JunimoHutBehavior.MoveIntoChests
                || fertilizerBehavior is not JunimoHutBehavior.MoveIntoChests
                || seedBehavior is not JunimoHutBehavior.MoveIntoChests;
        }

        /// <summary>Get the machine's processing state.</summary>
        public override MachineState GetState()
        {
            if (this.Machine.isUnderConstruction())
                return MachineState.Disabled;

            if (this.GetNextOutput() != null)
                return MachineState.Done;

            return this.HasInput
                ? MachineState.Empty
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
            if (!this.HasInput)
                return false;

            // get next item
            ITrackedStack? tracker = null;
            foreach (ITrackedStack stack in input.GetItems())
            {
                if (stack.Sample is not SObject obj)
                    continue;

                switch (obj.Category)
                {
                    case SObject.SeedsCategory when this.SeedBehavior == JunimoHutBehavior.MoveIntoHut:
                        tracker = stack;
                        break;

                    case SObject.fertilizerCategory when this.FertilizerBehavior == JunimoHutBehavior.MoveIntoHut:
                        tracker = stack;
                        break;

                    case (SObject.GemCategory or SObject.mineralsCategory) when this.GemBehavior == JunimoHutBehavior.MoveIntoHut:
                        tracker = stack;
                        break;
                }

                if (tracker is not null)
                    break;
            }
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
            this.Output.Items.Remove(item);
        }

        /// <summary>Get the next output item.</summary>
        private Item? GetNextOutput()
        {
            foreach (Item item in this.Output.Items.Where(p => p != null))
            {
                if (this.HasIgnoredOutput)
                {
                    bool ignore = false;

                    switch (item.Category)
                    {
                        case SObject.SeedsCategory when this.SeedBehavior is not JunimoHutBehavior.MoveIntoChests:
                        case SObject.fertilizerCategory when this.FertilizerBehavior is not JunimoHutBehavior.MoveIntoChests:
                        case (SObject.GemCategory or SObject.mineralsCategory) when this.GemBehavior is not JunimoHutBehavior.MoveIntoChests:
                            ignore = true;
                            break;
                    }

                    if (ignore)
                        continue;
                }

                return item;
            }

            return null;
        }
    }
}
