using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.LookupAnything.Framework.Fields;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace Pathoschild.LookupAnything.Framework.Subjects
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

            // show growth countdown
            if (!isMature)
            {
                System.Tuple<string, int> dayOfMaturity = GameHelper.GetDayOffset(tree.daysUntilMature, metadata.Constants.DaysInSeason);
                string growthText = $"mature on {dayOfMaturity.Item1} {dayOfMaturity.Item2} ({GameHelper.Pluralise(tree.daysUntilMature, "tomorrow", $"in {tree.daysUntilMature} days")})";

                yield return new GenericField("Next fruit", "too young to bear fruit");
                yield return new GenericField("Growth", growthText);
                if (this.HasAdjacentObjects(this.Tile))
                    yield return new GenericField("Complaints", "can't grow because there are adjacent objects");
            }

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

            // show seasons
            yield return new GenericField("Season", $"{tree.fruitSeason} (or any season in greenhouse)");
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
    }
}