using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.TractorMod.Framework.Config;
using Object = StardewValley.Object;

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

        /// <summary>Which sound effects to play while riding the tractor.</summary>
        public TractorSoundType SoundEffects { get; set; } = TractorSoundType.Tractor;

        /// <summary>The volume level for the tractor sound effects, as a value between 0 (silent) and 100 (full volume). Only applicable if <see cref="SoundEffects"/> is set to <see cref="TractorSoundType.Tractor"/>.</summary>
        public int SoundEffectsVolume { get; set; } = 75;

        /// <summary>The magnetic radius when riding the tractor.</summary>
        public int MagneticRadius { get; set; } = 384;

        /// <summary>The gold price to buy a tractor garage.</summary>
        public int BuildPrice { get; set; } = 150000;

        /// <summary>The materials needed to to buy the garage.</summary>
        public Dictionary<string, int> BuildMaterials { get; set; } = new()
        {
            [Object.ironBarQID] = 20,
            [Object.iridiumBarQID] = 5,
            ["(O)787"/* battery pack */] = 5
        };

        /// <summary>Whether the player can summon a temporary tractor without building a garage first.</summary>
        public bool CanSummonWithoutGarage { get; set; }

        /// <summary>Whether to highlight the tractor radius when riding it.</summary>
        public bool HighlightRadius { get; set; }

        /// <summary>The standard attachment features to enable.</summary>
        public StandardAttachmentsConfig StandardAttachments { get; set; } = new();

        /// <summary>The key bindings.</summary>
        public ModConfigKeys Controls { get; set; } = new();

        /// <summary>The custom tools or items to allow. These must match the exact internal tool/item names (not the display names).</summary>
        public string[] CustomAttachments { get; set; } = Array.Empty<string>();

        /// <summary>Whether the player should be invincible while they're on the tractor.</summary>
        public bool InvincibleOnTractor { get; set; } = true;


        /*********
        ** Public methods
        *********/
        /// <summary>Normalize the model after it's deserialized.</summary>
        /// <param name="context">The deserialization context.</param>
        [OnDeserialized]
        [SuppressMessage("ReSharper", "NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract", Justification = SuppressReasons.MethodValidatesNullability)]
        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = SuppressReasons.UsedViaOnDeserialized)]
        public void OnDeserialized(StreamingContext context)
        {
            this.BuildMaterials ??= new Dictionary<string, int>();
            this.StandardAttachments ??= new StandardAttachmentsConfig();
            this.Controls ??= new ModConfigKeys();
            this.CustomAttachments ??= Array.Empty<string>();
        }
    }
}
