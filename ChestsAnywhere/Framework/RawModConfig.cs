using System;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;

namespace ChestsAnywhere.Framework
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

        /// <summary>Whether to check for updates to the mod.</summary>
        public bool CheckForUpdates { get; set; }

        /// <summary>Whether to show the chest name in a tooltip when you point at a chest.</summary>
        public bool ShowHoverTooltips { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct a default instance.</summary>
        public RawModConfig()
        {
            this.Keyboard = new InputMapConfiguration<string>
            {
                Toggle = Keys.B.ToString(),
                PrevChest = Keys.Left.ToString(),
                NextChest = Keys.Right.ToString(),
                PrevCategory = Keys.Down.ToString(),
                NextCategory = Keys.Up.ToString(),
                SortItems = ""
            };
            this.Controller = new InputMapConfiguration<string>
            {
                Toggle = Buttons.Back.ToString(),
                PrevChest = Buttons.LeftShoulder.ToString(),
                NextChest = Buttons.RightShoulder.ToString(),
                PrevCategory = Buttons.LeftTrigger.ToString(),
                NextCategory = Buttons.RightTrigger.ToString(),
                SortItems = ""
            };
            this.CheckForUpdates = true;
            this.ShowHoverTooltips = true;
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
                    Toggle = this.TryParse(this.Keyboard.Toggle, Keys.B),
                    PrevChest = this.TryParse(this.Keyboard.PrevChest, Keys.Left),
                    NextChest = this.TryParse(this.Keyboard.NextChest, Keys.Right),
                    PrevCategory = this.TryParse(this.Keyboard.PrevCategory, Keys.Down),
                    NextCategory = this.TryParse(this.Keyboard.NextCategory, Keys.Up),
                    SortItems = this.TryParse<Keys>(this.Keyboard.SortItems)
                },
                Controller = new InputMapConfiguration<Buttons>
                {
                    Toggle = this.TryParse(this.Controller.Toggle, Buttons.Back),
                    PrevChest = this.TryParse(this.Controller.PrevChest, Buttons.LeftShoulder),
                    NextChest = this.TryParse(this.Controller.NextChest, Buttons.RightShoulder),
                    PrevCategory = this.TryParse(this.Controller.PrevCategory, Buttons.LeftTrigger),
                    NextCategory = this.TryParse(this.Controller.NextCategory, Buttons.RightTrigger),
                    SortItems = this.TryParse<Buttons>(this.Keyboard.SortItems)
                },
                CheckForUpdates = this.CheckForUpdates,
                ShowHoverTooltips = this.ShowHoverTooltips
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
