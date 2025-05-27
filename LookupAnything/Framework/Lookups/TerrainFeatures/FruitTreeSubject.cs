using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using Pathoschild.Stardew.LookupAnything.Framework.DebugFields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData.FruitTrees;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.TerrainFeatures;

/// <summary>Describes a non-fruit tree.</summary>
internal class FruitTreeSubject : BaseSubject
{
    /*********
    ** Fields
    *********/
    /// <summary>The underlying target.</summary>
    private readonly FruitTree Target;

    /// <summary>The tree's tile position.</summary>
    private readonly Vector2 Tile;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    /// <param name="tree">The lookup target.</param>
    /// <param name="tile">The tree's tile position.</param>
    public FruitTreeSubject(GameHelper gameHelper, FruitTree tree, Vector2 tile)
        : base(gameHelper, I18n.FruitTree_Name(fruitName: FruitTreeSubject.GetDisplayName(tree)), null, I18n.Type_FruitTree())
    {
        this.Target = tree;
        this.Tile = tile;
    }

    /// <inheritdoc />
    public override IEnumerable<ICustomField> GetData()
    {
        FruitTree tree = this.Target;

        // get basic info
        bool isMature = tree.daysUntilMature.Value <= 0;
        bool isDead = tree.stump.Value;
        bool isStruckByLightning = tree.struckByLightningCountdown.Value > 0;

        // added by mod
        {
            IModInfo? fromMod = this.GameHelper.TryGetModFromStringId(tree.treeId.Value);
            if (fromMod != null)
                yield return new GenericField(I18n.AddedByMod(), I18n.AddedByMod_Summary(modName: fromMod.Manifest.Name));
        }

        // show next fruit
        if (isMature && !isDead)
        {
            SDate nextFruit = SDate.Now().AddDays(1);

            string label = I18n.FruitTree_NextFruit();
            if (isStruckByLightning)
                yield return new GenericField(label, I18n.FruitTree_NextFruit_StruckByLightning(count: tree.struckByLightningCountdown.Value));
            else if (!this.IsInSeason(tree, nextFruit.Season))
                yield return new GenericField(label, I18n.FruitTree_NextFruit_OutOfSeason());
            else if (tree.fruit.Count >= FruitTree.maxFruitsOnTrees)
                yield return new GenericField(label, I18n.FruitTree_NextFruit_MaxFruit());
            else
                yield return new GenericField(label, I18n.Generic_Tomorrow());
        }

        // show growth data
        if (!isMature)
        {
            SDate dayOfMaturity = SDate.Now().AddDays(tree.daysUntilMature.Value);
            string grownOnDateText = I18n.FruitTree_Growth_Summary(date: this.Stringify(dayOfMaturity));

            yield return new GenericField(I18n.FruitTree_NextFruit(), I18n.FruitTree_NextFruit_TooYoung());
            yield return new GenericField(I18n.FruitTree_Growth(), $"{grownOnDateText} ({this.GetRelativeDateStr(dayOfMaturity)})");
            if (FruitTree.IsGrowthBlocked(this.Tile, tree.Location))
                yield return new GenericField(I18n.FruitTree_Complaints(), I18n.FruitTree_Complaints_AdjacentObjects());
        }
        else
        {
            // get quality schedule
            ItemQuality currentQuality = this.GetCurrentQuality(tree, this.Constants.FruitTreeQualityGrowthTime);
            if (currentQuality == ItemQuality.Iridium)
                yield return new GenericField(I18n.FruitTree_Quality(), I18n.FruitTree_Quality_Now(quality: I18n.For(currentQuality)));
            else
            {
                string[] summary = this
                    .GetQualitySchedule(tree, currentQuality, this.Constants.FruitTreeQualityGrowthTime)
                    .Select(entry =>
                    {
                        // read schedule
                        ItemQuality quality = entry.Key;
                        int daysLeft = entry.Value;
                        SDate date = SDate.Now().AddDays(daysLeft);
                        int yearOffset = date.Year - Game1.year;

                        // generate summary line
                        if (daysLeft <= 0)
                            return $"-{I18n.FruitTree_Quality_Now(quality: I18n.For(quality))}";

                        string line = yearOffset == 1
                            ? $"-{I18n.FruitTree_Quality_OnDateNextYear(quality: I18n.For(quality), date: this.Stringify(date))}"
                            : $"-{I18n.FruitTree_Quality_OnDate(quality: I18n.For(quality), date: this.Stringify(date))}";
                        line += $" ({this.GetRelativeDateStr(daysLeft)})";

                        return line;
                    })
                    .ToArray();

                yield return new GenericField(I18n.FruitTree_Quality(), string.Join(Environment.NewLine, summary));
            }
        }

        // show seasons
        {
            IEnumerable<string>? seasons = tree.GetData()?.Seasons?.Select(I18n.GetSeasonName);
            if (seasons != null)
            {
                yield return new GenericField(
                    I18n.FruitTree_Season(),
                    I18n.FruitTree_Season_Summary(I18n.List(seasons))
                );
            }
        }

        // internal ID
        yield return new GenericField(I18n.InternalId(), tree.treeId.Value);
    }

