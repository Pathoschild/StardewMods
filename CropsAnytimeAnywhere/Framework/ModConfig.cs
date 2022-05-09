using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;

namespace Pathoschild.Stardew.CropsAnytimeAnywhere.Framework
{
    /// <summary>The mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The per-location settings.</summary>
        public IDictionary<string, PerLocationConfig> Locations { get; set; } = new Dictionary<string, PerLocationConfig>
        {
            ["*"] = new(
                growCrops: true,
                growCropsOutOfSeason: true,
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
        [SuppressMessage("ReSharper", "ConstantNullCoalescingCondition", Justification = "This method enforces the expected nullability.")]
        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse", Justification = "This method enforces the expected nullability.")]
        public void OnDeserialized(StreamingContext context)
        {
            this.Locations ??= new Dictionary<string, PerLocationConfig>();

            // remove null values
            foreach (string key in this.Locations.Where(p => p.Value == null).Select(p => p.Key).ToArray())
                this.Locations.Remove(key);
        }
    }
}
