using System;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.GameData.Machines;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines
{
    /// <summary>An object that accepts input and provides output based on the rules in <see cref="DataLoader.Machines"/>.</summary>
    internal class DataBasedMachine : GenericObjectMachine<SObject>
    {
        /*********
        ** Fields
        *********/
        /// <summary>The minimum machine processing time in minutes for which to apply fairy dust.</summary>
        private readonly Func<int> MinMinutesForFairyDust;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        /// <param name="location">The location containing the machine.</param>
        /// <param name="tile">The tile covered by the machine.</param>
        /// <param name="minMinutesForFairyDust">The minimum machine processing time in minutes for which to apply fairy dust.</param>
        public DataBasedMachine(SObject machine, GameLocation location, Vector2 tile, Func<int> minMinutesForFairyDust)
            : base(machine, location, tile, DataBasedMachine.GetMachineId(machine.Name))
        {
            this.MinMinutesForFairyDust = minMinutesForFairyDust;
        }

        /// <inheritdoc />
        public override bool SetInput(IStorage input)
        {
            SObject machine = this.Machine;

            // skip if no input needed
            if (!machine.HasContextTag("machine_input"))
                return false;

            // add machine input
            bool addedInput = false;
            foreach (IContainer container in input.OutputContainers)
            {
                if (machine.AttemptAutoLoad(container.Inventory, Game1.player))
                {
                    addedInput = true;
                    break;
                }
            }

            // apply fairy dust
            if (addedInput)
                this.TryApplyFairyDust(input);

            return addedInput;
        }

        /// <summary>Get the output item.</summary>
        /// <remarks>This implementation is based on <see cref="SObject.CheckForActionOnMachine"/>.</remarks>
        public override ITrackedStack? GetOutput()
        {
            SObject machine = this.Machine;
            MachineData? machineData = machine.GetMachineData();

            // recalculate output if needed (e.g. bee house honey)
            if (machine.lastOutputRuleId.Value != null && machineData != null)
            {
                MachineOutputRule? outputRule = machineData.OutputRules?.FirstOrDefault(p => p.Id == machine.lastOutputRuleId.Value);
                if (outputRule?.RecalculateOnCollect == true)
                {
                    var prevOutput = machine.heldObject.Value;
                    machine.heldObject.Value = null;

                    machine.OutputMachine(machineData, outputRule, machine.lastInputItem.Value, null, machine.Location, false);

                    if (machine.heldObject.Value == null)
                        machine.heldObject.Value = prevOutput;
                }
            }

            // get output
            return this.GetTracked(this.Machine.heldObject.Value, onEmpty: this.OnOutputCollected);
        }

        /// <summary>Get a machine ID for a machine item.</summary>
        /// <param name="name">The machine's internal item.</param>
        public static string GetMachineId(string name)
        {
            return new string(name.Where(char.IsLetterOrDigit).ToArray());
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Reset the machine, so it's ready to accept a new input.</summary>
        /// <param name="item">The output item that was taken.</param>
        /// <remarks>This implementation is based on <see cref="SObject.CheckForActionOnMachine"/>.</remarks>
        protected void OnOutputCollected(Item item)
        {
            SObject machine = this.Machine;
            MachineData? machineData = machine.GetMachineData();

            // update stats
            MachineDataUtility.UpdateStats(machineData?.StatsToIncrementWhenHarvested, item, item.Stack);

            // reset machine data
            // This needs to happen before the OutputCollected check, which may start producing a new output.
            machine.heldObject.Value = null;
            machine.readyForHarvest.Value = false;
            machine.showNextIndex.Value = false;
            machine.ResetParentSheetIndex();

            // apply OutputCollected rule
            if (MachineDataUtility.TryGetMachineOutputRule(machine, machineData, MachineOutputTrigger.OutputCollected, item.getOne(), null, machine.Location, out MachineOutputRule outputCollectedRule, out _, out _, out _))
                machine.OutputMachine(machineData, outputCollectedRule, machine.lastInputItem.Value, null, machine.Location, false);

            // update tapper
            if (machine.IsTapper())
            {
                if (machine.Location.terrainFeatures.TryGetValue(machine.TileLocation, out TerrainFeature terrainFeature) && terrainFeature is Tree tree)
                    tree.UpdateTapperProduct(machine, item as SObject);
            }

            // grant any experience
            if (machineData?.ExperienceGainOnHarvest != null)
            {
                string[] expSplit = machineData.ExperienceGainOnHarvest.Split(' ');
                for (int i = 0; i < expSplit.Length + 1; i += 2)
                {
                    int skill = Farmer.getSkillNumberFromName(expSplit[i]);
                    if (skill != -1 && expSplit.Length > i + 1)
                    {
                        if (int.TryParse(expSplit[i + 1], out int amount))
                            Game1.player.gainExperience(skill, amount);
                    }
                }
            }
        }

        /// <summary>Apply fairy dust from the given containers if needed.</summary>
        /// <param name="input">The input to search for containers.</param>
        private void TryApplyFairyDust(IStorage input)
        {
            SObject machine = this.Machine;
            int minMinutes = Math.Max(10, this.MinMinutesForFairyDust());

            if (machine.MinutesUntilReady < minMinutes || !machine.TryApplyFairyDust(probe: true))
                return;

            int maxToApply = 3;
            foreach (IContainer container in input.OutputContainers)
            {
                while (maxToApply > 0 && container.Inventory.ContainsId("(O)872"))
                {
                    if (!machine.TryApplyFairyDust())
                        return;

                    container.Inventory.ReduceId("(O)872", 1);
                    maxToApply--;

                    if (machine.MinutesUntilReady < minMinutes || !machine.TryApplyFairyDust(probe: true))
                        return;
                }
            }
        }
    }
}
