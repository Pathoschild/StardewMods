using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace Pathoschild.Stardew.CentralStation.Framework;

/// <inheritdoc cref="ICentralStationApi" />
public class CentralStationApi : ICentralStationApi, IDeprecatedCentralStationApi
{
    /*********
    ** Fields
    *********/
    /// <summary>The mod which requested the API.</summary>
    private readonly IManifest Mod;

    /// <summary>The Central Station component which manages available destinations.</summary>
    private readonly StopManager StopManager;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="mod">The mod which requested the API.</param>
    /// <param name="stopManager">The Central Station component which manages available destinations.</param>
    internal CentralStationApi(IManifest mod, StopManager stopManager)
    {
        this.Mod = mod;
        this.StopManager = stopManager;
    }

    /****
    ** Main API
    ****/
    /// <inheritdoc />
    public IEnumerable<IStop> GetAllStops(StopNetworks? network = null)
    {
        return this.StopManager.GetStops(network ?? StopManager.AllNetworks, null);
    }

    /// <inheritdoc />
    public IEnumerable<IStop> GetAvailableStops(StopNetworks? network = null)
    {
        return this.StopManager.GetAvailableStops(network ?? StopManager.AllNetworks);
    }

    /// <inheritdoc />
    public void RegisterStop(string id, Func<string> displayName, string toLocation, Point? toTile, int toFacingDirection, int cost, StopNetworks network, string? condition)
    {
        // validate
        if (string.IsNullOrWhiteSpace(id))
            throw this.BuildArgRequiredError(null, nameof(id));
        if (displayName is null)
            throw this.BuildArgRequiredError(id, nameof(displayName));
        if (string.IsNullOrWhiteSpace(toLocation))
            throw this.BuildArgRequiredError(id, nameof(toLocation));
        if (!Enum.IsDefined(network))
            throw this.BuildArgRequiredError(id, nameof(network));

        // normalize
        id = this.GetStopId(id);
        toLocation = toLocation.Trim();
        if (toFacingDirection is not (Game1.up or Game1.left or Game1.right or Game1.down))
            toFacingDirection = Game1.down;
        if (cost < 0)
            cost = 0;
        if (string.IsNullOrWhiteSpace(condition))
            condition = null;

        // register stop
        this.StopManager.ModApiStops[id] = new Stop(
            Id: id,
            DisplayName: displayName,
            DisplayNameInCombinedLists: null,
            ToLocation: toLocation,
            ToTile: toTile,
            ToFacingDirection: toFacingDirection,
            Cost: cost,
            Network: network,
            Condition: condition
        );
    }

    /// <inheritdoc />
    public bool RemoveStop(string id)
    {
        id = this.GetStopId(id);

        return this.StopManager.ModApiStops.Remove(id);
    }

    /****
    ** Deprecated API
    ****/
    /// <inheritdoc />
    public void RegisterStop(string id, Func<string> displayName, string toLocation, Point? toTile, int toFacingDirection, int cost, string network, string? condition)
    {
        // validate
        if (!Utility.TryParseEnum(network, out StopNetworks parsedNetwork))
            throw this.BuildArgError(id, nameof(network), $"the '{nameof(network)}' value '{network}' can't be parsed as a valid network; must be one of ['{string.Join("', '", Enum.GetNames<StopNetworks>())}'] or a combination thereof");

        // register stop
        this.RegisterStop(id, displayName, toLocation, toTile, toFacingDirection, cost, parsedNetwork, condition);
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get the unique stop ID namespaced to the mod which registered it.</summary>
    /// <param name="localId">The local stop ID without the mod ID prefix.</param>
    private string GetStopId(string? localId)
    {
        return $"{this.Mod.UniqueID}_{localId?.Trim()}";
    }

    /// <summary>Build an argument exception for a missing required field.</summary>
    /// <param name="stopId">The stop ID being registered, if available.</param>
    /// <param name="fieldName">The name of the field which failed validation.</param>
    private ArgumentException BuildArgRequiredError(string? stopId, string fieldName)
    {
        return this.BuildArgError(stopId, fieldName, $"the '{fieldName}' argument is required");
    }

    /// <summary>Build an argument exception for a missing required field.</summary>
    /// <param name="stopId">The stop ID being registered, if available.</param>
    /// <param name="fieldName">The name of the field which failed validation.</param>
    /// <param name="errorPhrase">The error phrase which describes why the field failed validation.</param>
    private ArgumentException BuildArgError(string? stopId, string fieldName, string errorPhrase)
    {
        return new ArgumentException($"Can't register stop {(stopId != null ? "'{stopId}' " : "")}from mod '{this.Mod.Name}' because {errorPhrase}.", fieldName);
    }
}
