namespace Pathoschild.Stardew.FastAnimations.Framework
{
    /// <summary>The mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether to check for updates to the mod.</summary>
        public bool CheckForUpdates { get; set; } = true;

        /// <summary>The speed multiplier for eating and drinking.</summary>
        public int EatAndDrinkSpeed { get; set; } = 2;

        /// <summary>The speed multiplier for breaking geodes.</summary>
        public int BreakGeodeSpeed { get; set; } = 20;

        /// <summary>The speed multiplier for fishing.</summary>
        public int FishingSpeed { get; set; } = 2;
         
        /// <summary>The speed multiplier for milking.</summary>
        public int MilkSpeed { get; set; } = 2;

        /// <summary>The speed multiplier for shearing.</summary>
        public int ShearSpeed { get; set; } = 2;

        /// <summary>The speed multiplier for falling trees.</summary>
        public int TreeFallSpeed { get; set; } = 2;

        /// <summary>The speed multiplier for nearly everything else the player does..</summary>
        public int FarmerSpeed { get; set; } = 2;
    }
}
