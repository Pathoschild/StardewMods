using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace Pathoschild.Stardew.CentralStation.Framework.Integrations.BusLocations;

/// <summary>An integration which adds stops from the Bus Locations mod.</summary>
internal class BusLocationsStopProvider : ICustomStopProvider
{
    /*********
    ** Fields
    *********/
    /// <summary>Encapsulates monitoring and logging.</summary>
    private readonly IMonitor Monitor;

    /// <summary>Get a translation provided by the content pack.</summary>
    private readonly Func<string, object[], string> GetTranslation;

    /// <summary>The stops provided by Bus Locations.</summary>
    private readonly Stop[] BusStops;

    /// <summary>The unique ID for the Bus Locations mod.</summary>
    public const string ModId = "hootless.BusLocations";


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
    /// <param name="monitor">Encapsulates monitoring and logging.</param>
    /// <param name="getTranslation">Get a translation provided by the content pack.</param>
    public BusLocationsStopProvider(IModRegistry modRegistry, IMonitor monitor, Func<string, object[], string> getTranslation)
    {
        this.Monitor = monitor;
        this.GetTranslation = getTranslation;
        this.BusStops = this.LoadFromBusLocations(modRegistry, monitor) ?? [];
    }

    /// <summary>Whether the integration is needed.</summary>
    public bool IsNeeded()
    {
        return this.BusStops.Length > 0;
    }

    /// <inheritdoc />
    public IEnumerable<Stop> GetAvailableStops(StopNetworks networks)
    {
        return networks.HasFlag(StopNetworks.Bus)
            ? this.BusStops
            : [];
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Load the stops registered with the Bus Locations mod.</summary>
    /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
    /// <param name="monitor">Encapsulates monitoring and logging.</param>
    private Stop[]? LoadFromBusLocations(IModRegistry modRegistry, IMonitor monitor)
    {
        try
        {
            // get mod info
            IModInfo? modInfo = modRegistry.Get(ModId);
            if (modInfo is null)
                return null;

            // get mod instance
            object? mod = modInfo.GetType().GetProperty("Mod")?.GetValue(modInfo);
            if (mod is null)
            {
                monitor.Log($"Can't integrate with the Bus Locations mod because the {nameof(IMod)}.Mod property wasn't found.", LogLevel.Warn);
                return null;
            }

            // get its locations list
            if (mod.GetType().GetField("Locations", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(mod) is not IEnumerable locations)
            {
                monitor.Log("Can't integrate with the Bus Locations mod because its 'Locations' field wasn't found.", LogLevel.Warn);
                return null;
            }

            // load stops
            List<Stop> stops = [];
            foreach (object location in locations)
            {
                if (location is null)
                    continue;

                try
                {
                    // read model
                    Type type = location.GetType();
                    string? displayName = type.GetProperty("DisplayName")?.GetValue(location) as string;
                    string? mapName = type.GetProperty("MapName")?.GetValue(location) as string;
                    int destinationX = type.GetProperty("DestinationX")?.GetValue(location) as int? ?? -1;
                    int destinationY = type.GetProperty("DestinationY")?.GetValue(location) as int? ?? -1;
                    int arrivalFacing = type.GetProperty("ArrivalFacing")?.GetValue(location) as int? ?? Game1.down;
                    int ticketPrice = type.GetProperty("TicketPrice")?.GetValue(location) as int? ?? 0;

                    // ignore duplicate or invalid stops
                    if (string.IsNullOrWhiteSpace(mapName) || mapName is "Desert")
                        continue;

                    // add stop
                    stops.Add(
                        new Stop(
                            Id: $"BusLocations_{Guid.NewGuid():N}",
                            DisplayName: () => this.GetTranslation("destinations.from-bus-locations-mod", [displayName ?? mapName]),
                            DisplayNameInCombinedLists: null,
                            ToLocation: mapName,
                            ToTile: destinationX is not -1 && destinationY is not -1
                                ? new Point(destinationX, destinationY)
                                : null,
                            ToFacingDirection: arrivalFacing,
                            Cost: ticketPrice,
                            Network: StopNetworks.Bus,
                            Condition: null
                        )
                    );
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"Failed loading a stop from the Bus Locations mod.\nTechnical details: {ex}", LogLevel.Warn);
                }
            }

            return stops.ToArray();
        }
        catch (Exception ex)
        {
            monitor.Log($"Can't integrate with the Bus Locations mod due to an unexpected error.\nTechnical details: {ex}", LogLevel.Warn);
            return [];
        }
    }
}

