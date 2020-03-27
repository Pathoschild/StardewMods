using System.Collections.Generic;
using Pathoschild.Stardew.Automate.Framework.Machines.Buildings;

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

        /// <summary>Whether to pull gemstones out of Junimo huts. If true, you won't be able to change Junimo colors by placing gemstones in their hut.</summary>
        public bool PullGemstonesFromJunimoHuts { get; set; } = false;

        /// <summary>The number of ticks between each automation process (60 = once per second).</summary>
        public int AutomationInterval { get; set; } = 60;

        /// <summary>The key bindings.</summary>
        public ModConfigRawKeys Controls { get; set; } = new ModConfigRawKeys();

        /// <summary>The in-game object names through which machines can connect.</summary>
        public string[] ConnectorNames { get; set; } = { "Workbench" };

        /// <summary>Options affecting compatibility with other mods.</summary>
        public ModCompatibilityConfig ModCompatibility { get; set; } = new ModCompatibilityConfig();

        /// <summary>The priority order in which to process machines. Machines have a default priority of 0, and higher values are processed first.</summary>
        public IDictionary<string, int> MachinePriority = new Dictionary<string, int>
        {
            [nameof(ShippingBinMachine)] = -1
        };
    }
}
