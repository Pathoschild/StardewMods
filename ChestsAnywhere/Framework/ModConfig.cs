using Microsoft.Xna.Framework.Input;

namespace ChestsAnywhere.Framework
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

        /// <summary>Whether to check for updates to the mod.</summary>
        public bool CheckForUpdates { get; set; }

        /// <summary>Whether to show the chest name in a tooltip when you point at a chest.</summary>
        public bool ShowHoverTooltips { get; set; }
    }
}