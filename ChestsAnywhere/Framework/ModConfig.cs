using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Utilities;
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

        /// <summary>Whether to enable access to the shipping bin.</summary>
        public bool EnableShippingBin { get; set; } = true;

        /// <summary>The range at which chests are accessible.</summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ChestRange Range { get; set; } = ChestRange.Unlimited;

        /// <summary>The control bindings.</summary>
        public ModConfigControls Controls { get; set; } = new ModConfigControls();

        /// <summary>The locations in which to disable remote chest lookups.</summary>
        public InvariantHashSet DisabledInLocations { get; set; } = new InvariantHashSet();


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

            /// <summary>The control which, when held, enables scrolling the chest dropdown with the mouse scroll wheel.</summary>
            [JsonConverter(typeof(StringEnumArrayConverter))]
            public SButton[] HoldToMouseWheelScrollChests { get; set; } = { SButton.LeftControl };

            /// <summary>The control which, when held, enables scrolling the category dropdown with the mouse scroll wheel.</summary>
            [JsonConverter(typeof(StringEnumArrayConverter))]
            public SButton[] HoldToMouseWheelScrollCategories { get; set; } = { SButton.LeftAlt };
        }
    }
}
