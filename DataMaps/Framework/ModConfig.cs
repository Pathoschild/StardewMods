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
        /// <summary>When two groups of the same color overlap, draw one border around their edges instead of their individual borders.</summary>
        public bool CombineOverlappingBorders { get; set; } = true;

        /// <summary>The data maps to enable.</summary>
        public EnabledMaps EnableMaps { get; set; } = new EnabledMaps();

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

        /// <summary>A set of data maps to enable.</summary>
        internal class EnabledMaps
        {
            /// <summary>Whether to enable the accessibility map.</summary>
            public bool Accessibility { get; set; } = true;

            /// <summary>Whether to enable the bee house map.</summary>
            public bool CoverageForBeeHouses { get; set; } = true;

            /// <summary>Whether to enable the Junimo hut map.</summary>
            public bool CoverageForJunimoHuts { get; set; } = true;

            /// <summary>Whether to enable the scarecrow map.</summary>
            public bool CoverageForScarecrows { get; set; } = true;

            /// <summary>Whether to enable the sprinkler map.</summary>
            public bool CoverageForSprinklers { get; set; } = true;

            /// <summary>Whether to enable the fertilizer map.</summary>
            public bool CropFertilizer { get; set; } = true;

            /// <summary>Whether to enable the crop water map.</summary>
            public bool CropWater { get; set; } = true;
        }
    }
}
