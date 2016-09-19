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

        /// <summary>Whether to check for updates to the mod.</summary>
        public bool CheckForUpdates { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct a default instance.</summary>
        public RawModConfig()
        {
            this.ScrollAmount = 160;
            this.Keyboard = new InputMapConfiguration<string>
            {
                ToggleLookup = Keys.F1.ToString(),
                ScrollUp = Keys.Up.ToString(),
                ScrollDown = Keys.Down.ToString(),
                ToggleDebug = ""
            };
            this.Controller = new InputMapConfiguration<string>
            {
                ToggleLookup = "",
                ScrollUp = "",
                ScrollDown = "",
                ToggleDebug = ""
            };
            this.CheckForUpdates = true;
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
                    ToggleLookup = Enum.TryParse(this.Keyboard.ToggleLookup, out parsedKey) ? parsedKey : Keys.F1,
                    ScrollUp = Enum.TryParse(this.Keyboard.ScrollUp, out parsedKey) ? parsedKey : Keys.Up,
                    ScrollDown = Enum.TryParse(this.Keyboard.ScrollDown, out parsedKey) ? parsedKey : Keys.Down,
                    ToggleDebug = Enum.TryParse(this.Keyboard.ToggleDebug, out parsedKey) ? parsedKey : Keys.None
                },
                Controller = new InputMapConfiguration<Buttons?>
                {
                    ToggleLookup = Enum.TryParse(this.Controller.ToggleLookup, out parsedButton) ? parsedButton : null as Buttons?,
                    ScrollUp = Enum.TryParse(this.Controller.ScrollUp, out parsedButton) ? parsedButton : null as Buttons?,
                    ScrollDown = Enum.TryParse(this.Controller.ScrollDown, out parsedButton) ? parsedButton : null as Buttons?
                },
                CheckForUpdates = this.CheckForUpdates
            };
        }
    }
}
