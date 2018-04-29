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
        /// <summary>The health regeneration per second.</summary>
        public RegenConfig HealthRegenPerSecond { get; set; } = new RegenConfig
        {
            Rest = 6,
            Walk = 0,
            Run = 0
        };

        /// <summary>The stamina regeneration per second.</summary>
        public RegenConfig StaminaRegenPerSecond { get; set; } = new RegenConfig
        {
            Rest = 6,
            Walk = -0.25f,
            Run = -1
        };

        /// <summary>The number of seconds the player should stop doing anything before they're considered to be resting.</summary>
        public double RestDelay { get; set; } = 2;

        /// <summary>The number of seconds to accumulate regen before applying it.</summary>
        public double RegenDelay { get; set; } = 1;

        /*********
        ** Public methods
        *********/
        /// <summary>Convert all fields from seconds to milliseconds.</summary>
        public void ConvertToMilliseconds()
        {
            if (!this.IsMilliseconds)
            {
                this.RestDelay /= 1000;
                this.RegenDelay /= 1000;
                foreach (RegenConfig regen in new[] { this.HealthRegenPerSecond, this.StaminaRegenPerSecond })
                {
                    regen.Rest /= 1000;
                    regen.Run /= 1000;
                }
                this.IsMilliseconds = true;
            }
        }
    }
}
