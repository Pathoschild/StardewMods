using System;
using Microsoft.Xna.Framework.Input;

namespace Pathoschild.Stardew.DebugMode.Framework
{
    /// <summary>The raw mod configuration.</summary>
    internal class RawModConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The keyboard input map.</summary>
        public InputMapConfiguration<string> Keyboard { get; set; }

        /// <summary>The controller input map.</summary>
        public InputMapConfiguration<string> Controller { get; set; }

        /// <summary>Allow debug commands which are destructive. A command is considered destructive if it immediately ends the current day, randomises the player or farmhouse decorations, or crashes the game.</summary>
        public bool AllowDangerousCommands { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct a default instance.</summary>
        public RawModConfig()
        {
            this.Keyboard = new InputMapConfiguration<string>
            {
                ToggleDebug = Keys.OemTilde.ToString()
            };
            this.Controller = new InputMapConfiguration<string>
            {
                ToggleDebug = ""
            };
        }

        /// <summary>Get a parsed representation of the mod configuration.</summary>
        public ModConfig GetParsed()
        {
            return new ModConfig
            {
                Keyboard = new InputMapConfiguration<Keys>
                {
                    ToggleDebug = this.TryParse(this.Keyboard.ToggleDebug, Keys.OemTilde)
                },
                Controller = new InputMapConfiguration<Buttons>
                {
                    ToggleDebug = this.TryParse<Buttons>(this.Controller.ToggleDebug)
                },
                AllowDangerousCommands = this.AllowDangerousCommands
            };
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Parse a raw enum value.</summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="raw">The raw value.</param>
        /// <param name="defaultValue">The default value if it can't be parsed.</param>
        private T TryParse<T>(string raw, T defaultValue = default(T)) where T : struct
        {
            return Enum.TryParse(raw, out T parsed) ? parsed : defaultValue;
        }
    }
}
