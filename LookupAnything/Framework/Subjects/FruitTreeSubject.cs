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
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.LookupAnything.Framework.Subjects
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
        /// <param name="codex">Provides subject entries for target values.</param>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="tree">The lookup target.</param>
        /// <param name="tile">The tree's tile position.</param>
        /// <param name="translations">Provides translations stored in the mod folder.</param>
        public FruitTreeSubject(SubjectFactory codex, GameHelper gameHelper, FruitTree tree, Vector2 tile, ITranslationHelper translations)
            : base(codex, gameHelper, L10n.FruitTree.Name(fruitName: gameHelper.GetObjectBySpriteIndex(tree.indexOfFruit.Value).DisplayName), null, L10n.Types.FruitTree(), translations)
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

                string label = L10n.FruitTree.NextFruit();
                if (isStruckByLightning)
                    yield return new GenericField(this.GameHelper, label, L10n.FruitTree.NextFruitStruckByLightning(count: tree.struckByLightningCountdown.Value));
                else if (!tree.GreenHouseTree && nextFruit.Season != tree.fruitSeason.Value)
                    yield return new GenericField(this.GameHelper, label, L10n.FruitTree.NextFruitOutOfSeason());
                else if (tree.fruitsOnTree.Value == FruitTree.maxFruitsOnTrees)
                    yield return new GenericField(this.GameHelper, label, L10n.FruitTree.NextFruitMaxFruit());
                else
                    yield return new GenericField(this.GameHelper, label, L10n.Generic.Tomorrow());
            }

            // show growth data
            if (!isMature)
            {
                SDate dayOfMaturity = SDate.Now().AddDays(tree.daysUntilMature.Value);
                string grownOnDateText = L10n.FruitTree.GrowthSummary(date: this.Stringify(dayOfMaturity));

                yield return new GenericField(this.GameHelper, L10n.FruitTree.NextFruit(), L10n.FruitTree.NextFruitTooYoung());
                yield return new GenericField(this.GameHelper, L10n.FruitTree.Growth(), $"{grownOnDateText} ({this.GetRelativeDateStr(dayOfMaturity)})");
                if (this.HasAdjacentObjects(this.Tile))
                    yield return new GenericField(this.GameHelper, L10n.FruitTree.Complaints(), L10n.FruitTree.ComplaintsAdjacentObjects());
            }
            else
            {
                // get quality schedule
                ItemQuality currentQuality = this.GetCurrentQuality(tree, this.Constants.FruitTreeQualityGrowthTime);
                if (currentQuality == ItemQuality.Iridium)
                    yield return new GenericField(this.GameHelper, L10n.FruitTree.Quality(), L10n.FruitTree.QualityNow(quality: currentQuality));
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
                                return $"-{L10n.FruitTree.QualityNow(quality: quality)}";

                            string line;
                            if (yearOffset == 0)
                                line = $"-{L10n.FruitTree.QualityOnDate(quality: quality, date: this.Stringify(date))}";
                            else if (yearOffset == 1)
                                line = $"-{L10n.FruitTree.QualityOnDateNextYear(quality: quality, date: this.Stringify(date))}";
                            else
                                line = $"-{L10n.FruitTree.QualityOnDate(quality: quality, date: date.ToLocaleString(withYear: true))}";

                            line += $" ({this.GetRelativeDateStr(daysLeft)})";

                            return line;
                        })
                        .ToArray();

                    yield return new GenericField(this.GameHelper, L10n.FruitTree.Quality(), string.Join(Environment.NewLine, summary));
                }
            }

            // show season
            yield return new GenericField(this.GameHelper, L10n.FruitTree.Season(), L10n.FruitTree.SeasonSummary(this.Text.GetSeasonName(tree.fruitSeason.Value)));
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
        /// <summary>Whether there are adjacent objects that prevent growth.</summary>
        /// <param name="position">The tree's position in the current location.</param>
        private bool HasAdjacentObjects(Vector2 position)
        {
            GameLocation location = Game1.currentLocation;
            return (
                from adjacentTile in Utility.getSurroundingTileLocationsArray(position)
                let isOccupied = location.isTileOccupied(adjacentTile)
                let isEmptyDirt = location.terrainFeatures.ContainsKey(adjacentTile) && location.terrainFeatures[adjacentTile] is HoeDirt && ((HoeDirt)location.terrainFeatures[adjacentTile])?.crop == null
                select isOccupied && !isEmptyDirt
            ).Any(p => p);
        }

        /// <summary>Get the fruit quality produced by a tree.</summary>
        /// <param name="tree">The fruit tree.</param>
        /// <param name="daysPerQuality">The number of days before the tree begins producing a higher quality.</param>
        private ItemQuality GetCurrentQuality(FruitTree tree, int daysPerQuality)
        {
            int maturityLevel = Math.Max(0, Math.Min(3, -tree.daysUntilMature.Value / daysPerQuality));
            switch (maturityLevel)
            {
                case 0:
                    return ItemQuality.Normal;
                case 1:
                    return ItemQuality.Silver;
                case 2:
                    return ItemQuality.Gold;
                case 3:
                    return ItemQuality.Iridium;
                default:
                    throw new NotSupportedException($"Unexpected quality level {maturityLevel}.");
            }
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
    }
}
