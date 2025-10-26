using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace Pathoschild.Stardew.Common.Integrations.BushBloomMod;

/// <summary>Handles the logic for integrating with the Custom Bush mod.</summary>
internal class BushBloomModIntegration : BaseIntegration<IBushBloomModApi>
{
    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
    /// <param name="monitor">Encapsulates monitoring and logging.</param>
    public BushBloomModIntegration(IModRegistry modRegistry, IMonitor monitor)
        : base("CustomBush", "NCarigon.BushBloomMod", "1.2.4", modRegistry, monitor) { }

    /// <inheritdoc cref="IBushBloomModApi.GetActiveSchedules" />
    public (string, WorldDate, WorldDate)[] GetActiveSchedules(string season, int dayOfMonth, int? year = null, GameLocation? location = null, Vector2? tile = null)
    {
        return
            this.SafelyCallApi(
                api => api.GetActiveSchedules(season, dayOfMonth, year, location, tile),
                "Failed fetching active schedules from {0}."
            )
            ?? [];
    }

    /// <inheritdoc cref="IBushBloomModApi.IsReady" />
    public bool IsReady()
    {
        return this.SafelyCallApi(
            api => api.IsReady(),
            "Failed checking if {0} is ready."
        );
    }
}
