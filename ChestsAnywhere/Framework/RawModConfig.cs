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
                SortItems = ""
            };
            this.Controller = new InputMapConfiguration<string>
            {
                Toggle = "",
                PrevChest = "",
                NextChest = "",
                SortItems = ""
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
                Keyboard = new InputMapConfiguration<Keys>
                {
                    Toggle = Enum.TryParse(this.Keyboard.Toggle, out parsedKey) ? parsedKey : Keys.B,
                    PrevChest = Enum.TryParse(this.Keyboard.PrevChest, out parsedKey) ? parsedKey : Keys.Left,
                    NextChest = Enum.TryParse(this.Keyboard.NextChest, out parsedKey) ? parsedKey : Keys.Right,
                    SortItems = Enum.TryParse(this.Keyboard.SortItems, out parsedKey) ? parsedKey : Keys.None
                },
                Controller = new InputMapConfiguration<Buttons?>
                {
                    Toggle = Enum.TryParse(this.Controller.Toggle, out parsedButton) ? parsedButton : null as Buttons?,
                    PrevChest = Enum.TryParse(this.Controller.PrevChest, out parsedButton) ? parsedButton : null as Buttons?,
                    NextChest = Enum.TryParse(this.Controller.NextChest, out parsedButton) ? parsedButton : null as Buttons?,
                    SortItems = Enum.TryParse(this.Controller.SortItems, out parsedButton) ? parsedButton : null as Buttons?,
                }
            };
        }
    }
}
