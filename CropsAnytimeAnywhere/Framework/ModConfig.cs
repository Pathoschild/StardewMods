using System.Collections.Generic;

namespace Pathoschild.Stardew.CropsAnytimeAnywhere.Framework
{
    /// <summary>The mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The per-location settings.</summary>
        public IDictionary<string, PerLocationConfig> InLocations { get; set; } = new Dictionary<string, PerLocationConfig>
        {
            ["*"] = new PerLocationConfig
            {
                GrowCrops = true,
                GrowCropsOutOfSeason = true
            }
        };

        /// <summary>Whether to allow hoeing anywhere.</summary>
        public ModConfigForceTillable ForceTillable { get; set; } = new ModConfigForceTillable();
    }
}
