using System;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;

namespace Pathoschild.LookupAnything.Framework
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

        /// <summary>The amount to scroll long content when pressing a 'scroll up' or 'scroll down' control.</summary>
        public int ScrollAmount { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct a default instance.</summary>
        public RawModConfig()
        {
            this.ScrollAmount = 160;
            this.Keyboard = new InputMapConfiguration<string>
            {
                Lookup = Keys.F1.ToString(),
                ScrollUp = Keys.Up.ToString(),
                ScrollDown = Keys.Down.ToString(),
                ToggleDebugInfo = ""
            };
            this.Controller = new InputMapConfiguration<string>
            {
                Lookup = "",
                ScrollUp = "",
                ScrollDown = "",
                ToggleDebugInfo = ""
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
            Keys parsedKey;
            Buttons parsedButton;
            return new ModConfig
            {
                ScrollAmount = this.ScrollAmount,
                Keyboard = new InputMapConfiguration<Keys>
                {
                    Lookup = Enum.TryParse(this.Keyboard.Lookup, out parsedKey) ? parsedKey : Keys.F1,
                    ScrollUp = Enum.TryParse(this.Keyboard.ScrollUp, out parsedKey) ? parsedKey : Keys.Up,
                    ScrollDown = Enum.TryParse(this.Keyboard.ScrollDown, out parsedKey) ? parsedKey : Keys.Down,
                    ToggleDebugInfo = Enum.TryParse(this.Keyboard.ToggleDebugInfo, out parsedKey) ? parsedKey : Keys.None
                },
                Controller = new InputMapConfiguration<Buttons?>
                {
                    Lookup = Enum.TryParse(this.Controller.Lookup, out parsedButton) ? parsedButton : null as Buttons?,
                    ScrollUp = Enum.TryParse(this.Controller.ScrollUp, out parsedButton) ? parsedButton : null as Buttons?,
                    ScrollDown = Enum.TryParse(this.Controller.ScrollDown, out parsedButton) ? parsedButton : null as Buttons?
                }
            };
        }
    }
}
