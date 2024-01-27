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
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        /// <param name="location">The location containing the machine.</param>
        /// <param name="tile">The tile covered by the machine.</param>
        public DataBasedMachine(SObject machine, GameLocation location, Vector2 tile)
            : base(machine, location, tile, GetMachineId(machine)) { }

        /// <inheritdoc />
        public override bool SetInput(IStorage input)
        {
            if (!this.Machine.HasContextTag("machine_input"))
                return false;

            foreach (IContainer container in input.InputContainers)
            {
                if (this.Machine.AttemptAutoLoad(container.Inventory, Game1.player))
                    return true;
            }

            return false;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get a machine ID for a machine item.</summary>
        /// <param name="machine">The machine item.</param>
        private static string GetMachineId(SObject machine)
        {
            return new string(machine.Name.Where(char.IsLetterOrDigit).ToArray());
        }
    }
}
