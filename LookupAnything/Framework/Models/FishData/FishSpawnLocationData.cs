using System;
using System.Collections.Generic;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;

namespace Pathoschild.Stardew.LookupAnything.Framework.Models.FishData
{
    /// <summary>Location-specific spawn rules for a fish.</summary>
    internal class FishSpawnLocationData
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The location's internal name.</summary>
        public string LocationName { get; }

        /// <summary>The location's translated name.</summary>
        public string LocationDisplayName => this.Area != null
            ? L10n.LocationOverrides.AreaName(this.LocationName, this.Area)
            : L10n.LocationOverrides.LocationName(this.LocationName);

        /// <summary>The area ID within the location, if applicable.</summary>
        public string Area { get; }

        /// <summary>The required seasons.</summary>
        public ISet<string> Seasons { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="locationName">The location name.</param>
        /// <param name="area">The area ID within the location, if applicable.</param>
        /// <param name="seasons">The required seasons.</param>
        internal FishSpawnLocationData(string locationName, int? area, string[] seasons)
            : this(locationName, area >= 0 ? area.ToString() : null, seasons) { }

        /// <summary>Construct an instance.</summary>
        /// <param name="locationName">The location name.</param>
        /// <param name="area">The area ID within the location, if applicable.</param>
        /// <param name="seasons">The required seasons.</param>
        public FishSpawnLocationData(string locationName, string area, string[] seasons)
        {
            this.LocationName = locationName;
            this.Area = area;
            this.Seasons = new HashSet<string>(seasons, StringComparer.InvariantCultureIgnoreCase);
        }
    }
}
