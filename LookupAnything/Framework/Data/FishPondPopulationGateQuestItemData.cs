namespace Pathoschild.Stardew.LookupAnything.Framework.Data
{
    /// <summary>An item required to unlock a fish pond population gate.</summary>
    /// <param name="ItemID">The item ID.</param>
    /// <param name="MinCount">The minimum number of the item that may be requested.</param>
    /// <param name="MaxCount">The maximum number of the item that may be requested.</param>
    internal record FishPondPopulationGateQuestItemData(string ItemID, int MinCount, int MaxCount);
}
