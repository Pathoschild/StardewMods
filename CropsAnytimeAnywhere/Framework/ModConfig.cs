using System.Collections.Generic;
using System.Linq;
using StardewValley;

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


        /*********
        ** Public methods
        *********/
        /// <summary>Get the location config that applies for a given location name.</summary>
        /// <param name="location">The location.</param>
        public PerLocationConfig GetLocationConfig(GameLocation location)
        {
            return
                (
                    from entry in this.InLocations
                    where this.AppliesTo(entry.Key, location)
                    select entry.Value
                )
                .LastOrDefault();
        }

        /// <summary>Get whether this config applies to the given location.</summary>
        /// <param name="key">The per-location key.</param>
        /// <param name="location">The location instance.</param>
        public bool AppliesTo(string key, GameLocation location)
        {
            key = key.ToLower();
            string name = location.Name?.ToLower();
            string uniqueName = location.NameOrUniqueName?.ToLower();

            switch (key)
            {
                case "*":
                    return true;

                case "indoor":
                case "indoors":
                    return !location.IsOutdoors;

                case "outdoor":
                case "outdoors":
                    return location.IsOutdoors;

                default:
                    return key == name || key == uniqueName;
            }
        }
    }
}
