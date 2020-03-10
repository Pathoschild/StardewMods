using Pathoschild.Stardew.Common.Input;

namespace Pathoschild.Stardew.DebugMode.Framework
{
    /// <summary>A set of parsed key bindings.</summary>
    internal class ModConfigKeys
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The keys which toggle debug mode.</summary>
        public KeyBinding ToggleDebug { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="toggleDebug">The keys which toggle debug mode.</param>
        public ModConfigKeys(KeyBinding toggleDebug)
        {
            this.ToggleDebug = toggleDebug;
        }
    }
}
