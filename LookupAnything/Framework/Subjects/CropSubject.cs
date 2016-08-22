using System;
using System.Linq;
using Pathoschild.LookupAnything.Framework.Fields;
using StardewValley;
using Object = StardewValley.Object;

namespace Pathoschild.LookupAnything.Framework.Subjects
{
    /// <summary>Describes a growing crop.</summary>
    public class CropSubject : ObjectSubject
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="crop">The underlying crop.</param>
        /// <param name="obj">The underlying object.</param>
        public CropSubject(Crop crop, Object obj)
            : base(obj)
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
            this.AddCustomFields(
                new GenericField("Crop status", crop.dead ? "dead" : "healthy"),
                new GenericField("Crop seasons", string.Join(", ", crop.seasonsToGrowIn)),
                new GenericField("Crop drops", crop.minHarvest != crop.maxHarvest && crop.chanceForExtraCrops > 0
                    ? $"{crop.minHarvest} to {crop.maxHarvest} ({crop.chanceForExtraCrops:P} chance of extra crops)"
                    : crop.minHarvest.ToString()
                ),
                new GenericField("Crop schedule", $"harvest after {daysToFirstHarvest} days" + (crop.regrowAfterHarvest != -1 ? $", then every {crop.regrowAfterHarvest} days" : "")),
                new GenericField("Crop harvest", canHarvestNow ? "now" : $"in {daysToNextHarvest} days", hasValue: !crop.dead)
            );
        }
    }
}