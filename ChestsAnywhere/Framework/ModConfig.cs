using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Pathoschild.Stardew.Common.Utilities;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework
{
    /// <summary>The mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether to show the chest name in a tooltip when you point at a chest.</summary>
        public bool ShowHoverTooltips { get; set; } = true;

        /// <summary>Whether to enable access to the shipping bin.</summary>
        public bool EnableShippingBin { get; set; } = true;

        /// <summary>Whether to add an 'organise' button in chest UIs for the player inventory.</summary>
        public bool AddOrganisePlayerInventoryButton { get; set; } = true;

        /// <summary>The range at which chests are accessible.</summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ChestRange Range { get; set; } = ChestRange.Unlimited;

        /// <summary>The key bindings.</summary>
        public ModConfigRawKeys Controls { get; set; } = new ModConfigRawKeys();

        /// <summary>The locations in which to disable remote chest lookups.</summary>
        public InvariantHashSet DisabledInLocations { get; set; } = new InvariantHashSet();
    }
}
