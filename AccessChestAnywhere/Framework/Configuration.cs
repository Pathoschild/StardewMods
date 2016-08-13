using System;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;

namespace AccessChestAnywhere.Framework
{
    /// <summary>The mod configuration.</summary>
    internal class Configuration : Config
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The keyboard input map.</summary>
        public InputMapConfiguration<string> Keyboard { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct a default instance.</summary>
        public Configuration()
        {
            this.Keyboard = new InputMapConfiguration<string>
            {
                Toggle = Keys.B.ToString(),
                PrevChest = Keys.Left.ToString(),
                NextChest = Keys.Right.ToString()
            };
        }

        /// <summary>Get the keyboard mapping.</summary>
        public InputMapConfiguration<Keys> GetKeys()
        {
            Keys parsed;
            return new InputMapConfiguration<Keys>
            {
                Toggle = Enum.TryParse(this.Keyboard.Toggle, out parsed) ? parsed : Keys.B,
                PrevChest = Enum.TryParse(this.Keyboard.PrevChest, out parsed) ? parsed : Keys.Left,
                NextChest = Enum.TryParse(this.Keyboard.NextChest, out parsed) ? parsed : Keys.Right
            };
        }
    }
}
