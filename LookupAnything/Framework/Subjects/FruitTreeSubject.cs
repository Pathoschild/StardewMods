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


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="tree">The lookup target.</param>
        /// <param name="tile">The tree's tile position.</param>
        /// <remarks>Tree growth algorithm reverse engineered from <see cref="FruitTree.dayUpdate"/>.</remarks>
        public FruitTreeSubject(FruitTree tree, Vector2 tile)
        {
            // initialise
            this.Target = tree;
            this.Initialise($"{GameHelper.GetObjectBySpriteIndex(tree.indexOfFruit).Name} Tree", null, "Fruit Tree");

            // add custom fields
            {
                // get basic info
                bool isMature = tree.daysUntilMature <= 0;
                bool isDead = tree.stump;
                bool isStruckByLightning = tree.struckByLightningCountdown > 0;

                // show growth countdown
                if (!isMature)
                {
                    System.Tuple<string, int> dayOfMaturity = GameHelper.GetDayOffset(tree.daysUntilMature);
                    string growthText = $"mature in {tree.daysUntilMature} {GameHelper.Pluralise(tree.daysUntilMature, "day")} ({dayOfMaturity.Item1} {dayOfMaturity.Item2})";
                    if (this.HasAdjacentObjects(tile))
                        growthText += " (can't grow because there are adjacent objects)";
                    this.AddCustomFields(new GenericField("Growth", growthText));
                }

                // show next fruit
                if (isMature && !isDead)
                {
                    if (isStruckByLightning)
                        this.AddCustomFields(new GenericField("Next fruit", $"struck by lightning! Will recover in {tree.struckByLightningCountdown} days."));
                    if (Game1.currentSeason != tree.fruitSeason && !tree.greenHouseTree)
                        this.AddCustomFields(new GenericField("Next fruit", $"only grows in {tree.fruitSeason} (unless it's in the greenhouse)"));
                    else if (tree.fruitsOnTree == FruitTree.maxFruitsOnTrees)
                        this.AddCustomFields(new GenericField("Next fruit", "won't grow any more fruit until you harvest those it has"));
                    else
                        this.AddCustomFields(new GenericField("Next fruit", "tomorrow"));
                }

                // show seasons
                this.AddCustomFields(new GenericField("Season", $"{tree.fruitSeason} (or any season in greenhouse)"));
            }
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