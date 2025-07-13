using System.Collections.Generic;

namespace Pathoschild.Stardew.CentralStation.Framework.Integrations;

/// <summary>A mod integration which adds stops to Central Station networks.</summary>
internal interface ICustomStopProvider
{
    /// <summary>Get the stops available from this provider.</summary>
    /// <param name="shouldEnableStop">Get whether a stop should be selected.</param>
    IEnumerable<Stop> GetAvailableStops(ShouldEnableStopDelegate shouldEnableStop);
}
