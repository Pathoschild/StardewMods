using Microsoft.Xna.Framework.Input;

namespace Pathoschild.LookupAnything.Framework
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

        /// <summary>The amount to scroll long content when pressing a 'scroll up' or 'scroll down' control.</summary>
        public int ScrollAmount { get; set; }

        /// <summary>Whether to check for updates to the mod.</summary>
        public bool CheckForUpdates { get; set; }

        /// <summary>Whether to log debug metadata useful for troubleshooting.</summary>
        public bool DebugLog { get; set; }

        /// <summary>Whether to suppress the game's debug mode (enabled by SMAPI when you press <c>F2</c>) to prevent accidental use, which may cause unintended consequences like skipping a season.</summary>
        public bool SuppressGameDebug { get; set; }
    }
}