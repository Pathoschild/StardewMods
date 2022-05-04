namespace Pathoschild.Stardew.CropsAnytimeAnywhere.Framework
{
    /// <summary>Per-location mod configuration.</summary>
    internal class PerLocationConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether crops can grow here.</summary>
        public bool GrowCrops { get; }

        /// <summary>Whether out-of-season crops grow here too.</summary>
        public bool GrowCropsOutOfSeason { get; }

        /// <summary>Whether to allow hoeing anywhere.</summary>
        public ModConfigForceTillable ForceTillable { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="growCrops">Whether crops can grow here.</param>
        /// <param name="growCropsOutOfSeason">Whether out-of-season crops grow here too.</param>
        /// <param name="forceTillable">Whether to allow hoeing anywhere.</param>
        public PerLocationConfig(bool growCrops, bool growCropsOutOfSeason, ModConfigForceTillable? forceTillable)
        {
            this.GrowCrops = growCrops;
            this.GrowCropsOutOfSeason = growCropsOutOfSeason;
            this.ForceTillable = forceTillable ?? new(
                dirt: true,
                grass: true,
                stone: false,
                other: false
            );
        }
    }
}
