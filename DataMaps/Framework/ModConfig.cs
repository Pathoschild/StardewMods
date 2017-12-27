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
