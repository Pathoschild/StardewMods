namespace Pathoschild.Stardew.Automate.Framework.Models
{
    /// <summary>The raw mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether to treat the shipping bin as a machine that can be automated.</summary>
        public bool AutomateShippingBin { get; set; } = true;

        /// <summary>The number of ticks between each automation process (60 = once per second).</summary>
        public int AutomationInterval { get; set; } = 60;

        /// <summary>The key bindings.</summary>
        public ModConfigRawKeys Controls { get; set; } = new ModConfigRawKeys();

        /// <summary>The in-game object names through which machines can connect.</summary>
        public string[] ConnectorNames { get; set; } = new string[0];

        /// <summary>Options affecting compatibility with other mods.</summary>
        public ModCompatibilityConfig ModCompatibility { get; set; } = new ModCompatibilityConfig();
    }
}
