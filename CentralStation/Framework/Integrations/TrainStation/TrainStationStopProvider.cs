using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common.Integrations.TrainStation;
using StardewModdingAPI;

namespace Pathoschild.Stardew.CentralStation.Framework.Integrations.TrainStation;

/// <summary>An integration which adds stops from the Train Station mod.</summary>
internal class TrainStationStopProvider : ICustomStopProvider
{
    /*********
    ** Fields
    *********/
    /// <summary>Encapsulates monitoring and logging.</summary>
    private readonly IMonitor Monitor;

    /// <summary>Whether the Expanded Preconditions Utility mod is installed.</summary>
    private readonly bool HasExpandedPreconditionsUtility;

    /// <summary>The integration with the Train Station mod.</summary>
    private readonly TrainStationIntegration TrainStation;

    /// <summary>The stops loaded from content packs reassigned to Central Station.</summary>
    private readonly List<Stop> ReassignedStops = [];

    /// <summary>Get a translation provided by the content pack.</summary>
    private readonly Func<string, object[], string> GetTranslation;

    /// <summary>The Train Station stop IDs which shouldn't be added to Central Station.</summary>
    private readonly HashSet<string> IgnoreStopIds = ["Cherry.TrainStation_BoatTunnel", "Cherry.TrainStation_GingerIsland", "Cherry.TrainStation_Railroad"];


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
    /// <param name="monitor">Encapsulates monitoring and logging.</param>
    /// <param name="getTranslation">Get a translation provided by the content pack.</param>
    public TrainStationStopProvider(IModRegistry modRegistry, IMonitor monitor, Func<string, object[], string> getTranslation)
    {
        this.Monitor = monitor;
        this.GetTranslation = getTranslation;

        this.HasExpandedPreconditionsUtility = modRegistry.IsLoaded("Cherry.ExpandedPreconditionsUtility");
        this.TrainStation = new TrainStationIntegration(modRegistry, monitor);
    }

    /// <summary>Whether the integration is needed.</summary>
    public bool IsNeeded()
    {
        return this.TrainStation.IsLoaded;
    }

    /// <summary>Try to load a Train Station content pack.</summary>
    /// <param name="contentPack">The content pack to load.</param>
    /// <returns>Returns whether it was successfully loaded as a Train Station content pack.</returns>
    public bool TryLoadContentPack(IContentPack contentPack)
    {
        if (!contentPack.HasFile("TrainStops.json"))
            return false;

        ReassignedContentPackModel data = contentPack.ModContent.Load<ReassignedContentPackModel>("TrainStops.json");
        if (data.BoatStops is null && data.TrainStops is null)
            return false; // not a Bus Locations content pack

        foreach (Stop stop in this.TryLoadStopsFromContentPackList(contentPack, data.BoatStops, StopNetworks.Boat))
            this.ReassignedStops.Add(stop);

        foreach (Stop stop in this.TryLoadStopsFromContentPackList(contentPack, data.TrainStops, StopNetworks.Train))
            this.ReassignedStops.Add(stop);

        return true;
    }

    /// <inheritdoc />
    public IEnumerable<Stop> GetAvailableStops(ShouldEnableStopDelegate shouldEnableStop)
    {
        // from reassigned content packs
        if (this.ReassignedStops.Count > 0)
        {
            foreach (Stop stop in this.ReassignedStops)
            {
                if (shouldEnableStop(stop.Id, stop.ToLocation, stop.Condition, stop.Network))
                    yield return stop;
            }
        }

        // from Train Station API
        if (this.TrainStation is { IsLoaded: true } api)
        {
            // get enumerator
            IEnumerator<ITrainStationStopModel?>? enumerator = null;
            try
            {
                enumerator = api.GetAvailableStops(true).Concat(api.GetAvailableStops(false)).GetEnumerator();
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Could not load stops from the Train Station mod because its API returned an unexpected error.\nTechnical details: {ex}", LogLevel.Warn);
                enumerator?.Dispose();
                yield break;
            }

            // yield each result
            while (true)
            {
                // get next stop
                ITrainStationStopModel? stop;
                try
                {
                    if (!enumerator.MoveNext())
                    {
                        enumerator.Dispose();
                        yield break;
                    }

                    stop = enumerator.Current;
                    if (stop is null)
                        continue;
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"Could not load stops from the Train Station mod because its API returned an unexpected error.\nTechnical details: {ex}", LogLevel.Warn);
                    yield break;
                }

                // skip if not available
                StopNetworks network = stop.IsBoat ? StopNetworks.Boat : StopNetworks.Train;
                string? condition = this.ConvertExpandedPreconditionsToGameStateQuery(stop.Conditions);
                if (!shouldEnableStop(stop.Id, stop.TargetMapName, condition, network))
                    continue;

                // add stop if valid
                Stop? loadedStop = this.TryLoadStop(
                    id: stop.Id,
                    displayName: () => stop.DisplayName,
                    targetMapName: stop.TargetMapName,
                    targetX: stop.TargetX,
                    targetY: stop.TargetY,
                    facingDirectionAfterWarp: stop.FacingDirectionAfterWarp,
                    cost: stop.Cost,
                    condition: condition,
                    network: network
                );
                if (loadedStop is not null)
                    yield return loadedStop;
            }
        }
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Try to load Train Station stops from a content pack reassigned to Central Station.</summary>
    /// <param name="contentPack">The content pack being loaded.</param>
    /// <param name="rawStops">The raw stops listed in the content pack.</param>
    /// <param name="network">The network for which to add the stop.</param>
    private IEnumerable<Stop> TryLoadStopsFromContentPackList(IContentPack contentPack, List<ReassignedContentPackStopModel?>? rawStops, StopNetworks network)
    {
        if (rawStops?.Count is not > 0)
            yield break;

        int index = 0;
        foreach (ReassignedContentPackStopModel? stop in rawStops)
        {
            if (stop?.TargetMapName is null)
                continue;

            string? condition = this.ConvertExpandedPreconditionsToGameStateQuery(stop.Conditions);
            Stop? parsedStop = this.TryLoadStop(
                id: $"{contentPack.Manifest.UniqueID}_{network}_{index++}", // match generated Train Station IDs
                displayName: stop.GetDisplayName,
                targetMapName: stop.TargetMapName,
                targetX: stop.TargetX,
                targetY: stop.TargetY,
                facingDirectionAfterWarp: stop.FacingDirectionAfterWarp,
                cost: stop.Cost,
                condition: condition,
                network: network
            );
            if (parsedStop is not null)
                yield return parsedStop;
        }
    }

