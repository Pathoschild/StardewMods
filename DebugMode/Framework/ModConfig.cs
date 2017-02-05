using Microsoft.Xna.Framework.Input;

namespace Pathoschild.Stardew.DebugMode.Framework
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

        /// <summary>Allow debug commands which are destructive. A command is considered destructive if it immediately ends the current day, randomises the player or farmhouse decorations, or crashes the game.</summary>
        public bool AllowDangerousCommands { get; set; }
    }
}
