using System;
using System.Collections.Generic;
using System.Linq;
using Pathoschild.LookupAnything.Framework.Constants;
using Pathoschild.LookupAnything.Framework.Data;
using Pathoschild.LookupAnything.Framework.Fields;
using StardewValley;
using Object = StardewValley.Object;

namespace Pathoschild.LookupAnything.Framework.Subjects
{
    /// <summary>Describes a growing crop.</summary>
    internal class CropSubject : ItemSubject
    {
        /*********
        ** Properties
        *********/
        /// <summary>The crop to look up.</summary>
        private readonly Crop Crop;

        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="crop">The lookup target.</param>
        /// <param name="obj">The item that can be harvested from the crop.</param>
        public CropSubject(Crop crop, Object obj)
            : base(obj, ObjectContext.World, knownQuality: false)
        {
            this.Crop = crop;
        }

        /// <summary>Get the data to display for this subject.</summary>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        /// <remarks>Derived from <see cref="StardewValley.Crop.harvest"/> and <see cref="StardewValley.Crop.newDay"/>.</remarks>
        public override IEnumerable<ICustomField> GetData(Metadata metadata)
        {
            Crop crop = this.Crop;

            // get harvest schedule
            int harvestablePhase = crop.phaseDays.Count - 1;
            bool canHarvestNow = (crop.currentPhase >= harvestablePhase) && (!crop.fullyGrown || crop.dayOfCurrentPhase <= 0);
            int daysToFirstHarvest = crop.phaseDays.Take(crop.phaseDays.Count - 1).Sum(); // ignore harvestable phase

            // calculate next harvest
            int daysToNextHarvest = 0;
            Tuple<string, int> dayOfNextHarvest = null;
            if (!canHarvestNow)
            {
                // calculate days until next harvest
                int daysUntilLastPhase = daysToFirstHarvest - crop.dayOfCurrentPhase - crop.phaseDays.Take(crop.currentPhase).Sum();
                {
                    // growing: days until next harvest
                    if(!crop.fullyGrown)
                        daysToNextHarvest = daysUntilLastPhase;

                    // regrowable crop harvested today
                    else if (crop.dayOfCurrentPhase >= crop.regrowAfterHarvest)
                        daysToNextHarvest = crop.regrowAfterHarvest;

                    // regrowable crop
                    else
                        daysToNextHarvest = crop.dayOfCurrentPhase; // dayOfCurrentPhase decreases to 0 when fully grown, where <=0 is harvestable
                }
                dayOfNextHarvest = GameHelper.GetDayOffset(daysToNextHarvest, metadata.Constants.DaysInSeason);
            }

            // generate next-harvest summary
            string nextHarvestSummary;
            if (canHarvestNow)
                nextHarvestSummary = "now";
            else if (Game1.currentLocation.Name != Constant.LocationNames.Greenhouse && !crop.seasonsToGrowIn.Contains(dayOfNextHarvest.Item1))
                nextHarvestSummary = $"too late in the season for the next harvest (would be on {dayOfNextHarvest.Item1} {dayOfNextHarvest.Item2})";
            else
                nextHarvestSummary = $"{dayOfNextHarvest.Item1} {dayOfNextHarvest.Item2} ({GameHelper.Pluralise(daysToNextHarvest, "tomorrow", $"in {daysToNextHarvest} days")})";

            // yield crop fields
            if (crop.dead)
                yield return new GenericField("Crop status", "This crop is dead.");
            else
            {
                yield return new GenericField("Next harvest", nextHarvestSummary, hasValue: !crop.dead);
                yield return new GenericField("Schedule", $"after {daysToFirstHarvest} {GameHelper.Pluralise(daysToFirstHarvest, "day")}" + (crop.regrowAfterHarvest != -1 ? $", then every {GameHelper.Pluralise(crop.regrowAfterHarvest, "day", $"{crop.regrowAfterHarvest} days")}" : "") + $" (in {string.Join(", ", crop.seasonsToGrowIn)})");
                yield return new GenericField("Crop drops", crop.minHarvest != crop.maxHarvest && crop.chanceForExtraCrops > 0
                    ? $"{crop.minHarvest} to {crop.maxHarvest} ({Math.Round(crop.chanceForExtraCrops * 100, 2)}% chance of extra crops)"
                    : Math.Max(crop.minHarvest, 1).ToString()
                );
            }

            // yield item fields
            foreach (ICustomField field in base.GetData(metadata))
                yield return field;
        }
    }
}