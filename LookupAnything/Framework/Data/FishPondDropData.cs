using System;

namespace Pathoschild.Stardew.LookupAnything.Framework.Data
{
    /// <summary>An item that can be produced by a fish pond.</summary>
    internal record FishPondDropData : ItemDropData
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The minimum population needed for the item to drop.</summary>
        public int MinPopulation { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="minPopulation">The minimum population needed for the item to drop.</param>
        /// <param name="itemID">The unqualified item ID.</param>
        /// <param name="minDrop">The minimum number to drop.</param>
        /// <param name="maxDrop">The maximum number to drop.</param>
        /// <param name="probability">The probability that the item will be dropped.</param>
        public FishPondDropData(int minPopulation, string itemID, int minDrop, int maxDrop, float probability)
            : base(itemID, minDrop, maxDrop, probability)
        {
            this.MinPopulation = Math.Max(minPopulation, 1); // rule only applies if the pond has at least one fish, so assume minimum of 1 to avoid player confusion
        }
    }
}
