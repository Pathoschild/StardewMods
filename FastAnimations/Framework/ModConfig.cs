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

        /// <summary>Whether to make the blacksmith break geodes instantly.</summary>
        public bool InstantGeodes { get; set; } = true;

        /// <summary>Whether to make the milk pail instant.</summary>
        public bool InstantMilkPail { get; set; } = true;
    }
}