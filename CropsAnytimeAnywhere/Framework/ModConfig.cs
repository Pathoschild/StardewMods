using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Pathoschild.Stardew.Common;
using StardewValley.Extensions;

namespace Pathoschild.Stardew.CropsAnytimeAnywhere.Framework
{
    /// <summary>The mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The per-location settings.</summary>
        public Dictionary<string, PerLocationConfig> Locations { get; set; } = new()
        {
            ["*"] = new(
                growCrops: true,
                growCropsOutOfSeason: true,
                useFruitTreesSeasonalSprites: false,
                forceTillable: new(
                    dirt: true,
                    grass: true,
                    stone: false,
                    other: false
                )
            )
        };


        /*********
        ** Public methods
        *********/
        /// <summary>Normalize the model after it's deserialized.</summary>
        /// <param name="context">The deserialization context.</param>
        [OnDeserialized]
        [SuppressMessage("ReSharper", "NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract", Justification = SuppressReasons.MethodValidatesNullability)]
        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = SuppressReasons.UsedViaOnDeserialized)]
        public void OnDeserialized(StreamingContext context)
        {
            this.Locations ??= new Dictionary<string, PerLocationConfig>();
            this.Locations.RemoveWhere(p => p.Value is null);
        }
    }
}
