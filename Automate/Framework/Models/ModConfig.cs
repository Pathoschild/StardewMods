using Newtonsoft.Json;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;

namespace Pathoschild.Stardew.Automate.Framework.Models
{
    /// <summary>The raw mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Write more trace information to the log.</summary>
        public bool VerboseLogging { get; set; } = false;

        /// <summary>The number of ticks between each automation process (60 = once per second).</summary>
        public int AutomationInterval { get; set; } = 60;

        /// <summary>The control bindings.</summary>
        public ModConfigControls Controls { get; set; } = new ModConfigControls();


        /*********
        ** Nested models
        *********/
        /// <summary>A set of control bindings.</summary>
        internal class ModConfigControls
        {
            /// <summary>The button which toggles the automation overlay.</summary>
            [JsonConverter(typeof(StringEnumArrayConverter))]
            public SButton[] ToggleOverlay { get; set; } = { SButton.U };
        }
    }
}
