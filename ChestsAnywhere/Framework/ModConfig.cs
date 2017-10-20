using Newtonsoft.Json;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework
{
    /// <summary>The mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether to show the chest name in a tooltip when you point at a chest.</summary>
        public bool ShowHoverTooltips { get; set; } = true;

        /// <summary>The control bindings.</summary>
        public ModConfigControls Controls { get; set; } = new ModConfigControls();


        /*********
        ** Nested models
        *********/
        /// <summary>A set of control bindings.</summary>
        internal class ModConfigControls
        {
            /// <summary>The control which toggles the chest UI.</summary>
            [JsonConverter(typeof(StringEnumArrayConverter))]
            public SButton[] Toggle { get; set; } = { SButton.B };

            /// <summary>The control which navigates to the previous chest.</summary>
            [JsonConverter(typeof(StringEnumArrayConverter))]
            public SButton[] PrevChest { get; set; } = { SButton.Left, SButton.LeftShoulder };

            /// <summary>The control which navigates to the next chest.</summary>
            [JsonConverter(typeof(StringEnumArrayConverter))]
            public SButton[] NextChest { get; set; } = { SButton.Right, SButton.RightShoulder };

            /// <summary>The control which navigates to the previous category.</summary>
            [JsonConverter(typeof(StringEnumArrayConverter))]
            public SButton[] PrevCategory { get; set; } = { SButton.Up, SButton.LeftTrigger };

            /// <summary>The control which navigates to the next category.</summary>
            [JsonConverter(typeof(StringEnumArrayConverter))]
            public SButton[] NextCategory { get; set; } = { SButton.Down, SButton.RightTrigger };

            /// <summary>The control which edits the current chest.</summary>
            [JsonConverter(typeof(StringEnumArrayConverter))]
            public SButton[] EditChest { get; set; } = new SButton[0];

            /// <summary>The control which sorts items in the chest.</summary>
            [JsonConverter(typeof(StringEnumArrayConverter))]
            public SButton[] SortItems { get; set; } = new SButton[0];
        }
    }
}
