using Newtonsoft.Json;

namespace Pathoschild.Stardew.CropsAnytimeAnywhere.Framework;

/// <summary>Per-location mod configuration.</summary>
internal class PlantRule
{
    /*********
    ** Accessors
    *********/
    /// <summary>Whether crops can grow here.</summary>
    public bool GrowCrops { get; }

    /// <summary>Whether out-of-season crops grow here too.</summary>
    public bool GrowCropsOutOfSeason { get; }

    /// <summary>Whether to allow hoeing anywhere.</summary>
    public TillableRule ForceTillable { get; }

    /// <summary>Whether fruit trees match the calendar season when drawn, even if they produce fruit per <see cref="GrowCropsOutOfSeason"/>.</summary>
    public bool UseFruitTreesSeasonalSprites { get; }


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="growCrops">Whether crops can grow here.</param>
    /// <param name="growCropsOutOfSeason">Whether out-of-season crops grow here too.</param>
    /// <param name="useFruitTreesSeasonalSprites">Whether fruit trees match the calendar season when drawn, even if they produce fruit per <paramref name="growCropsOutOfSeason"/>.</param>
    /// <param name="forceTillable">Whether to allow hoeing anywhere.</param>
    [JsonConstructor]
    public PlantRule(bool growCrops, bool growCropsOutOfSeason, bool useFruitTreesSeasonalSprites, TillableRule? forceTillable)
    {
        this.GrowCrops = growCrops;
        this.GrowCropsOutOfSeason = growCropsOutOfSeason;
        this.ForceTillable = forceTillable ?? new(
            dirt: true,
            grass: true,
            stone: false,
            other: false
        );
        this.UseFruitTreesSeasonalSprites = useFruitTreesSeasonalSprites;
    }

    /// <summary>Construct an instance.</summary>
    /// <param name="config">The config instance to copy.</param>
    public PlantRule(PlantRule config)
        : this(config.GrowCrops, config.GrowCropsOutOfSeason, config.UseFruitTreesSeasonalSprites, new TillableRule(config.ForceTillable)) { }
}
