using System.Collections.Generic;
using StardewModdingAPI;
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.Common.Integrations.MultiFertilizer
{
    /// <summary>Handles the logic for integrating with the MultiFertilizer mod.</summary>
    internal class MultiFertilizerIntegration : BaseIntegration
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        public MultiFertilizerIntegration(IModRegistry modRegistry, IMonitor monitor)
            : base("MultiFertilizer", "spacechase0.MultiFertilizer", "1.0.2", modRegistry, monitor)
        {
            if (!this.IsLoaded)
                return;
        }

        /// <summary>Get the fertilizer item IDs applied to a dirt tile.</summary>
        /// <param name="dirt">The dirt tile to check.</param>
        /// <remarks>See <a href="https://github.com/spacechase0/StardewValleyMods/tree/develop/MultiFertilizer#for-mod-authors" /> for details.</remarks>
        public IEnumerable<int> GetAppliedFertilizers(HoeDirt dirt)
        {
            if (!this.IsLoaded)
                yield break;

            if (dirt.fertilizer.Value > 0)
                yield return dirt.fertilizer.Value;

            foreach (string key in new[] { "FertilizerLevel", "SpeedGrowLevel", "WaterRetainLevel" })
            {
                if (dirt.modData.TryGetValue($"spacechase0.MultiFertilizer/{key}", out string rawValue) && int.TryParse(rawValue, out int level))
                {
                    int fertilizer = $"{key}:{level}" switch
                    {
                        "FertilizerLevel:1" => HoeDirt.fertilizerLowQuality,
                        "FertilizerLevel:2" => HoeDirt.fertilizerHighQuality,
                        "FertilizerLevel:3" => HoeDirt.fertilizerDeluxeQuality,

                        "SpeedGrowLevel:1" => HoeDirt.speedGro,
                        "SpeedGrowLevel:2" => HoeDirt.superSpeedGro,
                        "SpeedGrowLevel:3" => HoeDirt.hyperSpeedGro,

                        "WaterRetainLevel:1" => HoeDirt.waterRetentionSoil,
                        "WaterRetainLevel:2" => HoeDirt.waterRetentionSoilQuality,
                        "WaterRetainLevel:3" => HoeDirt.waterRetentionSoilDeluxe,

                        _ => -1
                    };
                    if (fertilizer > 0)
                        yield return fertilizer;
                }
            }
        }
    }
}
