namespace TractorMod.Framework
{
    /// <summary>Configuration for a tool that can be used with the tractor.</summary>
    internal class ToolConfig
    {
        /*********
        ** Properties
        *********/
        /// <summary>The number of ticks since the tool was last used.</summary>
        private int TicksSinceLastUse;


        /*********
        ** Accessors
        *********/
        /// <summary>The name of the tool to configure.</summary>
        public string Name { get; set; }

        /// <summary>The minimum tool upgrade level for this config to apply.</summary>
        public int MinLevel { get; set; }

        /// <summary>The number of tiles on each side of the tractor to affect when seeding or fertilising (in addition to the tile under it).</summary>
        public int EffectRadius { get; set; } = 1;

        /// <summary>The multiple of ticks at which to try using the tool.</summary>
        public int ActiveEveryTickAmount { get; set; } = 1;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <remarks>This constructor is needed to read instances from the config file.</remarks>
        public ToolConfig() { }

        /// <summary>Construct an instance.</summary>
        /// <param name="name">The name of the tool to configure.</param>
        /// <param name="minLevel">The minimum tool upgrade level for this config to apply.</param>
        /// <param name="radius">The number of tiles on each side of the tractor to affect (in addition to the tile under it).</param>
        /// <param name="activeEveryTickAmount">The multiple of ticks at which to try using the tool.</param>
        public ToolConfig(string name, int minLevel = 0, int radius = 1, int activeEveryTickAmount = 1)
        {
            this.Name = name;
            this.MinLevel = minLevel;
            this.EffectRadius = radius;
            this.ActiveEveryTickAmount = activeEveryTickAmount > 0
                ? activeEveryTickAmount
                : 1;
        }

        /// <summary>Increment the internal tick counter.</summary>
        public void IncrementTicks()
        {
            this.TicksSinceLastUse++;
            this.TicksSinceLastUse %= this.ActiveEveryTickAmount;
        }

        /// <summary>Whether the tool is ready to use again.</summary>
        public bool IsReady()
        {
            return this.TicksSinceLastUse == 0;
        }
    }
}
