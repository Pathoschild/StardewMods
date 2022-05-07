namespace Pathoschild.Stardew.LookupAnything.Framework.Models.FishData
{
    /// <summary>A time of day range.</summary>
    /// <param name="MinTime">The minimum time of day.</param>
    /// <param name="MaxTime">The maximum time of day.</param>
    internal record FishSpawnTimeOfDayData(int MinTime, int MaxTime);
}
