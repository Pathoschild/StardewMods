using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.LookupAnything.Framework.Fields.Models;
using Pathoschild.Stardew.LookupAnything.Framework.Models.FishData;
using StardewValley;
using StardewValley.ItemTypeDefinitions;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields;

/// <summary>A metadata field which shows the spawn rules for a fish.</summary>
internal class FishSpawnRulesField : CheckboxListField
{
    /*********
    ** Fields
    *********/
    /// <summary>The valid seasons.</summary>
    private readonly string[] Seasons = ["spring", "summer", "fall", "winter"];


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    /// <param name="label">A short field label.</param>
    /// <param name="fish">The fish item data.</param>
    public FishSpawnRulesField(GameHelper gameHelper, string label, ParsedItemData fish)
        : base(label)
    {
        this.CheckboxLists = [ new CheckboxList(this.GetConditions(gameHelper, fish)) ];
        this.HasValue = this.CheckboxLists.Any();
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get the formatted checkbox conditions to display.</summary>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    /// <param name="fish">The fish item data.</param>
    private IEnumerable<CheckboxList.Checkbox> GetConditions(GameHelper gameHelper, ParsedItemData fish)
    {
        // get spawn data
        FishSpawnData spawnRules = gameHelper.GetFishSpawnRules(fish);
        if (spawnRules.Locations?.Any() != true)
            yield break;

        // not caught uet
        if (spawnRules.IsUnique)
            yield return this.GetCondition(I18n.Item_FishSpawnRules_NotCaughtYet(), !Game1.player.fishCaught.ContainsKey(fish.QualifiedItemId));

        // fishing level
        if (spawnRules.MinFishingLevel > 0)
            yield return this.GetCondition(I18n.Item_FishSpawnRules_MinFishingLevel(level: spawnRules.MinFishingLevel), Game1.player.FishingLevel >= spawnRules.MinFishingLevel);

        // extended family quest
        if (spawnRules.IsLegendaryFamily)
            yield return this.GetCondition(I18n.Item_FishSpawnRules_ExtendedFamilyQuestActive(), Game1.player.team.SpecialOrderRuleActive("LEGENDARY_FAMILY"));

        // weather
        if (spawnRules.Weather == FishSpawnWeather.Sunny)
            yield return this.GetCondition(I18n.Item_FishSpawnRules_WeatherSunny(), !Game1.isRaining);
        else if (spawnRules.Weather == FishSpawnWeather.Rainy)
            yield return this.GetCondition(I18n.Item_FishSpawnRules_WeatherRainy(), Game1.isRaining);

        // time of day
        if (spawnRules.TimesOfDay?.Any() == true)
        {
            yield return this.GetCondition(
                label: I18n.Item_FishSpawnRules_Time(
                    times: I18n.List(
                        spawnRules.TimesOfDay.Select(p => I18n.Generic_Range(CommonHelper.FormatTime(p.MinTime), CommonHelper.FormatTime(p.MaxTime)).ToString())
                    )
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
                        seasons: I18n.List(
                            firstLocation.Seasons.Select(gameHelper.TranslateSeason)
                        )
                    ),
                    isMet: firstLocation.Seasons.Contains(Game1.currentSeason)
                );
            }

            // locations
            yield return this.GetCondition(
                label: I18n.Item_FishSpawnRules_Locations(
                    locations: I18n.List(
                        spawnRules.Locations.Select(gameHelper.GetLocationDisplayName).OrderBy(p => p)
                    )
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
                    select new { Season = season, LocationName = gameHelper.GetLocationDisplayName(location) }
                )
                .GroupBy(p => p.Season, p => p.LocationName, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(p => p.Key, p => p.ToArray(), StringComparer.OrdinalIgnoreCase);

            var summary = new List<IFormattedText> { new FormattedText(I18n.Item_FishSpawnRules_LocationsBySeason_Label()) };
            foreach (string season in this.Seasons)
            {
                if (locationsBySeason.TryGetValue(season, out string[]? locationNames))
                {
                    summary.Add(new FormattedText(
                        text: Environment.NewLine + I18n.Item_FishSpawnRules_LocationsBySeason_SeasonLocations(season: gameHelper.TranslateSeason(season), locations: I18n.List(locationNames)),
                        color: season == Game1.currentSeason ? Color.Black : Color.Gray
                    ));
                }
            }

            bool hasMatch = spawnRules.Locations.Any(p => p.LocationId == Game1.currentLocation.Name && p.Seasons.Contains(Game1.currentSeason));
            yield return this.GetCondition(summary, hasMatch);
        }
    }

    /// <summary>Get a condition formatted for checkbox rendering.</summary>
    /// <param name="label">The display text for the condition.</param>
    /// <param name="isMet">Whether the condition is met.</param>
    private CheckboxList.Checkbox GetCondition(string label, bool isMet)
    {
        return new CheckboxList.Checkbox(text: label, isChecked: isMet);
    }

    /// <summary>Get a condition formatted for checkbox rendering.</summary>
    /// <param name="label">The display text for the condition.</param>
    /// <param name="isMet">Whether the condition is met.</param>
    private CheckboxList.Checkbox GetCondition(IEnumerable<IFormattedText> label, bool isMet)
    {
        return new CheckboxList.Checkbox(Text: label.ToArray(), IsChecked: isMet);
    }

    /// <summary>Get whether all locations specify the same seasons.</summary>
    /// <param name="locations">The locations to check.</param>
    private bool HaveSameSeasons(IEnumerable<FishSpawnLocationData> locations)
    {
        ISet<string>? seasons = null;
        foreach (FishSpawnLocationData location in locations)
        {
            if (seasons == null)
                seasons = location.Seasons;
            else if (seasons.Count != location.Seasons.Count || !location.Seasons.All(seasons.Contains))
                return false;
        }

        return true;
    }
}
