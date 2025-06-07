namespace Pathoschild.Stardew.CentralStation.Framework;

/// <inheritdoc cref="ContentManager.ShouldEnableStop"/>
internal delegate bool ShouldEnableStopDelegate(string id, string stopLocation, string? condition, StopNetworks stopNetworks, StopNetworks travelingNetworks);
