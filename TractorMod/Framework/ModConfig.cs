using Pathoschild.Stardew.TractorMod.Framework.Config;

namespace Pathoschild.Stardew.TractorMod.Framework
{
    /// <summary>The mod configuration model.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether to use the experimental feature which lets the tractor pass through trellis crops.</summary>
        public bool PassThroughTrellisCrops { get; set; }

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

        /// <summary>The standard attachment features to enable.</summary>
        public StandardAttachmentsConfig StandardAttachments { get; set; } = new StandardAttachmentsConfig();

        /// <summary>The control bindings.</summary>
        public ModConfigControls Controls { get; set; } = new ModConfigControls();

        /// <summary>The custom tools or items to allow. These must match the exact internal tool/item names (not the display names).</summary>
        public string[] CustomAttachments { get; set; } = new string[0];
    }
}