    /// <summary>Try to load a Train Station stop based on its raw data.</summary>
    /// <param name="id">The unique ID for this stop.</param>
    /// <param name="displayName"><inheritdoc cref="ITrainStationStopModel.DisplayName" path="/summary"/></param>
    /// <param name="targetMapName"><inheritdoc cref="ITrainStationStopModel.TargetMapName" path="/summary"/></param>
    /// <param name="targetX"><inheritdoc cref="ITrainStationStopModel.TargetX" path="/summary"/></param>
    /// <param name="targetY"><inheritdoc cref="ITrainStationStopModel.TargetY" path="/summary"/></param>
    /// <param name="facingDirectionAfterWarp"><inheritdoc cref="ITrainStationStopModel.FacingDirectionAfterWarp" path="/summary"/></param>
    /// <param name="cost"><inheritdoc cref="ITrainStationStopModel.Cost" path="/summary"/></param>
    /// <param name="condition"><inheritdoc cref="Stop.Condition" path="/summary"/></param>
    /// <param name="network"><inheritdoc cref="Stop.Network" path="/summary"/></param>
    private Stop? TryLoadStop(string id, Func<string> displayName, string targetMapName, int targetX, int targetY, int facingDirectionAfterWarp, int cost, string? condition, StopNetworks network)
    {
        // ignore stops which duplicate a Central Station stop
        if (this.IgnoreStopIds.Contains(id))
            return null;

        // get stop
        return new Stop(
            Id: id,
            DisplayName: () => this.GetTranslation("destinations.from-train-station-mod", [displayName()]),
            DisplayNameInCombinedLists: null,
            ToLocation: targetMapName,
            ToTile: new Point(targetX, targetY),
            ToFacingDirection: facingDirectionAfterWarp,
            Cost: cost,
            Network: network,
            Condition: condition
        );
    }

    /// <summary>Convert Expanded Preconditions Utility's conditions to its equivalent game state query syntax.</summary>
    /// <param name="conditions">The Expanded Preconditions Utility conditions.</param>
    private string? ConvertExpandedPreconditionsToGameStateQuery(string?[]? conditions)
    {
        // skip if nothing to do
        if (conditions?.Length is null or 0)
            return null;

        // skip if Expanded Preconditions Utility not installed
        if (!this.HasExpandedPreconditionsUtility)
        {
            this.Monitor.LogOnce("The Train Station mod adds destinations with Expanded Preconditions Utility conditions, but you don't have Expanded Preconditions Utility installed. The destinations will default to always visible.", LogLevel.Warn);
            return null;
        }

        // convert to its game state query syntax
        const string expandedPreconditionsQuery = "Cherry.ExpandedPreconditionsUtility";
        switch (conditions.Length)
        {
            case 1:
                return $"{expandedPreconditionsQuery} {conditions[0]}";

            default:
                {
                    string[] queries = new string[conditions.Length];
                    for (int i = 0; i < conditions.Length; i++)
                        queries[i] = $"{expandedPreconditionsQuery} {conditions[i]}";

                    return "ANY \"" + string.Join("\" \"", queries) + "\"";
                }
        }
    }
}
