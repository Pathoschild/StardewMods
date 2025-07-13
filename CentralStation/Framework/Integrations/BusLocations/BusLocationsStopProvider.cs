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
    private readonly List<Stop> BusStops = [];

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

        this.AddStopsFromBusLocations(modRegistry, monitor);
    }

    /// <summary>Whether the integration is needed.</summary>
    public bool IsNeeded()
    {
        return this.BusStops.Count > 0;
    }

    /// <summary>Try to load a Bus Locations content pack.</summary>
    /// <param name="contentPack">The content pack to load.</param>
    /// <returns>Returns whether it was successfully loaded as a Bus Locations content pack.</returns>
    public bool TryLoadContentPack(IContentPack contentPack)
    {
        if (!contentPack.HasFile("content.json"))
            return false;

        ReassignedContentPackModel data = contentPack.ModContent.Load<ReassignedContentPackModel>("content.json");
        if (data.MapName is null && data.DestinationX is 0 && data.DestinationY is 0)
            return false; // not a Bus Locations content pack

        string id = contentPack.Manifest.UniqueID;
        this.TryAddStop(id, data.DisplayName, data.MapName, data.DestinationX, data.DestinationY, data.ArrivalFacing, data.TicketPrice);
        return true;
    }

    /// <inheritdoc />
    public IEnumerable<Stop> GetAvailableStops(ShouldEnableStopDelegate shouldEnableStop)
    {
        foreach (Stop stop in this.BusStops)
        {
            if (shouldEnableStop(stop.Id, stop.ToLocation, stop.Condition, stop.Network))
                yield return stop;
        }
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Add all the stops provided by the loaded Bus Locations mod, if applicable.</summary>
    /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
    /// <param name="monitor">Encapsulates monitoring and logging.</param>
    private void AddStopsFromBusLocations(IModRegistry modRegistry, IMonitor monitor)
    {
        try
        {
            // get mod info
            IModInfo? modInfo = modRegistry.Get(ModId);
            if (modInfo is null)
                return;

            // get mod instance
            object? mod = modInfo.GetType().GetProperty("Mod")?.GetValue(modInfo);
            if (mod is null)
            {
                monitor.Log($"Can't integrate with the Bus Locations mod because the {nameof(IMod)}.Mod property wasn't found.", LogLevel.Warn);
                return;
            }

            // get its locations list
            if (mod.GetType().GetField("Locations", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(mod) is not IEnumerable locations)
            {
                monitor.Log("Can't integrate with the Bus Locations mod because its 'Locations' field wasn't found.", LogLevel.Warn);
                return;
            }

            // load stops
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
                    string id = $"BusLocations_{Guid.NewGuid():N}";
                    this.TryAddStop(id, displayName, mapName, destinationX, destinationY, arrivalFacing, ticketPrice);
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"Failed loading a stop from the Bus Locations mod.\nTechnical details: {ex}", LogLevel.Warn);
                }
            }
        }
        catch (Exception ex)
        {
            monitor.Log($"Can't integrate with the Bus Locations mod due to an unexpected error.\nTechnical details: {ex}", LogLevel.Warn);
        }
    }

    /// <summary>Try to add a Bus Locations stop based on its raw data.</summary>
    /// <param name="id">The unique ID for this stop.</param>
    /// <param name="displayName"><inheritdoc cref="ReassignedContentPackModel.DisplayName" path="/summary"/></param>
    /// <param name="mapName"><inheritdoc cref="ReassignedContentPackModel.MapName" path="/summary"/></param>
    /// <param name="destinationX"><inheritdoc cref="ReassignedContentPackModel.DestinationX" path="/summary"/></param>
    /// <param name="destinationY"><inheritdoc cref="ReassignedContentPackModel.DestinationY" path="/summary"/></param>
    /// <param name="arrivalFacing"><inheritdoc cref="ReassignedContentPackModel.ArrivalFacing" path="/summary"/></param>
    /// <param name="ticketPrice"><inheritdoc cref="ReassignedContentPackModel.TicketPrice" path="/summary"/></param>
    private void TryAddStop(string id, string? displayName, string? mapName, int destinationX, int destinationY, int arrivalFacing, int ticketPrice)
    {
        // ignore duplicate or invalid stops
        if (string.IsNullOrWhiteSpace(mapName) || mapName is "Desert")
            return;

        // add stop
        this.BusStops.Add(
            new Stop(
                Id: id,
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
}

