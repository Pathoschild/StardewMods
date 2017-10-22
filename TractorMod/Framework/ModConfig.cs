using Newtonsoft.Json;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;

namespace Pathoschild.Stardew.TractorMod.Framework
{
    /// <summary>The mod configuration model.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether the axe clears fruit trees.</summary>
        public bool AxeCutsFruitTrees { get; set; } = false;

        /// <summary>Whether the axe clears non-fruit trees.</summary>
        public bool AxeCutsTrees { get; set; } = false;

        /// <summary>Whether the axe clears live crops.</summary>
        public bool AxeClearsCrops { get; set; } = false;

        /// <summary>Whether the tractor can clear hoed dirt tiles when the pickaxe is selected.</summary>
        public bool PickaxeClearsDirt { get; set; } = true;

        /// <summary>Whether the tractor can break paths and flooring when the pickaxe is selected.</summary>
        public bool PickaxeBreaksFlooring { get; set; } = false;

        /// <summary>Whether to use the experimental feature which lets the tractor pass through trellis crops.</summary>
        public bool PassThroughTrellisCrops { get; set; }

        /// <summary>The custom tools to allow. These must match the exact in-game tool names.</summary>
        public string[] CustomTools { get; set; } = new string[0];

        /// <summary>The number of tiles on each side of the tractor to affect (in addition to the tile under it).</summary>
        public int Distance { get; set; } = 1;

        /// <summary>The speed modifier when riding the tractor.</summary>
        public int TractorSpeed { get; set; } = -2;

        /// <summary>The magnetic radius when riding the tractor.</summary>
        public int MagneticRadius { get; set; } = 384;

        /// <summary>Whether you need to provide building resources to buy the garage.</summary>
        public bool BuildUsesResources { get; set; } = true;

        /// <summary>The gold price to buy a tractor garage.</summary>
        public int BuildPrice { get; set; } = 150000;

        /// <summary>Whether to highlight the tractor radius when riding it.</summary>
        public bool HighlightRadius { get; set; }

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
            public SButton[] SummonTractor { get; set; } = { SButton.B };

            /// <summary>A button which activates the tractor when held, or none to activate automatically.</summary>
            [JsonConverter(typeof(StringEnumArrayConverter))]
            public SButton[] HoldToActivate { get; set; } = null;


        }
    }
}
