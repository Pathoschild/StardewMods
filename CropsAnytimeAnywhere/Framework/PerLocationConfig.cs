using System.Runtime.Serialization;

namespace Pathoschild.Stardew.CropsAnytimeAnywhere.Framework
{
    /// <summary>Per-location mod configuration.</summary>
    internal class PerLocationConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether crops can grow here.</summary>
        public bool GrowCrops { get; set; }

        /// <summary>Whether out-of-season crops grow here too.</summary>
        public bool GrowCropsOutOfSeason { get; set; }

        /// <summary>Whether to allow hoeing anywhere.</summary>
        public ModConfigForceTillable ForceTillable { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Normalize the model after it's deserialized.</summary>
        /// <param name="context">The deserialization context.</param>
        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            this.ForceTillable ??= new();
        }
    }
}
