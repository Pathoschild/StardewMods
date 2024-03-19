using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Pathoschild.Stardew.Common;
using StardewValley.Extensions;

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

        /// <summary>The number of ticks between each automation process (60 = once per second).</summary>
        public int AutomationInterval { get; set; } = 60;

        /// <summary>The key bindings.</summary>
        public ModConfigKeys Controls { get; set; } = new();

        /// <summary>The in-game object names through which machines can connect.</summary>
        public HashSet<string> ConnectorNames { get; set; } = new(StringComparer.OrdinalIgnoreCase) { "Workbench" };

        /// <summary>How Junimo huts should automate gems.</summary>
        /// <remarks>The <see cref="JunimoHutBehavior.AutoDetect"/> option is equivalent to <see cref="JunimoHutBehavior.Ignore"/>.</remarks>
        public JunimoHutBehavior JunimoHutBehaviorForGems { get; set; } = JunimoHutBehavior.AutoDetect;

        /// <summary>How Junimo huts should automate fertilizer items.</summary>
        /// <remarks>The <see cref="JunimoHutBehavior.AutoDetect"/> option is equivalent to <see cref="JunimoHutBehavior.Ignore"/> (if Better Junimos is installed), else <see cref="JunimoHutBehavior.MoveIntoChests"/>.</remarks>
        public JunimoHutBehavior JunimoHutBehaviorForFertilizer { get; set; } = JunimoHutBehavior.AutoDetect;

        /// <summary>How Junimo huts should automate seed items.</summary>
        /// <remarks>The <see cref="JunimoHutBehavior.AutoDetect"/> option is equivalent to <see cref="JunimoHutBehavior.Ignore"/> (if Better Junimos is installed), else <see cref="JunimoHutBehavior.MoveIntoChests"/>.</remarks>
        public JunimoHutBehavior JunimoHutBehaviorForSeeds { get; set; } = JunimoHutBehavior.AutoDetect;

        /// <summary>Whether to log a warning if the player installs a custom-machine mod that requires a separate compatibility patch which isn't installed.</summary>
        public bool WarnForMissingBridgeMod { get; set; } = true;

        /// <summary>The configuration for specific machines by ID.</summary>
        public Dictionary<string, ModConfigMachine> MachineOverrides { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>The minimum machine processing time in minutes for which to apply fairy dust.</summary>
        public int MinMinutesForFairyDust { get; set; } = 20;


        /*********
        ** Public methods
        *********/
        /// <summary>Normalize the model after it's deserialized.</summary>
        /// <param name="context">The deserialization context.</param>
        [OnDeserialized]
        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract", Justification = SuppressReasons.MethodValidatesNullability)]
        [SuppressMessage("ReSharper", "NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract", Justification = SuppressReasons.MethodValidatesNullability)]
        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = SuppressReasons.UsedViaOnDeserialized)]
        public void OnDeserialized(StreamingContext context)
        {
            this.Controls ??= new ModConfigKeys();

            this.ConnectorNames = this.ConnectorNames.ToNonNullCaseInsensitive();
            this.ConnectorNames.RemoveWhere(string.IsNullOrWhiteSpace);

            this.MachineOverrides = this.MachineOverrides.ToNonNullCaseInsensitive();
            this.MachineOverrides.RemoveWhere(pair => pair.Value is null);
        }
    }
}
