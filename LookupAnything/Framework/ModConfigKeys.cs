using Pathoschild.Stardew.Common.Input;

namespace Pathoschild.Stardew.LookupAnything.Framework
{
    /// <summary>A set of parsed key bindings.</summary>
    internal class ModConfigKeys
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The keys which toggle the lookup UI for something under the cursor.</summary>
        public KeyBinding ToggleLookup { get; }

        /// <summary>The keys which toggle the search UI.</summary>
        public KeyBinding ToggleSearch { get; set; }

        /// <summary>The keys which scroll up long content.</summary>
        public KeyBinding ScrollUp { get; }

        /// <summary>The keys which scroll down long content.</summary>
        public KeyBinding ScrollDown { get; }

        /// <summary>The keys which toggle the display of debug information.</summary>
        public KeyBinding ToggleDebug { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="toggleLookup">The keys which toggle the lookup UI for something under the cursor.</param>
        /// <param name="toggleSearch">The keys which toggle the search UI.</param>
        /// <param name="scrollUp">The keys which scroll up long content.</param>
        /// <param name="scrollDown">The keys which scroll down long content.</param>
        /// <param name="toggleDebug">The keys which toggle the display of debug information.</param>
        public ModConfigKeys(KeyBinding toggleLookup, KeyBinding toggleSearch, KeyBinding scrollUp, KeyBinding scrollDown, KeyBinding toggleDebug)
        {
            this.ToggleLookup = toggleLookup;
            this.ToggleSearch = toggleSearch;
            this.ScrollUp = scrollUp;
            this.ScrollDown = scrollDown;
            this.ToggleDebug = toggleDebug;
        }
    }
}
