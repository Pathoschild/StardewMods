namespace Pathoschild.Stardew.LookupAnything.Framework.Data
{
    /// <summary>An item required to unlock a fish pond population gate.</summary>
    internal class FishPondPopulationGateQuestItemData
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The item ID.</summary>
        public int ItemID { get; }

        /// <summary>The minimum number of the item that may be requested.</summary>
        public int MinCount { get; }

        /// <summary>The maximum number of the item that may be requested.</summary>
        public int MaxCount { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="itemID">The item ID.</param>
        /// <param name="minCount">The minimum number of the item that may be requested.</param>
        /// <param name="maxCount">The maximum number of the item that may be requested.</param>
        public FishPondPopulationGateQuestItemData(int itemID, int minCount, int maxCount)
        {
            this.ItemID = itemID;
            this.MinCount = minCount;
            this.MaxCount = maxCount;
        }
    }
}