    /// <inheritdoc />
    public override IEnumerable<IDebugField> GetDebugFields()
    {
        FruitTree target = this.Target;

        // pinned fields
        yield return new GenericDebugField("mature in", $"{target.daysUntilMature} days", pinned: true);
        yield return new GenericDebugField("growth stage", target.growthStage.Value, pinned: true);
        yield return new GenericDebugField("health", target.health.Value, pinned: true);

        // raw fields
        foreach (IDebugField field in this.GetDebugFieldsFrom(target))
            yield return field;
    }

    /// <inheritdoc />
    public override bool DrawPortrait(SpriteBatch spriteBatch, Vector2 position, Vector2 size)
    {
        this.Target.drawInMenu(spriteBatch, position, Vector2.Zero, 1, 1);
        return true;
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get the display name for a tree, without the 'tree' suffix (like "Banana" for a banana tree).</summary>
    /// <param name="tree">The fruit tree.</param>
    private static string GetDisplayName(FruitTree tree)
    {
        FruitTreeData? data = tree.GetData();
        string displayName = TokenParser.ParseText(data?.DisplayName);

        return string.IsNullOrWhiteSpace(data?.DisplayName)
            ? "???"
            : displayName;
    }

    /// <summary>Get the fruit quality produced by a tree.</summary>
    /// <param name="tree">The fruit tree.</param>
    /// <param name="daysPerQuality">The number of days before the tree begins producing a higher quality.</param>
    private ItemQuality GetCurrentQuality(FruitTree tree, int daysPerQuality)
    {
        int maturityLevel = Math.Max(0, Math.Min(3, -tree.daysUntilMature.Value / daysPerQuality));
        return maturityLevel switch
        {
            0 => ItemQuality.Normal,
            1 => ItemQuality.Silver,
            2 => ItemQuality.Gold,
            3 => ItemQuality.Iridium,
            _ => throw new NotSupportedException($"Unexpected quality level {maturityLevel}.")
        };
    }

    /// <summary>Get a schedule indicating when a fruit tree will begin producing higher-quality fruit.</summary>
    /// <param name="tree">The fruit tree.</param>
    /// <param name="currentQuality">The current quality produced by the tree.</param>
    /// <param name="daysPerQuality">The number of days before the tree begins producing a higher quality.</param>
    private IEnumerable<KeyValuePair<ItemQuality, int>> GetQualitySchedule(FruitTree tree, ItemQuality currentQuality, int daysPerQuality)
    {
        if (tree.daysUntilMature.Value > 0)
            yield break; // not mature yet

        // yield current
        yield return new KeyValuePair<ItemQuality, int>(currentQuality, 0);

        // yield future qualities
        int dayOffset = daysPerQuality - Math.Abs(tree.daysUntilMature.Value % daysPerQuality);
        foreach (ItemQuality futureQuality in new[] { ItemQuality.Silver, ItemQuality.Gold, ItemQuality.Iridium })
        {
            if (currentQuality >= futureQuality)
                continue;

            yield return new KeyValuePair<ItemQuality, int>(futureQuality, dayOffset);
            dayOffset += daysPerQuality;
        }
    }

    /// <summary>Get whether a tree produces fruit in the given season.</summary>
    /// <param name="tree">The fruit tree.</param>
    /// <param name="season">The season to check.</param>
    /// <remarks>Derived from <see cref="FruitTree.IsInSeasonHere"/> and <see cref="FruitTree.seasonUpdate"/>.</remarks>
    private bool IsInSeason(FruitTree tree, Season season)
    {
        if (tree.Location.SeedsIgnoreSeasonsHere())
            return true;

        List<Season>? growSeasons = tree.GetData()?.Seasons;
        if (growSeasons != null)
        {
            foreach (Season growSeason in growSeasons)
            {
                if (season == growSeason)
                    return true;
            }
        }

        return false;
    }
}
