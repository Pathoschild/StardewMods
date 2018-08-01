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

        /// <summary>The generic settings for each layer.</summary>
        public LayerConfigs Layers { get; set; } = new LayerConfigs();


        /*********
        ** Nested models
        *********/
        /// <summary>Configures the settings for each data layer.</summary>
        internal class LayerConfigs
        {
            /// <summary>The accessibility layer config.</summary>
            public LayerConfig Accessibility { get; set; } = new LayerConfig { UpdatesPerSecond = 2 };

            /// <summary>The bee house layer.</summary>
            public LayerConfig CoverageForBeeHouses { get; set; } = new LayerConfig { UpdatesPerSecond = 60 };

            /// <summary>The Junimo hut layer.</summary>
            public LayerConfig CoverageForJunimoHuts { get; set; } = new LayerConfig { UpdatesPerSecond = 60 };

            /// <summary>The scarecrow layer.</summary>
            public LayerConfig CoverageForScarecrows { get; set; } = new LayerConfig { UpdatesPerSecond = 60 };

            /// <summary>The sprinkler layer.</summary>
            public LayerConfig CoverageForSprinklers { get; set; } = new LayerConfig { UpdatesPerSecond = 60 };

            /// <summary>The fertilizer layer.</summary>
            public LayerConfig CropFertilizer { get; set; } = new LayerConfig { UpdatesPerSecond = 30 };

            /// <summary>The crop harvest layer.</summary>
            public LayerConfig CropHarvest { get; set; } = new LayerConfig { UpdatesPerSecond = 2 };

            /// <summary>The crop water layer.</summary>
            public LayerConfig CropWater { get; set; } = new LayerConfig { UpdatesPerSecond = 30 };
        }

        /// <summary>A set of control bindings.</summary>
        internal class ModConfigControls
        {
            /// <summary>The control which toggles the data layer overlay.</summary>
            [JsonConverter(typeof(StringEnumArrayConverter))]
            public SButton[] ToggleLayer { get; set; } = { SButton.F2 };

            /// <summary>The control which cycles foreward through data layers.</summary>
            [JsonConverter(typeof(StringEnumArrayConverter))]
            public SButton[] PrevLayer { get; set; } = { SButton.LeftControl, SButton.LeftShoulder };

            /// <summary>The control which cycles foreward through data layers.</summary>
            [JsonConverter(typeof(StringEnumArrayConverter))]
            public SButton[] NextLayer { get; set; } = { SButton.RightControl, SButton.RightShoulder };
        }
    }
}
