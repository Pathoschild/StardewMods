using System.Linq;

namespace Pathoschild.Stardew.LookupAnything.Framework.Models.FishData
{
    /// <summary>Spawning rules for a fish.</summary>
    internal class FishSpawnData
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The fish ID.</summary>
        public int FishID { get; }

        /// <summary>Where the fish will spawn.</summary>
        public FishSpawnLocationData[] Locations { get; }

        /// <summary>When the fish will spawn.</summary>
        public FishSpawnTimeOfDayData[] TimesOfDay { get; }

        /// <summary>The weather in which the fish will spawn.</summary>
        public FishSpawnWeather Weather { get; }

        /// <summary>The minimum fishing level.</summary>
        public int MinFishingLevel { get; }

        /// <summary>Whether the fish can only be caught once.</summary>
        public bool IsUnique { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="fishID">The fish ID.</param>
        /// <param name="locations">Where the fish will spawn.</param>
        /// <param name="timesOfDay">When the fish will spawn.</param>
        /// <param name="weather">The weather in which the fish will spawn.</param>
        /// <param name="minFishingLevel">The minimum fishing level.</param>
        /// <param name="isUnique">Whether the fish can only be caught once.</param>
        public FishSpawnData(int fishID, FishSpawnLocationData[] locations, FishSpawnTimeOfDayData[] timesOfDay, FishSpawnWeather weather, int minFishingLevel, bool isUnique)
        {
            this.FishID = fishID;
            this.Locations = locations;
            this.TimesOfDay = timesOfDay;
            this.Weather = weather;
            this.MinFishingLevel = minFishingLevel;
            this.IsUnique = isUnique;
        }

        /// <summary>Get whether the fish is available in a given location name.</summary>
        /// <param name="locationName">The location name to match.</param>
        public bool MatchesLocation(string locationName)
        {
            return this.Locations.Any(p => p.MatchesLocation(locationName));
        }
    }
}
