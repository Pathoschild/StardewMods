using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.Buildings
{
    /// <summary>A shipping bin that accepts input and provides output.</summary>
    internal class ShippingBinMachine : BaseMachine
    {
        /*********
        ** Fields
        *********/
        /// <summary>The farm to automate.</summary>
        private readonly Farm Farm;

        /// <summary>The constructed shipping bin, if applicable.</summary>
        private readonly ShippingBin Bin;

        /// <summary>Minimum number of each item to keep.</summary>
        private readonly int KeepAtLeast;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="farm">The farm containing the shipping bin.</param>
        /// <param name="tileArea">The tile area covered by the machine.</param>
        /// <param name="keepAtLeast">Minimum number of each item to keep.</param>
        public ShippingBinMachine(Farm farm, Rectangle tileArea, int keepAtLeast)
            : base(farm, tileArea)
        {
            this.Farm = farm;
            this.Bin = null;
            this.KeepAtLeast = keepAtLeast;
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="bin">The constructed shipping bin.</param>
        /// <param name="location">The location which contains the machine.</param>
        /// <param name="farm">The farm which has the shipping bin data.</param>
        /// <param name="keepAtLeast">Minimum number of each item to keep.</param>
        public ShippingBinMachine(ShippingBin bin, GameLocation location, Farm farm, int keepAtLeast)
            : base(location, BaseMachine.GetTileAreaFor(bin))
        {
            this.Farm = farm;
            this.Bin = bin;
            this.KeepAtLeast = keepAtLeast;
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
            IDictionary<int, int> itemCounter = new SortedList<int, int>();
            Func<ITrackedStack,bool> predicate = p => p.Sample is SObject obj && obj.canBeShipped() && p.Count > KeepAtLeast;
            foreach (ITrackedStack tracker in input.GetItemsAscendingQuality(itemCounter, predicate))
            {
                int itemCount=0;
                if (KeepAtLeast > 0 && !itemCounter.TryGetValue(tracker.Sample.ParentSheetIndex, out itemCount))
                {
                    string error = $"Failed to retrieve item count #{tracker.Sample.ParentSheetIndex} ('{tracker.Sample.Name}')";
                    throw new InvalidOperationException(error);
                }

                if (KeepAtLeast <= 0 || itemCounter[tracker.Sample.ParentSheetIndex] > KeepAtLeast)
                {
                  SObject item = (SObject)tracker.Take(Math.Min(tracker.Count, itemCount-KeepAtLeast));
                  this.Farm.getShippingBin(Game1.MasterPlayer).Add(item);
                  this.Farm.lastItemShipped = item;
                  this.Farm.showShipment(item, false);
                  return true;
                }
            }
            return false;
        }
    }
}
