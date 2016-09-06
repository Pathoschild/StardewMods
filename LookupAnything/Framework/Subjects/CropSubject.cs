using System;
using System.Linq;
using Pathoschild.LookupAnything.Framework.Data;
using Pathoschild.LookupAnything.Framework.Fields;
using StardewValley;
using Object = StardewValley.Object;

namespace Pathoschild.LookupAnything.Framework.Subjects
{
    /// <summary>Describes a growing crop.</summary>
    public class CropSubject : ItemSubject
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="crop">The underlying crop.</param>
        /// <param name="obj">The underlying object.</param>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        public CropSubject(Crop crop, Object obj, Metadata metadata)
            : base(new Target<Item>(TargetType.InventoryItem, obj), ObjectContext.World, knownQuality: false, metadata: metadata)
        {
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
                dayOfNextHarvest = GameHelper.GetDayOffset(daysToNextHarvest);
            }
            
            // generate next-harvest summary
            string nextHarvestSummary;
            if (canHarvestNow)
                nextHarvestSummary = "now";
            else if(!crop.seasonsToGrowIn.Contains(dayOfNextHarvest.Item1))
                nextHarvestSummary = $"too late in the season for the next harvest (would be on {dayOfNextHarvest.Item1} {dayOfNextHarvest.Item2})";
            else
                nextHarvestSummary = $"in {daysToNextHarvest} {GameHelper.Pluralise(daysToNextHarvest, "day")} ({dayOfNextHarvest.Item1} {dayOfNextHarvest.Item2})";

            // add fields
            if (crop.dead)
                this.AddCustomFields(new GenericField("Crop status", "This crop is dead."));
            else
            {
                this.AddCustomFields(
                    new GenericField("Crop schedule", $"grows in {string.Join(", ", crop.seasonsToGrowIn)}; harvest after {daysToFirstHarvest} {GameHelper.Pluralise(daysToFirstHarvest, "day")}" + (crop.regrowAfterHarvest != -1 ? $", then every {crop.regrowAfterHarvest} {GameHelper.Pluralise(crop.regrowAfterHarvest, "day")}" : "")),
                    new GenericField("Next harvest", nextHarvestSummary, hasValue: !crop.dead),
                    new GenericField("Crop drops", crop.minHarvest != crop.maxHarvest && crop.chanceForExtraCrops > 0
                        ? $"{crop.minHarvest} to {crop.maxHarvest} ({Math.Round(crop.chanceForExtraCrops * 100, 2)}% chance of extra crops)"
                        : Math.Max(crop.minHarvest, 1).ToString()
                    )
                );
            }
        }
    }
}