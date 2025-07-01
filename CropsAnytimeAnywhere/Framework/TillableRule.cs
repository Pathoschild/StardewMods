using System.Collections.Generic;
using Newtonsoft.Json;
using StardewValley;

namespace Pathoschild.Stardew.CropsAnytimeAnywhere.Framework;

/// <summary>A rule which sets the tiles to force tillable if it matches.</summary>
internal class TillableRule : BaseRule
{
    /*********
    ** Accessors
    *********/
    /// <summary>Whether to allow tilling dirt tiles not normally allowed by the game.</summary>
    public bool Dirt { get; }

    /// <summary>Whether to allow tilling grass tiles.</summary>
    public bool Grass { get; }

    /// <summary>Whether to allow tilling stone tiles.</summary>
    public bool Stone { get; }

    /// <summary>Whether to allow tilling other tile types (like paths, indoor floors, etc).</summary>
    public bool Other { get; }


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="conditions">The rule whose conditions to copy.</param>
    /// <param name="dirt"><inheritdoc cref="Dirt" path="/summary"/></param>
    /// <param name="grass"><inheritdoc cref="Grass" path="/summary"/></param>
    /// <param name="stone"><inheritdoc cref="Stone" path="/summary"/></param>
    /// <param name="other"><inheritdoc cref="Other" path="/summary"/></param>
    public TillableRule(BaseRule conditions, bool dirt, bool grass, bool stone, bool other)
        : this(conditions.ForLocations, conditions.ForLocationContexts, conditions.ForSeasons, dirt, grass, stone, other) { }

    /// <summary>Construct an instance.</summary>
    /// <param name="forLocations"><inheritdoc cref="BaseRule.ForLocations" path="/summary"/></param>
    /// <param name="forLocationContexts"><inheritdoc cref="BaseRule.ForLocationContexts" path="/summary"/></param>
    /// <param name="forSeasons"><inheritdoc cref="BaseRule.ForSeasons" path="/summary"/></param>
    /// <param name="dirt"><inheritdoc cref="Dirt" path="/summary"/></param>
    /// <param name="grass"><inheritdoc cref="Grass" path="/summary"/></param>
    /// <param name="stone"><inheritdoc cref="Stone" path="/summary"/></param>
    /// <param name="other"><inheritdoc cref="Other" path="/summary"/></param>
    [JsonConstructor]
    public TillableRule(HashSet<string>? forLocations, HashSet<string>? forLocationContexts, HashSet<Season>? forSeasons, bool dirt, bool grass, bool stone, bool other)
        : base(forLocations, forLocationContexts, forSeasons)
    {
        this.Dirt = dirt;
        this.Grass = grass;
        this.Stone = stone;
        this.Other = other;
    }

    /// <summary>Whether any of the options are enabled.</summary>
    public bool IsAnyEnabled()
    {
        return
            this.Dirt
            || this.Grass
            || this.Stone
            || this.Other;
    }
}
