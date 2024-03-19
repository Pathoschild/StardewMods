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
            : base("MultiFertilizer", "spacechase0.MultiFertilizer", "1.0.2", modRegistry, monitor) { }

        /// <summary>Get the fertilizer item IDs applied to a dirt tile.</summary>
        /// <param name="dirt">The dirt tile to check.</param>
        /// <remarks>See <a href="https://github.com/spacechase0/StardewValleyMods/tree/develop/MultiFertilizer#for-mod-authors">MultiFertilizer's mod author docs</a> for details.</remarks>
        public IEnumerable<string> GetAppliedFertilizers(HoeDirt dirt)
        {
            if (!this.IsLoaded)
                yield break;

            if (CommonHelper.IsItemId(dirt.fertilizer.Value, allowZero: false))
                yield return dirt.fertilizer.Value;

            foreach (string key in new[] { "FertilizerLevel", "SpeedGrowLevel", "WaterRetainLevel" })
            {
                if (dirt.modData.TryGetValue($"spacechase0.MultiFertilizer/{key}", out string rawValue) && int.TryParse(rawValue, out int level))
                {
                    string? fertilizer = $"{key}:{level}" switch
                    {
                        "FertilizerLevel:1" => HoeDirt.fertilizerLowQualityQID,
                        "FertilizerLevel:2" => HoeDirt.fertilizerHighQualityQID,
                        "FertilizerLevel:3" => HoeDirt.fertilizerDeluxeQualityQID,

                        "SpeedGrowLevel:1" => HoeDirt.speedGroQID,
                        "SpeedGrowLevel:2" => HoeDirt.superSpeedGroQID,
                        "SpeedGrowLevel:3" => HoeDirt.hyperSpeedGroQID,

                        "WaterRetainLevel:1" => HoeDirt.waterRetentionSoilQID,
                        "WaterRetainLevel:2" => HoeDirt.waterRetentionSoilQualityQID,
                        "WaterRetainLevel:3" => HoeDirt.waterRetentionSoilDeluxeQID,

                        _ => null
                    };
                    if (fertilizer != null)
                        yield return fertilizer;
                }
            }
        }
    }
}
