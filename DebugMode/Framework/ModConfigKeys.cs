using StardewModdingAPI;

namespace Pathoschild.Stardew.DebugMode.Framework
{
    /// <summary>A set of parsed key bindings.</summary>
    internal class ModConfigKeys
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The key which toggles debug mode.</summary>
        public SButton[] ToggleDebug { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="toggleDebug">The key which toggles debug mode.</param>
        public ModConfigKeys(SButton[] toggleDebug)
        {
            this.ToggleDebug = toggleDebug;
        }
    }
}
