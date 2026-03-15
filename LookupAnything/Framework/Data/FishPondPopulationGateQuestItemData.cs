namespace Pathoschild.Stardew.LookupAnything.Framework.Data;

/// <summary>An item required to unlock a fish pond population gate.</summary>
/// <param name="ItemId">The item ID.</param>
/// <param name="MinCount">The minimum number of the item that may be requested.</param>
/// <param name="MaxCount">The maximum number of the item that may be requested.</param>
internal record FishPondPopulationGateQuestItemData(string ItemId, int MinCount, int MaxCount);
