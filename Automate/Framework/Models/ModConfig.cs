using System.Collections.Generic;

namespace Pathoschild.Stardew.Automate.Framework.Models
{
    /// <summary>The raw mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether to pull gemstones out of Junimo huts. If true, you won't be able to change Junimo colors by placing gemstones in their hut.</summary>
        public bool PullGemstonesFromJunimoHuts { get; set; } = false;

        /// <summary>The number of ticks between each automation process (60 = once per second).</summary>
        public int AutomationInterval { get; set; } = 60;

        /// <summary>The key bindings.</summary>
        public ModConfigKeys Controls { get; set; } = new();

        /// <summary>The in-game object names through which machines can connect.</summary>
        public string[] ConnectorNames { get; set; } = { "Workbench" };

        /// <summary>Options affecting compatibility with other mods.</summary>
        public ModCompatibilityConfig ModCompatibility { get; set; } = new();

        /// <summary>The configuration for specific machines by ID.</summary>
        public IDictionary<string, ModConfigMachine> MachineOverrides { get; set; } = new Dictionary<string, ModConfigMachine>();
    }
}
