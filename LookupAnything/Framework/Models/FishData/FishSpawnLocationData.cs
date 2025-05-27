using System;
using System.Collections.Generic;
using StardewValley.Locations;

namespace Pathoschild.Stardew.LookupAnything.Framework.Models.FishData;

/// <summary>Location-specific spawn rules for a fish.</summary>
/// <param name="LocationId">The location's internal name.</param>
/// <param name="Area">The area ID within the location, if applicable.</param>
/// <param name="Seasons">The required seasons.</param>
internal record FishSpawnLocationData(string LocationId, string? Area, HashSet<string> Seasons)
{
    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="locationId">The location's internal name.</param>
    /// <param name="area">The area ID within the location, if applicable.</param>
    /// <param name="seasons">The required seasons.</param>
    internal FishSpawnLocationData(string locationId, int? area, string[] seasons)
        : this(locationId, area >= 0 ? area.ToString() : null, seasons) { }

    /// <summary>Construct an instance.</summary>
    /// <param name="locationId">The location's internal name.</param>
    /// <param name="area">The area ID within the location, if applicable.</param>
    /// <param name="seasons">The required seasons.</param>
    internal FishSpawnLocationData(string locationId, string? area, string[] seasons)
        : this(locationId, area, new HashSet<string>(seasons, StringComparer.OrdinalIgnoreCase)) { }

    /// <summary>Get whether this matches a given location name.</summary>
    /// <param name="locationId">The location internal name to match.</param>
    public bool MatchesLocation(string locationId)
    {
        // mine level
        if (this.LocationId == "UndergroundMine")
        {
            return string.IsNullOrWhiteSpace(this.Area)
                ? MineShaft.IsGeneratedLevel(locationId)
                : locationId == $"{this.LocationId}{this.Area}";
        }

        // location name
        return locationId == this.LocationId;
    }
}
