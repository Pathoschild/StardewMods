using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Automate.Framework.Machines.Objects;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Buildings
{
    /// <summary>A shipping bin that accepts input.</summary>
    /// <remarks>See also <see cref="MiniShippingBinMachine"/>.</remarks>
    internal class ShippingBinMachine : BaseMachine
    {
        /*********
        ** Fields
        *********/
        /// <summary>The constructed shipping bin, if applicable.</summary>
        private readonly ShippingBin Bin;


        /*********
        ** Accessors
        *********/
        /// <summary>Get the unique ID for the shipping bin machine.</summary>
        internal static string ShippingBinId { get; } = BaseMachine.GetDefaultMachineId(typeof(ShippingBinMachine));


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="location">The machine's in-game location.</param>
        /// <param name="tileArea">The tile area covered by the machine.</param>
        public ShippingBinMachine(GameLocation location, Rectangle tileArea)
            : base(location, tileArea, ShippingBinMachine.ShippingBinId)
        {
            this.Bin = null;
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="bin">The constructed shipping bin.</param>
        /// <param name="location">The location which contains the machine.</param>
        public ShippingBinMachine(ShippingBin bin, GameLocation location)
            : base(location, BaseMachine.GetTileAreaFor(bin), ShippingBinMachine.ShippingBinId)
        {
            this.Bin = bin;
        }

        /// <summary>Get the machine's processing state.</summary>
        public override MachineState GetState()
        {
            if (this.Bin?.isUnderConstruction() == true)
                return MachineState.Disabled;

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
            // get next item
            ITrackedStack tracker = input.GetItems().FirstOrDefault(p => p.Sample is SObject obj && obj.canBeShipped());
            if (tracker == null)
                return false;

            // ship item
            SObject item = (SObject)tracker.Take(tracker.Count);
            var binList = (this.Location as Farm ?? Game1.getFarm()).getShippingBin(Game1.MasterPlayer);
            Utility.addItemToThisInventoryList(item, binList, listMaxSpace: int.MaxValue);

            // play animation/sound
            if (this.Bin != null)
                this.Bin.showShipment(item, false);
            else if (this.Location is IslandWest islandFarm)
                islandFarm.showShipment(item, false);
            else if (this.Location is Farm farm)
                farm.showShipment(item, false);

            return true;
        }
    }
}
