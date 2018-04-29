namespace RestAnywhere.Framework
{
    /// <summary>The mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Properties
        *********/
        /// <summary>Whether the values are in milliseconds.</summary>
        private bool IsMilliseconds;


        /*********
        ** Accessors
        *********/
        /// <summary>The number of seconds the player should stop doing anything before they're considered to be resting.</summary>
        public double RestDelay { get; set; } = 2;

        /// <summary>The health regeneration per second.</summary>
        public RegenConfig HealthRegen { get; set; } = new RegenConfig
        {
            Rest = 6,
            Run = 0
        };

        /// <summary>The stamina regeneration per second.</summary>
        public RegenConfig StaminaRegen { get; set; } = new RegenConfig
        {
            Rest = 6,
            Run = -1
        };


        /*********
        ** Public methods
        *********/
        /// <summary>Convert all fields from seconds to milliseconds.</summary>
        public void ConvertToMilliseconds()
        {
            if (!this.IsMilliseconds)
            {
                this.RestDelay /= 1000;
                foreach (RegenConfig regen in new[] { this.HealthRegen, this.StaminaRegen })
                {
                    regen.Rest /= 1000;
                    regen.Run /= 1000;
                }
                this.IsMilliseconds = true;
            }
        }
    }
}
