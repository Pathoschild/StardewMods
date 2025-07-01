using System.Collections.Generic;
using Newtonsoft.Json;
using StardewValley;

namespace Pathoschild.Stardew.CropsAnytimeAnywhere.Framework;

/// <summary>A rule which sets the plant growth options if it matches.</summary>
internal class PlantRule : BaseRule
{
    /*********
    ** Accessors
    *********/
    /// <summary>Whether crops can be planted here.</summary>
    public bool CanPlant { get; }

    /// <summary>Whether crops can grow here when out of season.</summary>
    public bool CanGrowOutOfSeason { get; }

    /// <summary>Whether fruit trees should match the calendar season when drawn, even if they produce fruit per <see cref="CanGrowOutOfSeason"/>.</summary>
    public bool UseFruitTreesSeasonalSprites { get; }


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="conditions">The rule whose conditions to copy.</param>
    /// <param name="canPlant"><inheritdoc cref="CanPlant" path="/summary"/></param>
    /// <param name="canGrowOutOfSeason"><inheritdoc cref="CanGrowOutOfSeason" path="/summary"/></param>
    /// <param name="useFruitTreesSeasonalSprites"><inheritdoc cref="UseFruitTreesSeasonalSprites" path="/summary"/></param>
    public PlantRule(BaseRule conditions, bool canPlant, bool canGrowOutOfSeason, bool useFruitTreesSeasonalSprites)
        : this(conditions.ForLocations, conditions.ForLocationContexts, conditions.ForSeasons, canPlant, canGrowOutOfSeason, useFruitTreesSeasonalSprites) { }

    /// <summary>Construct an instance.</summary>
    /// <param name="forLocations"><inheritdoc cref="BaseRule.ForLocations" path="/summary"/></param>
    /// <param name="forLocationContexts"><inheritdoc cref="BaseRule.ForLocationContexts" path="/summary"/></param>
    /// <param name="forSeasons"><inheritdoc cref="BaseRule.ForSeasons" path="/summary"/></param>
    /// <param name="canPlant"><inheritdoc cref="CanPlant" path="/summary"/></param>
    /// <param name="canGrowOutOfSeason"><inheritdoc cref="CanGrowOutOfSeason" path="/summary"/></param>
    /// <param name="useFruitTreesSeasonalSprites"><inheritdoc cref="UseFruitTreesSeasonalSprites" path="/summary"/></param>
    [JsonConstructor]
    public PlantRule(HashSet<string>? forLocations, HashSet<string>? forLocationContexts, HashSet<Season>? forSeasons, bool canPlant, bool canGrowOutOfSeason, bool useFruitTreesSeasonalSprites)
        : base(forLocations, forLocationContexts, forSeasons)
    {
        this.CanPlant = canPlant;
        this.CanGrowOutOfSeason = canGrowOutOfSeason;
        this.UseFruitTreesSeasonalSprites = useFruitTreesSeasonalSprites;
    }
}
