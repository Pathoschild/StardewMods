using System;
using Microsoft.Xna.Framework.Input;

namespace Pathoschild.Stardew.DataMaps.Framework
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


        /*********
        ** Public methods
        *********/
        /// <summary>Construct a default instance.</summary>
        public RawModConfig()
        {
            this.Keyboard = new InputMapConfiguration<string>
            {
                ToggleMap = Keys.F2.ToString()
            };
            this.Controller = new InputMapConfiguration<string>
            {
                ToggleMap = ""
            };
        }

        /// <summary>Get a parsed representation of the mod configuration.</summary>
        public ModConfig GetParsed()
        {
            return new ModConfig
            {
                Keyboard = new InputMapConfiguration<Keys>
                {
                    ToggleMap = this.TryParse(this.Keyboard.ToggleMap, Keys.F2)
                },
                Controller = new InputMapConfiguration<Buttons>
                {
                    ToggleMap = this.TryParse<Buttons>(this.Controller.ToggleMap)
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
