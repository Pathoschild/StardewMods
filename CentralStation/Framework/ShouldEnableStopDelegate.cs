namespace Pathoschild.Stardew.CentralStation.Framework;

/// <summary>A filter which indicates whether a stop should be selected.</summary>
/// <param name="id"><inheritdoc cref="Stop.Id"/></param>
/// <param name="stopLocation"><inheritdoc cref="Stop.ToLocation"/></param>
/// <param name="condition"><inheritdoc cref="Stop.Condition"/></param>
/// <param name="stopNetworks"><inheritdoc cref="Stop.Network"/></param>
internal delegate bool ShouldEnableStopDelegate(string id, string stopLocation, string? condition, StopNetworks stopNetworks);
