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

        /// <summary>The speed multiplier for eating or drinking.</summary>
        public int EatAndDrinkSpeed { get; set; } = 10;

        /// <summary>The speed multiplier for breaking geodes.</summary>
        public int BreakGeodeSpeed { get; set; } = 20;

        /// <summary>The speed multiplier for milking.</summary>
        public int MilkSpeed { get; set; } = 5;

        /// <summary>Whether to make the shears instant.</summary>
        public bool InstantShears { get; set; } = true;
    }
}