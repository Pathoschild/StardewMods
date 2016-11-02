using System;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;

namespace Pathoschild.Stardew.DebugMode.Framework
{
    /// <summary>The raw mod configuration.</summary>
    internal class RawModConfig : Config
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The keyboard input map.</summary>
        public InputMapConfiguration<string> Keyboard { get; set; }

        /// <summary>The controller input map.</summary>
        public InputMapConfiguration<string> Controller { get; set; }


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

        /// <summary>Construct the default configuration.</summary>
        /// <typeparam name="T">The expected configuration type.</typeparam>
        public override T GenerateDefaultConfig<T>()
        {
            return new RawModConfig() as T;
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
                }
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
            T parsed;
            return Enum.TryParse(raw, out parsed) ? parsed : defaultValue;
        }
    }
}
