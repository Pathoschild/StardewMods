namespace Pathoschild.Stardew.LookupAnything.Framework
{
    /// <summary>The parsed mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether to close the lookup UI when the lookup key is release.</summary>
        public bool HideOnKeyUp { get; set; }

        /// <summary>The amount to scroll long content on each up/down scroll.</summary>
        public int ScrollAmount { get; set; } = 160;

        /// <summary>Whether to show advanced data mining fields.</summary>
        public bool ShowDataMiningFields { get; set; }

        /// <summary>Whether to include map tiles as lookup targets.</summary>
        public bool EnableTileLookups { get; set; }

        /// <summary>The key bindings.</summary>
        public ModConfigRawKeys Controls { get; set; } = new ModConfigRawKeys();
    }
}
