using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;

namespace Pathoschild.Stardew.CentralStation;

/// <summary>The public API for the Central Station mod.</summary>
public interface ICentralStationApi
{
    /// <summary>Get the destinations which are available at this moment from the player's current location.</summary>
    /// <param name="network">If set, only return stops connected to these networks.</param>
    /// <remarks>The <see cref="IStop.Condition"/> field is checked before returning each stop, so there's no need to check it again.</remarks>
    IEnumerable<IStop> GetAvailableStops(StopNetworks? network = null);

    /// <summary>Add a destination that can be visited by the player, or replace one you previously registered from the same mod.</summary>
    /// <param name="id">An identifier for this stop. This is automatically prefixed with your mod ID, so you shouldn't prefix it manually.</param>
    /// <param name="displayName">The translated name for the stop, shown in the destination menu.</param>
    /// <param name="toLocation">The internal name of the location to which the player should warp when they select this stop.</param>
    /// <param name="toTile">The tile position to which the player should warp when they select this stop, or <c>null</c> to auto-detect a position based on the ticket machine tile (if present) else the default warp arrival tile.</param>
    /// <param name="toFacingDirection">The direction the player should be facing after they warp, matching a constant like <see cref="Game1.up"/>.</param>
    /// <param name="cost">The gold price to go to that stop.</param>
    /// <param name="network">The networks through which this stop is available.</param>
    /// <param name="condition">If set, a game state query which indicates whether this stop should appear in the menu at a given time. The contextual location is set to the player's current location.</param>
    void RegisterStop(string id, Func<string> displayName, string toLocation, Point? toTile, int toFacingDirection, int cost, StopNetworks network, string? condition);

    /// <summary>Remove a stop that was registered by the same mod.</summary>
    /// <param name="id">The identifier for the stop which was passed to <see cref="RegisterStop"/>, excluding the mod ID prefix which is automatically added by that method.</param>
    /// <returns>Returns whether the stop was found and removed.</returns>
    bool RemoveStop(string id);
}

/// <summary>The deprecated public API methods for the Central Station mod.</summary>
public interface IDeprecatedCentralStationApi
{
    // DO NOT USE THIS INTERFACE IN NEW CODE.
    // These methods are only provided for backwards compatibility.

    /// <inheritdoc cref="ICentralStationApi.RegisterStop"/>
    [Obsolete]
    void RegisterStop(string id, Func<string> displayName, string toLocation, Point? toTile, int toFacingDirection, int cost, string network, string? condition);
}
