using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Pathoschild.Stardew.Common;

namespace Pathoschild.Stardew.LookupAnything.Framework
{
    /// <summary>The parsed mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /****
        ** Common settings
        ****/
        /// <summary>The key bindings.</summary>
        public ModConfigKeys Controls { get; set; } = new();

        /// <summary>Whether to show gift tastes which the player hasn't learned about in-game yet (e.g. from dialogue text or experimenting).</summary>
        public bool ShowUnknownGiftTastes { get; set; } = true;

        /// <summary>Whether to show recipes the player hasn't learned in-game yet.</summary>
        public bool ShowUnknownRecipes { get; set; } = true;

        /// <summary>Whether to show puzzle solutions.</summary>
        public bool ShowPuzzleSolutions { get; set; } = true;

        /// <summary>Whether to highlight item gift tastes which haven't been revealed in the NPC profile.</summary>
        public bool HighlightUnrevealedGiftTastes { get; set; }

        /// <summary>Which gift taste levels to show in NPC and item lookups.</summary>
        public ModGiftTasteConfig ShowGiftTastes { get; set; } = new();

        /// <summary>The minimum field values needed before they're auto-collapsed.</summary>
        public ModCollapseLargeFieldsConfig CollapseLargeFields { get; set; } = new();

        /// <summary>Whether to show gift tastes that the player doesn't own somewhere in the world.</summary>
        public bool ShowUnownedGifts { get; set; } = true;

        /// <summary>Whether to close the lookup UI when the lookup key is released.</summary>
        public bool HideOnKeyUp { get; set; }

        /****
        ** Advanced settings
        ****/
        /// <summary>Whether to look up the original entity when the game spawns a temporary copy.</summary>
        /// <remarks>In some cases the game spawns a temporary entity to represent another one. For example, Abigail in the mines is actually a temporary NPC with the name 'AbigailMine', so looking her up there won't show Abigail's real info. With this option enabled, Lookup Anything will look up the original Abigail instead.</remarks>
        public bool EnableTargetRedirection { get; set; } = true;

        /// <summary>Whether to include map tiles as lookup targets.</summary>
        public bool EnableTileLookups { get; set; }

        /// <summary>Whether the menu should always be full-screen, instead of centered in the window.</summary>
        public bool ForceFullScreen { get; set; } = false;

        /// <summary>The number of pixels to shift content on each up/down scroll.</summary>
        public int ScrollAmount { get; set; } = 160;

        /// <summary>Whether to show advanced data mining fields.</summary>
        public bool ShowDataMiningFields { get; set; }

        /// <summary>Whether to show recipes involving error items.</summary>
        public bool ShowInvalidRecipes { get; set; } = false;


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
            this.Controls ??= new ModConfigKeys();
            this.ShowGiftTastes ??= new ModGiftTasteConfig();
            this.CollapseLargeFields ??= new ModCollapseLargeFieldsConfig();
        }
    }
}
