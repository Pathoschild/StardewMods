using System.Collections.Generic;
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
            ["*"] = new()
            {
                GrowCrops = true,
                GrowCropsOutOfSeason = true,
                ForceTillable = new()
                {
                    Dirt = true,
                    Grass = true
                }
            }
        };


        /*********
        ** Public methods
        *********/
        /// <summary>Normalize the model after it's deserialized.</summary>
        /// <param name="context">The deserialization context.</param>
        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            this.Locations ??= new Dictionary<string, PerLocationConfig>();

            // remove null values
            foreach (string key in this.Locations.Where(p => p.Value == null).Select(p => p.Key).ToArray())
                this.Locations.Remove(key);
        }
    }
}
