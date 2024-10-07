using System.Linq;
using StardewValley.ItemTypeDefinitions;

namespace Pathoschild.Stardew.LookupAnything.Framework.Models.FishData
{
    /// <summary>Spawning rules for a fish.</summary>
    /// <param name="FishItem">The fish item data.</param>
    /// <param name="Locations">Where the fish will spawn.</param>
    /// <param name="TimesOfDay">When the fish will spawn.</param>
    /// <param name="Weather">The weather in which the fish will spawn.</param>
    /// <param name="MinFishingLevel">The minimum fishing level.</param>
    /// <param name="IsUnique">Whether the fish can only be caught once.</param>
    internal record FishSpawnData(ParsedItemData FishItem, FishSpawnLocationData[]? Locations, FishSpawnTimeOfDayData[]? TimesOfDay, FishSpawnWeather Weather, int MinFishingLevel, bool IsUnique)
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get whether the fish is available in a given location name.</summary>
        /// <param name="locationName">The location name to match.</param>
        public bool MatchesLocation(string locationName)
        {
            return this.Locations?.Any(p => p.MatchesLocation(locationName)) is true;
        }
    }
}
