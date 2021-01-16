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

        /// <summary>The configuration for specific machines by ID.</summary>
        public IDictionary<string, ModConfigMachine> MachineOverrides { get; set; } = new Dictionary<string, ModConfigMachine>
        {
            [BaseMachine.GetDefaultMachineId(typeof(ShippingBinMachine))] = new ModConfigMachine { Priority = -1 }
        };


        /*********
        ** Public methods
        *********/
        /// <summary>Get the settings for a machine.</summary>
        /// <param name="id">The unique machine ID.</param>
        public ModConfigMachine GetMachineOverrides(string id)
        {
            return this.MachineOverrides.TryGetValue(id, out ModConfigMachine config)
                ? config
                : null;
        }
    }
}
