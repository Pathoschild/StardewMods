using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Pathoschild.Stardew.Common;
using StardewValley.Extensions;

namespace Pathoschild.Stardew.CropsAnytimeAnywhere.Framework;

/// <summary>The mod configuration.</summary>
internal class ModConfig
{
    /*********
    ** Accessors
    *********/
    /// <summary>Where and when plants can be grown. The first matching rule is applied.</summary>
    public List<PlantRule> PlantRules { get; set; } =
    [
        new(
            forLocations: [],
            forLocationContexts: [],
            forSeasons: [],
            canPlant: true,
            canGrowOutOfSeason: true,
            useFruitTreesSeasonalSprites: false
        )
    ];

    /// <summary>Which tiles to mark tillable. The first matching rule is applied.</summary>
    public List<TillableRule> TillableRules { get; set; } =
    [
        new(
            forLocations: [],
            forLocationContexts: [],
            forSeasons: [],
            dirt: true,
            grass: true,
            stone: false,
            other: false
        )
    ];


    /*********
    ** Public methods
    *********/
    /// <summary>Normalize the model after it's deserialized.</summary>
    /// <param name="context">The deserialization context.</param>
    [OnDeserialized]
    [SuppressMessage("ReSharper", "ConditionalAccessQualifierIsNonNullableAccordingToAPIContract", Justification = SuppressReasons.MethodValidatesNullability)]
    [SuppressMessage("ReSharper", "NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract", Justification = SuppressReasons.MethodValidatesNullability)]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = SuppressReasons.UsedViaOnDeserialized)]
    public void OnDeserialized(StreamingContext context)
    {
        this.PlantRules?.RemoveWhere(p => p is null);
        this.PlantRules ??= [];

        this.TillableRules?.RemoveWhere(p => p is null);
        this.TillableRules ??= [];
    }
}
