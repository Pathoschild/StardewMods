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
        public InputMapConfiguration<Buttons?> Controller { get; set; }

        /// <summary>The amount to scroll long content when pressing a 'scroll up' or 'scroll down' control.</summary>
        public int ScrollAmount { get; set; }
    }
}