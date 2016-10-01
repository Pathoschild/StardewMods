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
        public InputMapConfiguration<Buttons?> Controller { get; set; }
    }
}