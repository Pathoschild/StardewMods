using Microsoft.Xna.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Objects
{
    /// <summary>A crystalarium that accepts input and provides output.</summary>
    /// <remarks>Derived from <see cref="SObject.performObjectDropInAction"/> (search for 'Crystalarium').</remarks>
    internal class CrystalariumMachine : GenericObjectMachine<SObject>
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying machine.</param>
        /// <param name="location">The location containing the machine.</param>
        /// <param name="tile">The tile covered by the machine.</param>
        public CrystalariumMachine(SObject machine, GameLocation location, Vector2 tile)
            : base(machine, location, tile) { }

        /// <summary>Get the machine's processing state.</summary>
        public override MachineState GetState()
        {
            return this.GetGenericState(emptyState: MachineState.Disabled);
        }

        /// <summary>Get the output item.</summary>
        public override ITrackedStack? GetOutput()
        {
            SObject machine = this.Machine;
            Item? output = machine.heldObject.Value?.getOne();

            return this.GetTracked(output, onEmpty: _ =>
            {
                machine.MinutesUntilReady = output.QualifiedItemId switch
                {
                    // temporarily taken from Data/Machines until Automate is updated to use it
                    "(O)80" => 420,
                    "(O)60" => 3000,
                    "(O)68" => 1120,
                    "(O)70" => 2400,
                    "(O)64" => 3000,
                    "(O)62" => 2240,
                    "(O)66" => 1360,
                    "(O)72" => 7200,
                    "(O)82" => 1300,
                    "(O)84" => 1120,
                    "(O)86" => 800,
                    _ => 5000
                };
                machine.readyForHarvest.Value = false;
            });
        }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool SetInput(IStorage input)
        {
            return false; // started manually
        }
    }
}
