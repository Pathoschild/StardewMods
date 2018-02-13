using Newtonsoft.Json;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;

namespace Pathoschild.Stardew.DataMaps.Framework
{
    /// <summary>The parsed mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The control bindings.</summary>
        public ModConfigControls Controls { get; set; } = new ModConfigControls();

        /// <summary>When two groups of the same color overlap, draw one border around their edges instead of their individual borders.</summary>
        public bool CombineOverlappingBorders { get; set; } = true;

        /// <summary>The data maps to disable.</summary>
        public string[] DisabledMaps { get; set; } = new string[0];

        /*********
        ** Nested models
        *********/
        /// <summary>A set of control bindings.</summary>
        internal class ModConfigControls
        {
            /// <summary>The control which toggles the data map overlay.</summary>
            [JsonConverter(typeof(StringEnumArrayConverter))]
            public SButton[] ToggleMap { get; set; } = { SButton.F2 };

            /// <summary>The control which cycles foreward through data maps.</summary>
            [JsonConverter(typeof(StringEnumArrayConverter))]
            public SButton[] PrevMap { get; set; } = { SButton.LeftControl, SButton.LeftShoulder };

            /// <summary>The control which cycles foreward through data maps.</summary>
            [JsonConverter(typeof(StringEnumArrayConverter))]
            public SButton[] NextMap { get; set; } = { SButton.RightControl, SButton.RightShoulder };
        }
    }
}
