using StardewModdingAPI;

namespace Pathoschild.Stardew.LookupAnything.Framework
{
    /// <summary>A set of parsed key bindings.</summary>
    internal class ModConfigKeys
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The key which toggles the lookup UI for something under the cursor.</summary>
        public SButton[] ToggleLookup { get; }

        /// <summary>The key which toggles the lookup UI for something in front of the player.</summary>
        public SButton[] ToggleLookupInFrontOfPlayer { get; }

        /// <summary>The key which scrolls up long content.</summary>
        public SButton[] ScrollUp { get; }

        /// <summary>The key which scrolls down long content.</summary>
        public SButton[] ScrollDown { get; }

        /// <summary>The key which toggles the display of debug information.</summary>
        public SButton[] ToggleDebug { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="toggleLookup">The key which toggles the lookup UI for something under the cursor.</param>
        /// <param name="toggleLookupInFrontOfPlayer">The key which toggles the lookup UI for something in front of the player.</param>
        /// <param name="scrollUp">The key which scrolls up long content.</param>
        /// <param name="scrollDown">The key which scrolls down long content.</param>
        /// <param name="toggleDebug">The key which toggles the display of debug information.</param>
        public ModConfigKeys(SButton[] toggleLookup, SButton[] toggleLookupInFrontOfPlayer, SButton[] scrollUp, SButton[] scrollDown, SButton[] toggleDebug)
        {
            this.ToggleLookup = toggleLookup;
            this.ToggleLookupInFrontOfPlayer = toggleLookupInFrontOfPlayer;
            this.ScrollUp = scrollUp;
            this.ScrollDown = scrollDown;
            this.ToggleDebug = toggleDebug;
        }
    }
}
