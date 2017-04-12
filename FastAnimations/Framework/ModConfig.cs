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

        /// <summary>Whether to make eating or drinking instant.</summary>
        public bool InstantEatAndDrink { get; set; } = true;

        /// <summary>The speed multiplier fpr the geode breaking animation (1 = normal speed).</summary>
        public int BreakGeodeSpeed { get; set; } = 20;

        /// <summary>Whether to make the milk pail instant.</summary>
        public bool InstantMilkPail { get; set; } = true;

        /// <summary>Whether to make the shears instant.</summary>
        public bool InstantShears { get; set; } = true;
    }
}