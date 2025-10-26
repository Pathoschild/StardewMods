using System.Collections.Generic;
using StardewModdingAPI;

namespace Pathoschild.Stardew.Common.Integrations.TrainStation;

/// <summary>Handles the logic for integrating with the Train Station mod.</summary>
internal class TrainStationIntegration : BaseIntegration<ITrainStationApi>
{
    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
    /// <param name="monitor">Encapsulates monitoring and logging.</param>
    public TrainStationIntegration(IModRegistry modRegistry, IMonitor monitor)
        : base("Train Station", "Cherry.TrainStation", "2.2.0", modRegistry, monitor) { }

    /// <summary>Get the available stops from the player's current location.</summary>
    /// <param name="isBoat">Whether to get boat stops (<c>true</c>) or train stops (<c>false</c>).</param>
    public IEnumerable<ITrainStationStopModel> GetAvailableStops(bool isBoat)
    {
        this.AssertLoaded();

        return
            this.SafelyCallApi(
                api => api.GetAvailableStops(isBoat),
                "Failed getting stops from {0}."
            )
            ?? [];
    }
}
