using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using Pathoschild.Stardew.LookupAnything.Framework.DebugFields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.TerrainFeatures
{
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
            : base(gameHelper, I18n.FruitTree_Name(fruitName: gameHelper.GetObjectBySpriteIndex(tree.indexOfFruit.Value).DisplayName), null, I18n.Type_FruitTree())
        {
            this.Target = tree;
            this.Tile = tile;
        }

        /// <summary>Get the data to display for this subject.</summary>
        /// <remarks>Tree growth algorithm reverse engineered from <see cref="FruitTree.dayUpdate"/>.</remarks>
        public override IEnumerable<ICustomField> GetData()
        {
            FruitTree tree = this.Target;

            // get basic info
            bool isMature = tree.daysUntilMature.Value <= 0;
            bool isDead = tree.stump.Value;
            bool isStruckByLightning = tree.struckByLightningCountdown.Value > 0;

            // show next fruit
            if (isMature && !isDead)
            {
                SDate nextFruit = SDate.Now().AddDays(1);

                string label = I18n.FruitTree_NextFruit();
                if (isStruckByLightning)
                    yield return new GenericField(label, I18n.FruitTree_NextFruit_StruckByLightning(count: tree.struckByLightningCountdown.Value));
                else if (!this.IsInSeason(tree, nextFruit.Season))
                    yield return new GenericField(label, I18n.FruitTree_NextFruit_OutOfSeason());
                else if (tree.fruitsOnTree.Value == FruitTree.maxFruitsOnTrees)
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
                if (FruitTree.IsGrowthBlocked(this.Tile, tree.currentLocation))
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

            // show season
            yield return new GenericField(
                I18n.FruitTree_Season(),
                I18n.FruitTree_Season_Summary(I18n.GetSeasonName(tree.fruitSeason.Value == "island" ? "summer" : tree.fruitSeason.Value))
            );
        }

        /// <summary>Get raw debug data to display for this subject.</summary>
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

        /// <summary>Draw the subject portrait (if available).</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="size">The size of the portrait to draw.</param>
        /// <returns>Returns <c>true</c> if a portrait was drawn, else <c>false</c>.</returns>
        public override bool DrawPortrait(SpriteBatch spriteBatch, Vector2 position, Vector2 size)
        {
            this.Target.drawInMenu(spriteBatch, position, Vector2.Zero, 1, 1);
            return true;
        }


        /*********
        ** Private methods
        *********/
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
        private bool IsInSeason(FruitTree tree, string season)
        {
            if (season == tree.fruitSeason.Value || tree.currentLocation.SeedsIgnoreSeasonsHere())
                return true;

            if (tree.fruitSeason.Value == "island")
                return season == "summer" || tree.currentLocation.GetLocationContext() == GameLocation.LocationContext.Island;

            return false;
        }
    }
}
