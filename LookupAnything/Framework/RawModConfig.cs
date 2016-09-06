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
                Toggle = Keys.F1.ToString(),
                ScrollUp = Keys.Up.ToString(),
                ScrollDown = Keys.Down.ToString()
            };
            this.Controller = new InputMapConfiguration<string>
            {
                Toggle = "",
                ScrollUp = "",
                ScrollDown = ""
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
                    Toggle = Enum.TryParse(this.Keyboard.Toggle, out parsedKey) ? parsedKey : Keys.F1,
                    ScrollUp = Enum.TryParse(this.Keyboard.ScrollUp, out parsedKey) ? parsedKey : Keys.Up,
                    ScrollDown = Enum.TryParse(this.Keyboard.ScrollDown, out parsedKey) ? parsedKey : Keys.Down
                },
                Controller = new InputMapConfiguration<Buttons?>
                {
                    Toggle = Enum.TryParse(this.Controller.Toggle, out parsedButton) ? parsedButton : null as Buttons?,
                    ScrollUp = Enum.TryParse(this.Controller.ScrollUp, out parsedButton) ? parsedButton : null as Buttons?,
                    ScrollDown = Enum.TryParse(this.Controller.ScrollDown, out parsedButton) ? parsedButton : null as Buttons?
                }
            };
        }
    }
}
