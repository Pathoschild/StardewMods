using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.Common.Integrations.UltimateFertilizer;

/// <summary>Handles the logic for integrating with the Ultimate Fertilizer mod.</summary>
internal class UltimateFertilizerIntegration : BaseIntegration
{
    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
    /// <param name="monitor">Encapsulates monitoring and logging.</param>
    public UltimateFertilizerIntegration(IModRegistry modRegistry, IMonitor monitor)
        : base("Ultimate Fertilizer", "fox_white25.ultimate_fertilizer", "1.2.5", modRegistry, monitor) { }

    /// <summary>Get the fertilizer item IDs applied to a dirt tile.</summary>
    /// <param name="dirt">The dirt tile to check.</param>
    /// <remarks>See <a href="https://github.com/spacechase0/StardewValleyMods/tree/develop/MultiFertilizer#for-mod-authors">MultiFertilizer's mod author docs</a> for details.</remarks>
    public IEnumerable<string> GetAppliedFertilizers(HoeDirt dirt)
    {
        if (!this.IsLoaded || string.IsNullOrWhiteSpace(dirt.fertilizer.Value))
            yield break;

        foreach (string itemId in dirt.fertilizer.Value.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            yield return itemId;
    }
}
