using Newtonsoft.Json;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;

namespace Pathoschild.Stardew.RotateToolbar.Framework
{
    /// <summary>The mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether to deselect the current slot after rotating the toolbar.</summary>
        public bool DeselectItemOnRotate { get; set; } = false;

        /// <summary>The control bindings.</summary>
        public ModConfigControls Controls { get; set; } = new ModConfigControls();


        /*********
        ** Nested models
        *********/
        /// <summary>A set of control bindings.</summary>
        internal class ModConfigControls
        {
            /// <summary>The control which rotates the toolbar up (i.e. show the previous inventory row).</summary>
            [JsonConverter(typeof(StringEnumArrayConverter))]
            public SButton[] ShiftToPrevious { get; set; } = new SButton[0];

            /// <summary>The control which rotates the toolbar up (i.e. show the next inventory row).</summary>
            [JsonConverter(typeof(StringEnumArrayConverter))]
            public SButton[] ShiftToNext { get; set; } = { SButton.Tab };
        }
    }
}
