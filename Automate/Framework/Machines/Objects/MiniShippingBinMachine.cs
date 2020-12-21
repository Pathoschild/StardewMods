using System.Linq;
using Pathoschild.Stardew.Automate.Framework.Machines.Buildings;
using Pathoschild.Stardew.Automate.Framework.Storage;
using StardewValley;
using StardewValley.Objects;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Objects
{
    /// <summary>A mini-shipping bin that accepts input.</summary>
    /// <remarks>See also <see cref="ShippingBinMachine"/>.</remarks>
    internal class MiniShippingBinMachine : BaseMachine
    {
        /*********
        ** Fields
        *********/
        /// <summary>The mini-shipping bin.</summary>
        private readonly IContainer MiniBin;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="miniBin">The mini-shipping bin.</param>
        /// <param name="location">The location which contains the machine.</param>
        public MiniShippingBinMachine(Chest miniBin, GameLocation location)
            : base(location, BaseMachine.GetTileAreaFor(miniBin.TileLocation))
        {
            this.MiniBin = new ChestContainer(miniBin, location, miniBin.TileLocation, migrateLegacyOptions: false);
        }

        /// <summary>Get the machine's processing state.</summary>
        public override MachineState GetState()
        {
            return MachineState.Empty; // always accepts items
        }

        /// <summary>Get the output item.</summary>
        public override ITrackedStack GetOutput()
        {
            return null; // no output
        }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool SetInput(IStorage input)
        {
            foreach (ITrackedStack tracker in input.GetItems().Where(p => p.Sample is Object obj && obj.canBeShipped()))
            {
                int prevStack = tracker.Count;
                this.MiniBin.Store(tracker);
                if (prevStack > tracker.Count)
                    return true;
            }

            return false;
        }
    }
}
