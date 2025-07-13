using System;
using Microsoft.Xna.Framework;
using StardewValley;

namespace Pathoschild.Stardew.CentralStation;

/// <summary>A destination that can be visited by the player.</summary>
public interface IStop
{
    /// <summary>The unique stop ID.</summary>
    string Id { get; }

    /// <summary>The translated name for the stop, shown in the destination menu.</summary>
    Func<string> DisplayName { get; }

    /// <summary>If set, overrides <see cref="DisplayName"/> when shown in a menu containing multiple transport networks.</summary>
    Func<string?>? DisplayNameInCombinedLists { get; }

    /// <summary>The internal name of the location to which the player should warp when they select this stop.</summary>
    string ToLocation { get; }

    /// <summary>The tile position to which the player should warp when they select this stop, or <c>null</c> to auto-detect a position based on the ticket machine tile (if present) else the default warp arrival tile.</summary>
    Point? ToTile { get; }

    /// <summary>The direction the player should be facing after they warp, matching a constant like <see cref="Game1.up"/>.</summary>
    int ToFacingDirection { get; }

    /// <summary>The gold price to go to that stop.</summary>
    int Cost { get; }

    /// <summary>The networks through which this stop is available.</summary>
    StopNetworks Network { get; }

    /// <summary>If set, a game state query which indicates whether this stop should appear in the menu at a given time. The contextual location is set to the player's current location.</summary>
    string? Condition { get; }
}
