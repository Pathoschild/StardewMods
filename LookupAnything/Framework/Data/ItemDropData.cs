namespace Pathoschild.Stardew.LookupAnything.Framework.Data
{
    /// <summary>A loot entry parsed from the game data.</summary>
    /// <param name="ItemID">The item's parent sprite index.</param>
    /// <param name="MinDrop">The minimum number to drop.</param>
    /// <param name="MaxDrop">The maximum number to drop.</param>
    /// <param name="Probability">The probability that the item will be dropped.</param>
    internal record ItemDropData(int ItemID, int MinDrop, int MaxDrop, float Probability);
}
