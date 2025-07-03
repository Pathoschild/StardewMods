namespace Pathoschild.Stardew.TractorMod.Framework;

/// <summary>Provides constants for the mod implementation.</summary>
internal static class ModConstants
{
    /// <summary>The maximum recommended tool effect distance.</summary>
    public const int MaxRecommendedDistance = 15;

    /// <summary>The number of ticks during which the distance should be highlighted after temporarily increasing or reducing it.</summary>
    public const int HighlightDistanceTicks = 2 * 60;

    /// <summary>The amount by which the distance highlight's opacity fades each tick when it's temporarily displayed.</summary>
    public const float HighlightDistanceFade = 0.05f;
}
