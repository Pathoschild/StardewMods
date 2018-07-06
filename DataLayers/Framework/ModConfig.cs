using Newtonsoft.Json;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;

namespace Pathoschild.Stardew.DataLayers.Framework
{
    /// <summary>The parsed mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>When two groups of the same color overlap, draw one border around their edges instead of their individual borders.</summary>
        public bool CombineOverlappingBorders { get; set; } = true;

        /// <summary>The control bindings.</summary>
        public ModConfigControls Controls { get; set; } = new ModConfigControls();

        /// <summary>The number of updates per second for each map.</summary>
        public MapConfigs DataMaps { get; set; } = new MapConfigs();


        /*********
        ** Nested models
        *********/
        /// <summary>Configures the settings for each data map.</summary>
        internal class MapConfigs
        {
            /// <summary>The accessibility map config.</summary>
            public MapConfig Accessibility { get; set; } = new MapConfig { UpdatesPerSecond = 2 };

            /// <summary>The bee house map.</summary>
            public MapConfig CoverageForBeeHouses { get; set; } = new MapConfig { UpdatesPerSecond = 60 };

            /// <summary>The Junimo hut map.</summary>
            public MapConfig CoverageForJunimoHuts { get; set; } = new MapConfig { UpdatesPerSecond = 60 };

            /// <summary>The scarecrow map.</summary>
            public MapConfig CoverageForScarecrows { get; set; } = new MapConfig { UpdatesPerSecond = 60 };

            /// <summary>The sprinkler map.</summary>
            public MapConfig CoverageForSprinklers { get; set; } = new MapConfig { UpdatesPerSecond = 60 };

            /// <summary>The fertilizer map.</summary>
            public MapConfig CropFertilizer { get; set; } = new MapConfig { UpdatesPerSecond = 30 };

            /// <summary>The crop water map.</summary>
            public MapConfig CropWater { get; set; } = new MapConfig { UpdatesPerSecond = 30 };
        }

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
