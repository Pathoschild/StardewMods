using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using Pathoschild.Stardew.Common;

namespace Pathoschild.Stardew.Automate.Framework.Models
{
    /// <summary>The raw mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether Automate is enabled.</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>Whether to pull gemstones out of Junimo huts. If true, you won't be able to change Junimo colors by placing gemstones in their hut.</summary>
        public bool PullGemstonesFromJunimoHuts { get; set; } = false;

        /// <summary>The number of ticks between each automation process (60 = once per second).</summary>
        public int AutomationInterval { get; set; } = 60;

        /// <summary>The key bindings.</summary>
        public ModConfigKeys Controls { get; set; } = new();

        /// <summary>The in-game object names through which machines can connect.</summary>
        public HashSet<string> ConnectorNames { get; set; } = new() { "Workbench" };

        /// <summary>Options affecting compatibility with other mods.</summary>
        public ModCompatibilityConfig ModCompatibility { get; set; } = new();

        /// <summary>The configuration for specific machines by ID.</summary>
        public Dictionary<string, ModConfigMachine> MachineOverrides { get; set; } = new();


        /*********
        ** Public methods
        *********/
        /// <summary>Normalize the model after it's deserialized.</summary>
        /// <param name="context">The deserialization context.</param>
        [OnDeserialized]
        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse", Justification = "This is the method that prevents null values in the rest of the code.")]
        [SuppressMessage("ReSharper", "ConstantNullCoalescingCondition", Justification = "This is the method that prevents null values in the rest of the code.")]
        public void OnDeserialized(StreamingContext context)
        {
            // normalize
            this.Controls ??= new();
            this.ConnectorNames = this.ConnectorNames.ToNonNullCaseInsensitive();
            this.ModCompatibility ??= new();
            this.MachineOverrides = this.MachineOverrides.ToNonNullCaseInsensitive();

            // remove null values
            this.ConnectorNames.Remove(null!);
            foreach (string key in this.MachineOverrides.Where(p => p.Value == null).Select(p => p.Key).ToArray())
                this.MachineOverrides.Remove(key);
        }
    }
}
