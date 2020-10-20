using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Models.FishData;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields
{
    /// <summary>A metadata field which shows the spawn rules for a fish.</summary>
    internal class FishSpawnRulesField : CheckboxListField
    {
        /*********
        ** Fields
        *********/
        /// <summary>The valid seasons.</summary>
        private readonly string[] Seasons = { "spring", "summer", "fall", "winter" };


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="label">A short field label.</param>
        /// <param name="fishID">The fish ID.</param>
        public FishSpawnRulesField(GameHelper gameHelper, string label, int fishID)
            : base(label)
        {
            this.Checkboxes = this.GetConditions(gameHelper, fishID).ToArray();
            this.HasValue = this.Checkboxes.Any();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the formatted checkbox conditions to display.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="fishID">The fish ID.</param>
        private IEnumerable<KeyValuePair<IFormattedText[], bool>> GetConditions(GameHelper gameHelper, int fishID)
        {
            // get spawn data
            FishSpawnData spawnRules = gameHelper.GetFishSpawnRules(fishID);
            if (spawnRules == null || !spawnRules.Locations.Any())
                yield break;

            // not caught uet
            if (spawnRules.IsUnique)
                yield return this.GetCondition(I18n.Item_FishSpawnRules_NotCaughtYet(), !Game1.player.fishCaught.ContainsKey(fishID));

            // fishing level
            if (spawnRules.MinFishingLevel > 0)
                yield return this.GetCondition(I18n.Item_FishSpawnRules_MinFishingLevel(level: spawnRules.MinFishingLevel), Game1.player.FishingLevel >= spawnRules.MinFishingLevel);

            // weather
            if (spawnRules.Weather == FishSpawnWeather.Sunny)
                yield return this.GetCondition(I18n.Item_FishSpawnRules_WeatherSunny(), !Game1.isRaining);
            else if (spawnRules.Weather == FishSpawnWeather.Rainy)
                yield return this.GetCondition(I18n.Item_FishSpawnRules_WeatherRainy(), Game1.isRaining);

            // time of day
            if (spawnRules.TimesOfDay.Any())
            {
                yield return this.GetCondition(
                    label: I18n.Item_FishSpawnRules_Time(
                        times: string.Join(", ", spawnRules.TimesOfDay.Select(p => I18n.Generic_Range(gameHelper.FormatMilitaryTime(p.MinTime), gameHelper.FormatMilitaryTime(p.MaxTime)).ToString()))
                    ),
                    isMet: spawnRules.TimesOfDay.Any(p => Game1.timeOfDay >= p.MinTime && Game1.timeOfDay <= p.MaxTime)
                );
            }

            // locations & seasons
            if (this.HaveSameSeasons(spawnRules.Locations))
            {
                var firstLocation = spawnRules.Locations[0];

                // seasons
                if (firstLocation.Seasons.Count == 4)
                    yield return this.GetCondition(I18n.Item_FishSpawnRules_SeasonAny(), true);
                else
                {
                    yield return this.GetCondition(
                        label: I18n.Item_FishSpawnRules_SeasonList(
                            seasons: string.Join(", ", firstLocation.Seasons.Select(gameHelper.TranslateSeason))
                        ),
                        isMet: firstLocation.Seasons.Contains(Game1.currentSeason)
                    );
                }

                // locations
                yield return this.GetCondition(
                    label: I18n.Item_FishSpawnRules_Locations(
                        locations: string.Join(", ", spawnRules.Locations.Select(p => p.LocationDisplayName).OrderBy(p => p))
                    ),
                    isMet: spawnRules.MatchesLocation(Game1.currentLocation.Name)
                );
            }
            else
            {
                IDictionary<string, string[]> locationsBySeason =
                    (
                        from location in spawnRules.Locations
                        from season in location.Seasons
                        select new { Season = season, LocationName = location.LocationDisplayName }
                    )
                    .GroupBy(p => p.Season, p => p.LocationName)
                    .ToDictionary(p => p.Key, p => p.ToArray());

                var summary = new List<IFormattedText> { new FormattedText(I18n.Item_FishSpawnRules_LocationsBySeason_Label()) };
                foreach (string season in this.Seasons)
                {
                    if (locationsBySeason.TryGetValue(season, out string[] locationNames))
                    {
                        summary.Add(new FormattedText(
                            text: Environment.NewLine + I18n.Item_FishSpawnRules_LocationsBySeason_SeasonLocations(season: gameHelper.TranslateSeason(season), locations: string.Join(", ", locationNames)),
                            color: season == Game1.currentSeason ? Color.Black : Color.Gray
                        ));
                    }
                }

                bool hasMatch = spawnRules.Locations.Any(p => p.LocationName == Game1.currentLocation.Name && p.Seasons.Contains(Game1.currentSeason));
                yield return this.GetCondition(summary, hasMatch);
            }
        }

        /// <summary>Get a condition formatted for checkbox rendering.</summary>
        /// <param name="label">The display text for the condition.</param>
        /// <param name="isMet">Whether the condition is met.</param>
        private KeyValuePair<IFormattedText[], bool> GetCondition(string label, bool isMet)
        {
            return CheckboxListField.Checkbox(text: label, value: isMet);
        }

        /// <summary>Get a condition formatted for checkbox rendering.</summary>
        /// <param name="label">The display text for the condition.</param>
        /// <param name="isMet">Whether the condition is met.</param>
        private KeyValuePair<IFormattedText[], bool> GetCondition(IEnumerable<IFormattedText> label, bool isMet)
        {
            return CheckboxListField.Checkbox(text: label.ToArray(), value: isMet);
        }

        /// <summary>Get whether all locations specify the same seasons.</summary>
        /// <param name="locations">The locations to check.</param>
        private bool HaveSameSeasons(IEnumerable<FishSpawnLocationData> locations)
        {
            ISet<string> seasons = null;
            foreach (var location in locations)
            {
                if (seasons == null)
                    seasons = location.Seasons;
                else if (seasons.Count != location.Seasons.Count || !location.Seasons.All(seasons.Contains))
                    return false;
            }

            return true;
        }
    }
}
