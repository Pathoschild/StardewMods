using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Pathoschild.Stardew.CentralStation.Framework.Integrations;
using Pathoschild.Stardew.CentralStation.Framework.Integrations.BusLocations;
using Pathoschild.Stardew.CentralStation.Framework.Integrations.TrainStation;
using StardewModdingAPI;
using StardewValley;

namespace Pathoschild.Stardew.CentralStation.Framework;

/// <summary>Manages the available destinations, including destinations provided through other frameworks like Train Station.</summary>
internal class StopManager
{
    /*********
    ** Fields
    *********/
    /// <summary>Manages the Central Station content provided by content packs.</summary>
    private readonly ContentManager ContentManager;

    /// <summary>Encapsulates monitoring and logging.</summary>
    private readonly IMonitor Monitor;

    /// <summary>The stop provider which provides compatibility with the Bus Locations mod.</summary>
    private readonly BusLocationsStopProvider BusLocationsProvider;

    /// <summary>The stop provider which provides compatibility with the Train Station mod.</summary>
    private readonly TrainStationStopProvider TrainStationStopProvider;

    /// <summary>The mod integrations which add stops to the Central Station networks.</summary>
    /// <remarks>Most code should use <see cref="GetCustomStopProviders"/> instead.</remarks>
    private List<ICustomStopProvider>? CustomStopProviders;


    /*********
    ** Public methods
    *********/
    /// <summary>The stops registered through the mod API.</summary>
    public Dictionary<string, Stop> ModApiStops { get; } = [];

    /// <summary>A network value which includes all network types.</summary>
    public const StopNetworks AllNetworks = StopNetworks.Boat | StopNetworks.Bus | StopNetworks.Train;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="contentManager"><inheritdoc cref="ContentManager" path="/summary" /></param>
    /// <param name="monitor">Encapsulates monitoring and logging.</param>
    /// <param name="modRegistry">The SMAPI API for fetching metadata about loaded mods.</param>
    public StopManager(ContentManager contentManager, IMonitor monitor, IModRegistry modRegistry)
    {
        this.ContentManager = contentManager;
        this.Monitor = monitor;

        this.BusLocationsProvider = new BusLocationsStopProvider(modRegistry, monitor, this.ContentManager.GetTranslation);
        this.TrainStationStopProvider = new TrainStationStopProvider(modRegistry, monitor, this.ContentManager.GetTranslation);
    }

    /// <summary>Get the stops which can be selected from the current location.</summary>
    /// <param name="networks">The networks for which to get stops.</param>
    public IEnumerable<Stop> GetAvailableStops(StopNetworks networks)
    {
        return this.GetStops(networks, ShouldSelectStop);

        bool ShouldSelectStop(string id, string stopLocation, string? condition, StopNetworks stopNetworks)
        {
            return this.ShouldEnableStop(id, stopLocation, condition, stopNetworks, networks);
        }
    }

    /// <summary>Get the stops which can be selected from the current location.</summary>
    /// <param name="networks">The networks for which to get stops.</param>
    /// <param name="shouldEnableStop">A filter which returns true for the stops to return; or <c>null</c> for all stops.</param>
    public IEnumerable<Stop> GetStops(StopNetworks networks, ShouldEnableStopDelegate? shouldEnableStop)
    {
        // Central Station stops
        foreach (Stop stop in this.ContentManager.GetStops(ShouldSelectStop))
            yield return stop;

        // from API
        foreach (Stop stop in this.ModApiStops.Values)
        {
            if (ShouldSelectStop(stop.Id, stop.ToLocation, stop.Condition, stop.Network))
                yield return stop;
        }

        // from mod integrations
        foreach (ICustomStopProvider provider in this.GetCustomStopProviders())
        {
            foreach (Stop stop in provider.GetAvailableStops(ShouldSelectStop))
                yield return stop;
        }

        bool ShouldSelectStop(string id, string stopLocation, string? condition, StopNetworks stopNetworks)
        {
            return
                stopNetworks.HasAnyFlag(networks)
                && shouldEnableStop?.Invoke(id, stopLocation, condition, stopNetworks) is not false;
        }
    }

    /// <summary>Try to load a legacy content pack which was reassigned to Central Station.</summary>
    /// <param name="contentPack">The content pack to load.</param>
    public bool TryLoadContentPack(IContentPack contentPack)
    {
        if (this.BusLocationsProvider.TryLoadContentPack(contentPack) || this.TrainStationStopProvider.TryLoadContentPack(contentPack))
        {
            this.CustomStopProviders = null;
            return true;
        }

        return false;
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get whether a stop should be enabled from the current location.</summary>
    /// <param name="id"><inheritdoc cref="Stop.Id"/></param>
    /// <param name="stopLocation"><inheritdoc cref="Stop.ToLocation"/></param>
    /// <param name="condition"><inheritdoc cref="Stop.Condition"/></param>
    /// <param name="stopNetworks"><inheritdoc cref="Stop.Network"/></param>
    /// <param name="travelingNetworks">The networks on which the player is traveling.</param>
    private bool ShouldEnableStop(string id, string stopLocation, string? condition, StopNetworks stopNetworks, StopNetworks travelingNetworks)
    {
        if (!stopNetworks.HasAnyFlag(travelingNetworks) || stopLocation == Game1.currentLocation.Name || !GameStateQuery.CheckConditions(condition))
            return false;

        if (Game1.getLocationFromName(stopLocation) is null)
        {
            this.Monitor.LogOnce($"Ignored {stopNetworks} destination with ID '{id}' because its target location '{stopLocation}' could not be found.", LogLevel.Warn);
            return false;
        }

        return true;
    }

    /// <summary>Load the integrations with other mods if they're not already loaded.</summary>
    [MemberNotNull(nameof(StopManager.CustomStopProviders))]
    private List<ICustomStopProvider> GetCustomStopProviders()
    {
        if (this.CustomStopProviders is null)
        {
            this.CustomStopProviders = [];

            if (this.BusLocationsProvider.IsNeeded())
                this.CustomStopProviders.Add(this.BusLocationsProvider);

            if (this.TrainStationStopProvider.IsNeeded())
                this.CustomStopProviders.Add(this.TrainStationStopProvider);
        }

        return this.CustomStopProviders;
    }
}
