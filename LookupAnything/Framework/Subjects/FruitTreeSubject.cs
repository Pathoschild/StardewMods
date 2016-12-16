using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using Pathoschild.Stardew.LookupAnything.Framework.DebugFields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.LookupAnything.Framework.Subjects
{
    /// <summary>Describes a non-fruit tree.</summary>
    internal class FruitTreeSubject : BaseSubject
    {
        /*********
        ** Properties
        *********/
        /// <summary>The underlying target.</summary>
        private readonly FruitTree Target;

        /// <summary>The tree's tile position.</summary>
        private readonly Vector2 Tile;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="tree">The lookup target.</param>
        /// <param name="tile">The tree's tile position.</param>
        public FruitTreeSubject(FruitTree tree, Vector2 tile)
            : base($"{GameHelper.GetObjectBySpriteIndex(tree.indexOfFruit).Name} Tree", null, "Fruit Tree")
        {
            this.Target = tree;
            this.Tile = tile;
        }

        /// <summary>Get the data to display for this subject.</summary>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        /// <remarks>Tree growth algorithm reverse engineered from <see cref="FruitTree.dayUpdate"/>.</remarks>
        public override IEnumerable<ICustomField> GetData(Metadata metadata)
        {
            FruitTree tree = this.Target;

            // get basic info
            bool isMature = tree.daysUntilMature <= 0;
            bool isDead = tree.stump;
            bool isStruckByLightning = tree.struckByLightningCountdown > 0;

            // show next fruit
            if (isMature && !isDead)
            {
                if (isStruckByLightning)
                    yield return new GenericField("Next fruit", $"struck by lightning! Will recover in {tree.struckByLightningCountdown} days.");
                else if (Game1.currentSeason != tree.fruitSeason && !tree.greenHouseTree)
                    yield return new GenericField("Next fruit", "out of season");
                else if (tree.fruitsOnTree == FruitTree.maxFruitsOnTrees)
                    yield return new GenericField("Next fruit", "won't grow any more fruit until you harvest those it has");
                else
                    yield return new GenericField("Next fruit", "tomorrow");
            }

            // show growth data
            if (!isMature)
            {
                GameDate dayOfMaturity = GameHelper.GetDate(metadata.Constants.DaysInSeason).GetDayOffset(tree.daysUntilMature);
                string growthText = $"mature on {dayOfMaturity} ({TextHelper.Pluralise(tree.daysUntilMature, "tomorrow", $"in {tree.daysUntilMature} days")})";

                yield return new GenericField("Next fruit", "too young to bear fruit");
                yield return new GenericField("Growth", growthText);
                if (this.HasAdjacentObjects(this.Tile))
                    yield return new GenericField("Complaints", "can't grow because there are adjacent objects");
            }
            else
            {
                // get quality schedule
                ItemQuality currentQuality = this.GetCurrentQuality(tree, metadata.Constants.FruitTreeQualityGrowthTime);
                if (currentQuality == ItemQuality.Iridium)
                    yield return new GenericField("Quality", $"{currentQuality.GetName()} now");
                else
                {
                    string[] summary = this
                        .GetQualitySchedule(tree, currentQuality, metadata.Constants.FruitTreeQualityGrowthTime)
                        .Select(entry =>
                        {
                            // read schedule
                            ItemQuality quality = entry.Key;
                            int daysLeft = entry.Value;
                            GameDate date = GameHelper.GetDate(metadata.Constants.DaysInSeason).GetDayOffset(daysLeft);
                            int yearOffset = date.Year - Game1.year;

                            // generate summary line
                            if (daysLeft <= 0)
                                return $"-{quality.GetName()} now";

                            string line = $"-{quality.GetName()} on {date}";
                            if (yearOffset == 1)
                                line += " next year";
                            else if (yearOffset > 0)
                                line += $" in year {date.Year}";
                            line += $" ({TextHelper.Pluralise(daysLeft, "tomorrow", $"in {daysLeft} days")})";

                            return line;
                        })
                        .ToArray();

                    yield return new GenericField("Quality", string.Join(Environment.NewLine, summary));
                }
            }

            // show seasons
            yield return new GenericField("Season", $"{tree.fruitSeason} (or anytime in greenhouse)");
        }

        /// <summary>Get raw debug data to display for this subject.</summary>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        public override IEnumerable<IDebugField> GetDebugFields(Metadata metadata)
        {
            // raw fields
            foreach (IDebugField field in this.GetDebugFieldsFrom(this.Target))
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
            int maturityLevel = Math.Min(0, Math.Max(3, -tree.daysUntilMature / daysPerQuality));
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
            if (tree.daysUntilMature > 0)
                yield break; // not mature yet

            // yield current
            yield return new KeyValuePair<ItemQuality, int>(currentQuality, 0);

            // yield future qualities
            int dayOffset = daysPerQuality - Math.Abs(tree.daysUntilMature % daysPerQuality);
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
