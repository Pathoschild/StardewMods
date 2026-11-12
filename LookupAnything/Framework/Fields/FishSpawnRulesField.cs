using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    /// <summary>Whether the field describes a water tile.</summary>
    private readonly bool IsTileLookup;

    /// <summary>The valid seasons.</summary>
    private static readonly string[] Seasons = ["spring", "summer", "fall", "winter"];


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance for a single fish.</summary>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    /// <param name="label">A short field label.</param>
    /// <param name="fish">The fish item data.</param>
    /// <param name="showUncaughtFishSpawnRules">Whether to show spawn conditions for uncaught fish.</param>
    public FishSpawnRulesField(GameHelper gameHelper, string label, ParsedItemData fish, bool showUncaughtFishSpawnRules)
        : this(label, new CheckboxList(FishSpawnRulesField.GetConditions(gameHelper, fish), isHidden: !showUncaughtFishSpawnRules && !FishSpawnRulesField.HasPlayerCaughtFish(fish))) { }

    /// <summary>Construct an instance for all fish in a body of water.</summary>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    /// <param name="label">A short field label.</param>
    /// <param name="location">The location whose fish spawn conditions to get.</param>
    /// <param name="tile">The tile for which to get the spawn rules.</param>
    /// <param name="fishAreaId">The internal ID of the fishing area for which to get the spawn rules.</param>
    /// <param name="showUncaughtFishSpawnRules">Whether to show spawn conditions for uncaught fish.</param>
    public FishSpawnRulesField(GameHelper gameHelper, string label, GameLocation location, Vector2 tile, string fishAreaId, bool showUncaughtFishSpawnRules)
        : this(label, FishSpawnRulesField.GetConditions(gameHelper, location, tile, fishAreaId, showUncaughtFishSpawnRules).ToArray()) { this.IsTileLookup = true; }

    /// <summary>Whether this field describes a water tile rather than one fish.</summary>
    public bool IsWaterTile => this.IsTileLookup;

    /// <inheritdoc />
    public override string? GetSpokenText()
    {
        if (!this.IsTileLookup)
            return base.GetSpokenText();

        CheckboxList[] visible = this.CheckboxLists.Where(p => !p.IsHidden).ToArray();
        string summary = I18n.ScreenReader_FishSummary(count: visible.Length, catchable: visible.Count(p => p.Checkboxes.All(c => c.IsChecked)));
        string? hidden = this.GetSpokenHiddenListsText();
        return hidden == null ? summary : $"{summary}, {hidden}";
    }

    /// <inheritdoc />
    public override IReadOnlyList<SpokenEntry> GetSpokenEntries()
    {
        if (!this.IsTileLookup)
            return base.GetSpokenEntries();

        List<SpokenEntry> entries = this.CheckboxLists
            .Where(p => !p.IsHidden)
            .OrderByDescending(p => p.Checkboxes.All(c => c.IsChecked))
            .ThenBy(p => p.Intro?.Text, StringComparer.CurrentCultureIgnoreCase)
            .Select(p =>
            {
                string name = p.Intro?.Text ?? "";
                string[] unmet = p.Checkboxes.Where(c => !c.IsChecked)
                    .Select(c => c.UnmetText ?? c.SpokenText ?? SpokenText.FromFormatted(c.Text) ?? "")
                    .Where(text => !string.IsNullOrWhiteSpace(text)).ToArray();
                string text = p.Checkboxes.All(c => c.IsChecked)
                    ? I18n.ScreenReader_FishCatchable(name)
                    : I18n.ScreenReader_FishNotCatchable(name: name, conditions: string.Join(", ", unmet));
                SpokenEntry[] children = this.GetSpokenCheckboxes(p).Select(text => new SpokenEntry(text)).ToArray();
                return new SpokenEntry(text, Children: children);
            }).ToList();
        string? hidden = this.GetSpokenHiddenListsText();
        if (hidden != null)
            entries.Add(new SpokenEntry(hidden));
        return entries;
    }

    /// <inheritdoc/>
    public override Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth, float visibleHeight)
    {
        float topOffset = 0;
        int hiddenSpawnRulesCount = 0;

        // draw checkbox lists
        foreach (CheckboxList checkboxList in this.CheckboxLists)
        {
            if (topOffset > visibleHeight)
                break;

            if (checkboxList.IsHidden)
                hiddenSpawnRulesCount++;
            else
                topOffset += this.DrawCheckboxList(checkboxList, spriteBatch, font, new Vector2(position.X, position.Y + topOffset), wrapWidth, visibleHeight).Y;
        }

        // draw 'X uncaught fish' message
        if (hiddenSpawnRulesCount > 0 && topOffset < visibleHeight)
            topOffset += this.LineHeight + this.DrawIconText(spriteBatch, font, new Vector2(position.X, position.Y + topOffset), wrapWidth, I18n.Item_UncaughtFish(hiddenSpawnRulesCount), Color.Gray).Y;

        return new Vector2(wrapWidth, topOffset - this.LineHeight);
    }


    /*********
    ** Protected methods
    *********/
    /// <inheritdoc />
    protected override bool CheckboxesAreConditions => true;

    /// <inheritdoc />
    protected override string? GetSpokenHiddenListsText()
    {
        int hiddenCount = this.CheckboxLists.Count(checkboxList => checkboxList.IsHidden);
        return hiddenCount > 0
            ? I18n.Item_UncaughtFish(hiddenCount)
            : null;
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="label">A short field label.</param>
    /// <param name="spawnConditions">Array of lists of fish spawn conditions.</param>
    private FishSpawnRulesField(string label, params CheckboxList[] spawnConditions)
        : base(label)
    {
        this.CheckboxLists = spawnConditions;
        this.HasValue = this.CheckboxLists.Any(checkboxList => checkboxList.Checkboxes.Length > 0);
    }

    /// <summary>Get the formatted checkbox conditions for all fish in a location.</summary>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    /// <param name="location">The location whose fish spawn conditions to get.</param>
    /// <param name="tile">The tile for which to get the spawn rules.</param>
    /// <param name="fishAreaId">The internal ID of the fishing area for which to get the spawn rules.</param>
    /// <param name="showUncaughtFishSpawnRules">Whether to show spawn conditions for uncaught fish.</param>
    private static IEnumerable<CheckboxList> GetConditions(GameHelper gameHelper, GameLocation location, Vector2 tile, string fishAreaId, bool showUncaughtFishSpawnRules)
    {
        HashSet<string> added = [];

        foreach (FishSpawnData spawnRules in gameHelper.GetFishSpawnRules(location, tile, fishAreaId))
        {
            // parse item data
            ParsedItemData fishItemData = ItemRegistry.GetDataOrErrorItem(spawnRules.FishItem.QualifiedItemId);
            bool isCheckboxListHidden = !showUncaughtFishSpawnRules && !FishSpawnRulesField.HasPlayerCaughtFish(fishItemData);
            Checkbox[] conditions = FishSpawnRulesField.GetConditions(gameHelper, fishItemData).ToArray();

            // skip duplicates
            {
                string textRepresentation =
                    $"""
                    {fishItemData.QualifiedItemId}
                    -{string.Join("\n-", conditions.Select(checkbox => string.Join("", checkbox.Text.Select(p => p.Text))))}
                    """;

                if (!added.Add(textRepresentation))
                    continue;
            }

            // add field
            CheckboxList checkboxList = new(conditions, isCheckboxListHidden);
            checkboxList.AddIntro(fishItemData.DisplayName, new SpriteInfo(fishItemData.GetTexture(), fishItemData.GetSourceRect()));
            yield return checkboxList;
        }
    }

    /// <summary>Get the formatted checkbox conditions to display.</summary>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    /// <param name="fish">The fish item data.</param>
    private static IEnumerable<Checkbox> GetConditions(GameHelper gameHelper, ParsedItemData fish)
    {
        // get spawn data
        FishSpawnData spawnRules = gameHelper.GetFishSpawnRules(fish);
        if (spawnRules.Locations?.Any() != true)
            yield break;

        // not caught yet
        if (spawnRules.IsUnique)
            yield return FishSpawnRulesField.GetCondition(I18n.Item_FishSpawnRules_NotCaughtYet(), !FishSpawnRulesField.HasPlayerCaughtFish(fish));

        // fishing level
        if (spawnRules.MinFishingLevel > 0)
            yield return FishSpawnRulesField.GetCondition(I18n.Item_FishSpawnRules_MinFishingLevel(level: spawnRules.MinFishingLevel), Game1.player.FishingLevel >= spawnRules.MinFishingLevel);

        // extended family quest
        if (spawnRules.IsLegendaryFamily)
            yield return FishSpawnRulesField.GetCondition(I18n.Item_FishSpawnRules_ExtendedFamilyQuestActive(), Game1.player.team.SpecialOrderRuleActive("LEGENDARY_FAMILY"));

        // weather
        if (spawnRules.Weather == FishSpawnWeather.Sunny)
            yield return FishSpawnRulesField.GetCondition(I18n.Item_FishSpawnRules_WeatherSunny(), !Game1.IsRainingHere()) with { UnmetText = I18n.ScreenReader_FishWeatherSunny() };
        else if (spawnRules.Weather == FishSpawnWeather.Rainy)
            yield return FishSpawnRulesField.GetCondition(I18n.Item_FishSpawnRules_WeatherRainy(), Game1.IsRainingHere()) with { UnmetText = I18n.ScreenReader_FishWeatherRainy() };

        // time of day
        if (spawnRules.TimesOfDay?.Any() == true)
        {
            yield return FishSpawnRulesField.GetCondition(
                label: I18n.Item_FishSpawnRules_Time(
                    times: I18n.List(
                        spawnRules.TimesOfDay.Select(p => I18n.Generic_Range(CommonHelper.FormatTime(p.MinTime), CommonHelper.FormatTime(p.MaxTime)).ToString())
                    )
                ),
                isMet: spawnRules.TimesOfDay.Any(p => Game1.timeOfDay >= p.MinTime && Game1.timeOfDay <= p.MaxTime)
            ) with { UnmetText = I18n.List(spawnRules.TimesOfDay.Select(p => I18n.Generic_Range(CommonHelper.FormatTime(p.MinTime), CommonHelper.FormatTime(p.MaxTime)).ToString())) };
        }

        // locations & seasons
        if (FishSpawnRulesField.HaveSameSeasons(spawnRules.Locations))
        {
            FishSpawnLocationData firstLocation = spawnRules.Locations[0];

            // seasons
            if (firstLocation.Seasons.Count == 4)
                yield return FishSpawnRulesField.GetCondition(I18n.Item_FishSpawnRules_SeasonAny(), true);
            else
            {
                yield return FishSpawnRulesField.GetCondition(
                    label: I18n.Item_FishSpawnRules_SeasonList(
                        seasons: I18n.List(
                            firstLocation.Seasons.Select(gameHelper.TranslateSeason)
                        )
                    ),
                    isMet: firstLocation.Seasons.Contains(Game1.currentSeason)
                ) with { UnmetText = I18n.List(firstLocation.Seasons.Select(gameHelper.TranslateSeason)) };
            }

            // locations
            yield return FishSpawnRulesField.GetCondition(
                label: I18n.Item_FishSpawnRules_Locations(
                    locations: I18n.List(
                        spawnRules.Locations.Select(gameHelper.GetLocationDisplayName).OrderBy(p => p).Distinct()
                    )
                ),
                isMet: spawnRules.MatchesLocation(Game1.currentLocation.Name)
            );
        }
        else
        {
            Dictionary<string, string[]> locationsBySeason =
                (
                    from location in spawnRules.Locations
                    from season in location.Seasons
                    select new { Season = season, LocationName = gameHelper.GetLocationDisplayName(location) }
                )
                .GroupBy(p => p.Season, p => p.LocationName, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(p => p.Key, p => p.ToArray(), StringComparer.OrdinalIgnoreCase);

            // the sighted view greys out seasons other than the current one; the spoken version names the current one instead
            var summary = new List<IFormattedText> { new FormattedText(I18n.Item_FishSpawnRules_LocationsBySeason_Label()) };
            var spokenSummary = new List<string> { I18n.Item_FishSpawnRules_LocationsBySeason_Label() };
            foreach (string season in FishSpawnRulesField.Seasons)
            {
                if (locationsBySeason.TryGetValue(season, out string[]? locationNames))
                {
                    string seasonText = I18n.Item_FishSpawnRules_LocationsBySeason_SeasonLocations(season: gameHelper.TranslateSeason(season), locations: I18n.List(locationNames.OrderBy(p => p).Distinct()));
                    summary.Add(new FormattedText(
                        text: Environment.NewLine + seasonText,
                        color: season == Game1.currentSeason ? Color.Black : Color.Gray
                    ));
                    spokenSummary.Add(season == Game1.currentSeason ? I18n.ScreenReader_SeasonCurrent(seasonText) : seasonText);
                }
            }

            bool hasMatch = spawnRules.Locations.Any(p => p.LocationId == Game1.currentLocation.Name && p.Seasons.Contains(Game1.currentSeason));
            yield return new Checkbox(hasMatch, summary.ToArray()) { SpokenText = SpokenText.Sentences(spokenSummary) };
        }
    }

    /// <summary>Get a condition formatted for checkbox rendering.</summary>
    /// <param name="label">The display text for the condition.</param>
    /// <param name="isMet">Whether the condition is met.</param>
    private static Checkbox GetCondition(string label, bool isMet)
    {
        return new Checkbox(isMet, label);
    }

    /// <summary>Get a condition formatted for checkbox rendering.</summary>
    /// <param name="label">The display text for the condition.</param>
    /// <param name="isMet">Whether the condition is met.</param>
    private static Checkbox GetCondition(IEnumerable<IFormattedText> label, bool isMet)
    {
        return new Checkbox(isMet, label.ToArray());
    }

    /// <summary>Get whether all locations specify the same seasons.</summary>
    /// <param name="locations">The locations to check.</param>
    private static bool HaveSameSeasons(IEnumerable<FishSpawnLocationData> locations)
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

    /// <summary>Gets whether the player has caught a given fish.</summary>
    /// <param name="fish">The fish item data.</param>
    private static bool HasPlayerCaughtFish(ParsedItemData fish)
    {
        return Game1.player.fishCaught.ContainsKey(fish.QualifiedItemId);
    }
}
