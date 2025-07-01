using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Pathoschild.Stardew.Common;
using StardewValley;

namespace Pathoschild.Stardew.CropsAnytimeAnywhere.Framework;

/// <summary>The base rule options.</summary>
internal abstract class BaseRule
{
    /*********
    ** Accessors
    *********/
    /// <summary>The locations where this rule applies. This can contain internal location names, <c>Indoors</c>, or <c>Outdoors</c>.</summary>
    public HashSet<string> ForLocations { get; }

    /// <summary>The location contexts where this rule applies.</summary>
    public HashSet<string> ForLocationContexts { get; }

    /// <summary>The calendar seasons for which this rule applies.</summary>
    public HashSet<Season> ForSeasons { get; }

    /// <summary>Whether this rule has any conditions.</summary>
    [JsonIgnore]
    public bool HasConditions { get; }


    /*********
    ** Protected methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="forLocations"><inheritdoc cref="ForLocations" path="/summary"/></param>
    /// <param name="forLocationContexts"><inheritdoc cref="ForLocationContexts" path="/summary"/></param>
    /// <param name="forSeasons"><inheritdoc cref="ForSeasons" path="/summary"/></param>
    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract", Justification = SuppressReasons.MethodValidatesNullability)]
    public BaseRule(HashSet<string>? forLocations, HashSet<string>? forLocationContexts, HashSet<Season>? forSeasons)
    {
        forLocations?.RemoveWhere(p => p is null);
        forLocationContexts?.RemoveWhere(p => p is null);

        this.ForLocations = forLocations.ToNonNullCaseInsensitive();
        this.ForLocationContexts = forLocationContexts.ToNonNullCaseInsensitive();
        this.ForSeasons = forSeasons ?? [];

        this.HasConditions =
            this.ForLocations.Count > 0
            || this.ForLocationContexts.Count > 0
            || this.ForSeasons.Count > 0;
    }

    /// <summary>Get whether this rule applies to the given location in the current season.</summary>
    /// <param name="location">The location to check.</param>
    public bool AppliesTo(GameLocation location)
    {
        if (this.HasConditions)
        {
            if (this.ForSeasons.Count > 0 && !this.ForSeasons.Contains(Game1.season))
                return false;

            if (this.ForLocations.Count > 0 && !this.ForLocations.Contains(location.NameOrUniqueName) && !this.ForLocations.Contains(location.Name) && !this.ForLocations.Contains(location.IsOutdoors ? "Indoors" : "Outdoors"))
                return false;

            if (this.ForLocationContexts.Count > 0 && !this.ForLocationContexts.Contains(location.GetLocationContextId()))
                return false;
        }

        return true;
    }
}
