using System.Collections.Generic;
using Pathoschild.Stardew.TractorMod.Framework.Config;
using StardewValley;

namespace Pathoschild.Stardew.TractorMod.Framework
{
    /// <summary>The mod configuration model.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The number of tiles on each side of the tractor to affect (in addition to the tile under it).</summary>
        public int Distance { get; set; } = 1;

        /// <summary>The speed modifier when riding the tractor.</summary>
        public int TractorSpeed { get; set; } = -2;

        /// <summary>The magnetic radius when riding the tractor.</summary>
        public int MagneticRadius { get; set; } = 384;

        /// <summary>The gold price to buy a tractor garage.</summary>
        public int BuildPrice { get; set; } = 150000;

        /// <summary>The materials needed to to buy the garage.</summary>
        public Dictionary<int, int> BuildMaterials { get; set; } = new Dictionary<int, int>
        {
            [Object.ironBar] = 20,
            [Object.iridiumBar] = 5,
            [787/* battery pack */] = 5
        };

        /// <summary>Whether to highlight the tractor radius when riding it.</summary>
        public bool HighlightRadius { get; set; }

        /// <summary>The standard attachment features to enable.</summary>
        public StandardAttachmentsConfig StandardAttachments { get; set; } = new StandardAttachmentsConfig();

        /// <summary>The key bindings.</summary>
        public ModConfigRawKeys Controls { get; set; } = new ModConfigRawKeys();

        /// <summary>The custom tools or items to allow. These must match the exact internal tool/item names (not the display names).</summary>
        public string[] CustomAttachments { get; set; } = new string[0];

        /// <summary>Whether the player should be invincible while they're on the tractor.</summary>
        public bool InvincibleOnTractor { get; set; } = true;
    }
}
