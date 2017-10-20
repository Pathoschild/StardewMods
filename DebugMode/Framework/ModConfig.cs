using Newtonsoft.Json;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;

namespace Pathoschild.Stardew.DebugMode.Framework
{
    /// <summary>The parsed mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Allow debug commands which are destructive. A command is considered destructive if it immediately ends the current day, randomises the player or farmhouse decorations, or crashes the game.</summary>
        public bool AllowDangerousCommands { get; set; }

        /// <summary>The control bindings.</summary>
        public ModConfigControls Controls { get; set; } = new ModConfigControls();


        /*********
        ** Nested models
        *********/
        /// <summary>A set of control bindings.</summary>
        internal class ModConfigControls
        {
            /// <summary>The control which toggles debug mode.</summary>
            [JsonConverter(typeof(StringEnumArrayConverter))]
            public SButton[] ToggleDebug { get; set; } = { SButton.OemTilde };
        }
    }
}
