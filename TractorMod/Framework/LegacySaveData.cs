namespace Pathoschild.Stardew.TractorMod.Framework
{
    /// <summary>Contains legacy data that's stored in the save file.</summary>
    /// <param name="Buildings">The custom buildings to save.</param>
    internal record LegacySaveData(LegacySaveDataBuilding[] Buildings);
}
