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
        /// <summary>The human-readable key which toggles the chest UI.</summary>
        public string ToggleKey = Keys.B.ToString();


        /*********
        ** Public methods
        *********/
        /// <summary>Get the key which toggles the chest UI.</summary>
        public Keys GetToggleKey()
        {
            Keys parsed;
            return Enum.TryParse(this.ToggleKey, out parsed)
                ? parsed
                : Keys.B;
        }
    }
}
