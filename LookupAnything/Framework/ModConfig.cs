using Microsoft.Xna.Framework.Input;

namespace Pathoschild.Stardew.LookupAnything.Framework
{
    /// <summary>The parsed mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The keyboard input map.</summary>
        public InputMapConfiguration<Keys> Keyboard { get; set; }

        /// <summary>The controller input map.</summary>
        public InputMapConfiguration<Buttons> Controller { get; set; }

        /// <summary>Whether to close the lookup UI when the lookup key is release.</summary>
        public bool HideOnKeyUp { get; set; }

        /// <summary>The amount to scroll long content on each up/down scroll.</summary>
        public int ScrollAmount { get; set; }

        /// <summary>Whether to check for updates to the mod.</summary>
        public bool CheckForUpdates { get; set; }

        /// <summary>Whether to show advanced data mining fields.</summary>
        public bool ShowDataMiningFields { get; set; }
    }
}