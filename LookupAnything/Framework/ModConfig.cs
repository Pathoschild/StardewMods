using System;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;

namespace Pathoschild.LookupAnything.Framework
{
    /// <summary>The mod configuration.</summary>
    internal class ModConfig : Config
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
        public ModConfig()
        {
            this.Keyboard = new InputMapConfiguration<string>
            {
                Toggle = Keys.F1.ToString()
            };
            this.Controller = new InputMapConfiguration<string>
            {
                Toggle = ""
            };
        }

        /// <summary>Construct the default configuration.</summary>
        /// <typeparam name="T">The expected configuration type.</typeparam>
        public override T GenerateDefaultConfig<T>()
        {
            return new ModConfig() as T;
        }

        /// <summary>Get the keyboard mapping.</summary>
        public InputMapConfiguration<Keys> GetKeyboard()
        {
            Keys parsed;
            return new InputMapConfiguration<Keys>
            {
                Toggle = Enum.TryParse(this.Keyboard.Toggle, out parsed) ? parsed : Keys.F1
            };
        }

        /// <summary>Get the controller mapping.</summary>
        public InputMapConfiguration<Buttons?> GetController()
        {
            Buttons parsed;
            return new InputMapConfiguration<Buttons?>
            {
                Toggle = Enum.TryParse(this.Controller.Toggle, out parsed) ? parsed : null as Buttons?
            };
        }
    }
}
