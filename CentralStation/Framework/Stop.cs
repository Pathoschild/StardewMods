using System;
using Microsoft.Xna.Framework;

namespace Pathoschild.Stardew.CentralStation.Framework;

/// <inheritdoc cref="IStop"/>
/// <param name="Id"><inheritdoc cref="IStop.Id"/></param>
/// <param name="DisplayName"><inheritdoc cref="IStop.DisplayName"/></param>
/// <param name="DisplayNameInCombinedLists"><inheritdoc cref="IStop.DisplayNameInCombinedLists"/></param>
/// <param name="ToLocation"><inheritdoc cref="IStop.ToLocation"/></param>
/// <param name="ToTile"><inheritdoc cref="IStop.ToTile"/></param>
/// <param name="ToFacingDirection"><inheritdoc cref="IStop.ToFacingDirection"/></param>
/// <param name="Cost"><inheritdoc cref="IStop.Cost"/></param>
/// <param name="Network"><inheritdoc cref="IStop.Network"/></param>
/// <param name="Condition"><inheritdoc cref="IStop.Condition"/></param>
internal record Stop(string Id, Func<string> DisplayName, Func<string?>? DisplayNameInCombinedLists, string ToLocation, Point? ToTile, int ToFacingDirection, int Cost, StopNetworks Network, string? Condition) : IStop;
