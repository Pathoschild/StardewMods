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

        /// <summary>The speed multiplier for eating or drinking (1 = normal speed).</summary>
        public int EatAndDrinkSpeed { get; set; } = 10;

        /// <summary>The speed multiplier for breaking geodes (1 = normal speed).</summary>
        public int BreakGeodeSpeed { get; set; } = 20;

        /// <summary>Whether to make the milk pail instant.</summary>
        public bool InstantMilkPail { get; set; } = true;

        /// <summary>Whether to make the shears instant.</summary>
        public bool InstantShears { get; set; } = true;
    }
}