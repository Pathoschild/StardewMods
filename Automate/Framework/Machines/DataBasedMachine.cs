using System;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;
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

        /// <summary>Get a machine ID for a machine item.</summary>
        /// <param name="name">The machine's internal item.</param>
        public static string GetMachineId(string name)
        {
            return new string(name.Where(char.IsLetterOrDigit).ToArray());
        }


        /*********
        ** Private methods
        *********/
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
