using Microsoft.Xna.Framework.Input;

namespace Pathoschild.Stardew.RotateToolbar.Framework
{
    /// <summary>The mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The keyboard input map.</summary>
        public InputMapConfiguration<Keys> Keyboard { get; set; }

        /// <summary>The controller input map.</summary>
        public InputMapConfiguration<Buttons> Controller { get; set; }

        /// <summary>Whether to deselect the current slot after rotating the toolbar.</summary>
        public bool DeselectItemOnRotate { get; set; } = false;

        /// <summary>Whether to check for updates to the mod.</summary>
        public bool CheckForUpdates { get; set; } = true;
    }
}
