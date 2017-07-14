using System.Linq;
using Pathoschild.Stardew.Automate.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Machines.Tiles
{
    /// <summary>A shipping bin that accepts input and provides output.</summary>
    internal class ShippingBinMachine : IMachine
    {
        /*********
        ** Properties
        *********/
        /// <summary>The farm containing the shipping bin.</summary>
        private readonly Farm Farm;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="farm">The farm containing the shipping bin.</param>
        public ShippingBinMachine(Farm farm)
        {
            this.Farm = farm;
        }

        /// <summary>Get the machine's processing state.</summary>
        public MachineState GetState()
        {
            return MachineState.Empty; // always accepts items
        }

        /// <summary>Get the output item.</summary>
        public ITrackedStack GetOutput()
        {
            return null; // no output
        }

        /// <summary>Pull items from the connected pipes.</summary>
        /// <param name="pipes">The connected IO pipes.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public bool Pull(IPipe[] pipes)
        {
            ITrackedStack tracker = pipes.GetItems(p => p.Sample is SObject obj && obj.canBeShipped()).Take(1).FirstOrDefault();
            if (tracker != null)
            {
                SObject item = (SObject)tracker.Take(tracker.Count);
                this.Farm.shippingBin.Add(item);
                this.Farm.lastItemShipped = item;
                this.Farm.showShipment(item, false);
                return true;
            }
            return false;
        }
    }
}
