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
        public override IEnumerable<ICustomField> GetData(Metadata metadata)
        {
            Crop crop = this.Crop;

            // get harvest schedule
            bool canRegrow = crop.regrowAfterHarvest != -1;
            int lastPhaseID = crop.phaseDays.Count - 1;
            bool canHarvestNow =
                (crop.currentPhase >= lastPhaseID) // last phase is harvestable
                && (!crop.fullyGrown || crop.dayOfCurrentPhase <= 0);
            int daysToFirstHarvest = crop.phaseDays
                .Take(crop.phaseDays.Count - 1) // last phase is harvestable
                .Sum();

            // calculate next harvest
            int daysToNextHarvest = 0;
            Tuple<string, int> dayOfNextHarvest = null;
            if (!canHarvestNow)
            {
                int daysUntilLastPhase = daysToFirstHarvest - crop.dayOfCurrentPhase - crop.phaseDays
                    .Take(crop.currentPhase)
                    .Sum();
                daysToNextHarvest = daysUntilLastPhase;
                if (crop.fullyGrown && canRegrow && crop.currentPhase >= lastPhaseID)
                    daysToNextHarvest = crop.regrowAfterHarvest; // after harvesting a regrowable crop, current phase isn't reset until the next day
                dayOfNextHarvest = GameHelper.GetDayOffset(daysToNextHarvest, metadata.Constants.DaysInSeason);
            }

            // generate next-harvest summary
            string nextHarvestSummary;
            if (canHarvestNow)
                nextHarvestSummary = "now";
            else if (Game1.currentLocation.Name != Constant.LocationNames.Greenhouse && !crop.seasonsToGrowIn.Contains(dayOfNextHarvest.Item1))
                nextHarvestSummary = $"too late in the season for the next harvest (would be on {dayOfNextHarvest.Item1} {dayOfNextHarvest.Item2})";
            else
                nextHarvestSummary = $"in {daysToNextHarvest} {GameHelper.Pluralise(daysToNextHarvest, "day")} ({dayOfNextHarvest.Item1} {dayOfNextHarvest.Item2})";

            // yield crop fields
            if (crop.dead)
                yield return new GenericField("Crop status", "This crop is dead.");
            else
            {
                yield return new GenericField("Next harvest", nextHarvestSummary, hasValue: !crop.dead);
                yield return new GenericField("Crop schedule", $"grows in {string.Join(", ", crop.seasonsToGrowIn)}; harvest after {daysToFirstHarvest} {GameHelper.Pluralise(daysToFirstHarvest, "day")}" + (crop.regrowAfterHarvest != -1 ? $", then every {crop.regrowAfterHarvest} {GameHelper.Pluralise(crop.regrowAfterHarvest, "day")}" : ""));
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