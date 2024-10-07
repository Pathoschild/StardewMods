using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Pathoschild.Stardew.Common;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework
{
    /// <summary>The mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /****
        ** General options
        ****/
        /// <summary>Whether to show the chest name in a tooltip when you point at a chest.</summary>
        public bool ShowHoverTooltips { get; set; } = true;

        /// <summary>Whether to enable access to the shipping bin.</summary>
        public bool EnableShippingBin { get; set; } = true;

        /// <summary>Whether sprinkler attachments like enrichers can be accessed remotely.</summary>
        public bool EnableSprinklerAttachments = false;

        /// <summary>Whether to add an 'organize' button in chest UIs for the player inventory.</summary>
        public bool AddOrganizePlayerInventoryButton { get; set; } = true;

        /****
        ** Remote access
        ****/
        /// <summary>The range at which chests are accessible.</summary>
        public ChestRange Range { get; set; } = ChestRange.Unlimited;

        /// <summary>The locations in which to disable remote chest lookups.</summary>
        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Auto)]
        public HashSet<string> DisabledInLocations { get; } = new(StringComparer.OrdinalIgnoreCase);

        /****
        ** Hotkey to open chest UI
        ****/
        /// <summary>When the player presses the 'toggle UI' hotkey, whether to reopen the last chest that was selected directly in the current session.</summary>
        public bool ReopenLastChest { get; set; } = true;

        /// <summary>When the player presses the 'toggle UI' hotkey, show the chests in this category by default. This applies after <see cref="ReopenLastChest"/>.</summary>
        public string? DefaultCategory { get; set; } = null;

        /****
        ** Controls
        ****/
        /// <summary>The key bindings.</summary>
        public ModConfigKeys Controls { get; set; } = new();


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

            this.DisabledInLocations.RemoveWhere(string.IsNullOrWhiteSpace);
        }
    }
}
