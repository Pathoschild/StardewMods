using System;
using Microsoft.Xna.Framework.Input;

namespace Pathoschild.Stardew.LookupAnything.Framework
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

        /// <summary>The amount to scroll long content when pressing a 'scroll up' or 'scroll down' control.</summary>
        public int ScrollAmount { get; set; }

        /// <summary>Whether the lookup UI should only be visible as long as the key is pressed.</summary>
        public bool HideOnKeyUp { get; set; }

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
                ToggleLookupFront = "",
                ScrollUp = Keys.Up.ToString(),
                ScrollDown = Keys.Down.ToString(),
                ToggleDebug = ""
            };
            this.Controller = new InputMapConfiguration<string>
            {
                ToggleLookup = "",
                ToggleLookupFront = "",
                ScrollUp = "",
                ScrollDown = "",
                ToggleDebug = ""
            };
            this.HideOnKeyUp = false;
            this.CheckForUpdates = true;
        }

        /// <summary>Get a parsed representation of the mod configuration.</summary>
        public ModConfig GetParsed()
        {
            return new ModConfig
            {
                ScrollAmount = this.ScrollAmount,
                Keyboard = new InputMapConfiguration<Keys>
                {
                    ToggleLookup = this.TryParse(this.Keyboard.ToggleLookup, Keys.F1),
                    ToggleLookupFront = this.TryParse(this.Keyboard.ToggleLookupFront, Keys.F1),
                    ScrollUp = this.TryParse(this.Keyboard.ScrollUp, Keys.Up),
                    ScrollDown = this.TryParse(this.Keyboard.ScrollDown, Keys.Down),
                    ToggleDebug = this.TryParse(this.Keyboard.ToggleDebug, Keys.None)
                },
                Controller = new InputMapConfiguration<Buttons>
                {
                    ToggleLookup = this.TryParse<Buttons>(this.Controller.ToggleLookup),
                    ToggleLookupFront = this.TryParse<Buttons>(this.Controller.ToggleLookupFront),
                    ScrollUp = this.TryParse<Buttons>(this.Controller.ScrollUp),
                    ScrollDown = this.TryParse<Buttons>(this.Controller.ScrollDown),
                    ToggleDebug = this.TryParse<Buttons>(this.Controller.ToggleDebug)
                },
                HideOnKeyUp = this.HideOnKeyUp,
                CheckForUpdates = this.CheckForUpdates
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
