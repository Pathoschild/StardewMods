using System;
using System.Linq;
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
            : base(obj, knownQuality: false, metadata: metadata)
        {
            // get harvest schedule
            bool canRegrow = crop.regrowAfterHarvest != -1;
            bool canHarvestNow =
                (crop.currentPhase >= crop.phaseDays.Count - 1) // last phase is harvestable
                && (!crop.fullyGrown || crop.dayOfCurrentPhase <= 0);
            int daysToFirstHarvest = crop.phaseDays
                .Take(crop.phaseDays.Count - 1) // last phase is harvestable
                .Sum();

            // calculate next harvest
            int daysToNextHarvest = crop.fullyGrown && canRegrow
                ? crop.regrowAfterHarvest - crop.dayOfCurrentPhase
                : daysToFirstHarvest - crop.dayOfCurrentPhase;
            if (daysToNextHarvest == 0 && crop.fullyGrown && canRegrow)
                daysToNextHarvest = crop.regrowAfterHarvest; // after harvesting a regrowable crop, day of current phase is set to 0 until the next day

            // add fields
            if (crop.dead)
                this.AddCustomFields(new GenericField("Crop status", "This crop is dead."));
            else
            {
                this.AddCustomFields(
                    new GenericField("Crop schedule", $"grows in {string.Join(", ", crop.seasonsToGrowIn)}; harvest after {daysToFirstHarvest} days" + (crop.regrowAfterHarvest != -1 ? $", then every {crop.regrowAfterHarvest} days" : "")),
                    new GenericField("Next harvest", canHarvestNow ? "now" : $"in {daysToNextHarvest} days", hasValue: !crop.dead),
                    new GenericField("Crop drops", crop.minHarvest != crop.maxHarvest && crop.chanceForExtraCrops > 0
                        ? $"{crop.minHarvest} to {crop.maxHarvest} ({Math.Round(crop.chanceForExtraCrops * 100, 2)}% chance of extra crops)"
                        : Math.Max(crop.minHarvest, 1).ToString()
                    )
                );
            }
        }
    }
}