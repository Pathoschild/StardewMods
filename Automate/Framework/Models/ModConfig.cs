using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        /// <summary>The fields read from JSON which don't match any model field.</summary>
        [JsonExtensionData]
        public IDictionary<string, JToken> ExtensionFields { get; set; }


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

        /// <summary>Get the settings for a machine, creating a new entry if missing.</summary>
        /// <param name="id">The unique machine ID.</param>
        public ModConfigMachine GetOrAddMachineOverrides(string id)
        {
            ModConfigMachine config = this.GetMachineOverrides(id);

            if (config == null)
                this.MachineOverrides[id] = config = new ModConfigMachine();

            return config;
        }
    }
}
