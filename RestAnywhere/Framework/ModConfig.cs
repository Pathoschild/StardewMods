namespace RestAnywhere.Framework
{
    /// <summary>The mod configuration.</summary>
    internal class ModConfig
    {
        /// <summary>The number of seconds the player should stop doing anything before they're considered to be resting.</summary>
        public double RestDelay { get; set; } = 2;

        /// <summary>The health regeneration to apply each second when resting.</summary>
        public double HealthRegen { get; set; } = 6;

        /// <summary>The stamina regeneration to apply each second when resting.</summary>
        public double StaminaRegen { get; set; } = 6;
    }
}
