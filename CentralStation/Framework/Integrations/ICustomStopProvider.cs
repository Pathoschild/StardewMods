using System.Collections.Generic;

namespace Pathoschild.Stardew.CentralStation.Framework.Integrations;

/// <summary>A mod integration which adds stops to Central Station networks.</summary>
internal interface ICustomStopProvider
{
    /// <summary>Get the stops which can be selected from the current location.</summary>
    /// <param name="networks">The networks for which to get stops.</param>
    /// <param name="shouldEnableStop">Get whether a stop should be enabled from the current location.</param>
    IEnumerable<Stop> GetAvailableStops(StopNetworks networks, ShouldEnableStopDelegate shouldEnableStop);
}
