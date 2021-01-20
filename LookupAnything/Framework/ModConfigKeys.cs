using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace Pathoschild.Stardew.LookupAnything.Framework
{
    /// <summary>A set of parsed key bindings.</summary>
    internal class ModConfigKeys
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The keys which toggle the lookup UI for something under the cursor.</summary>
        public KeybindList ToggleLookup { get; set; } = KeybindList.ForSingle(SButton.F1);

        /// <summary>The keys which toggle the search UI.</summary>
        public KeybindList ToggleSearch { get; set; } = KeybindList.Parse($"{SButton.LeftShift} + {SButton.F1}");

        /// <summary>The keys which scroll up long content.</summary>
        public KeybindList ScrollUp { get; set; } = KeybindList.ForSingle(SButton.Up);

        /// <summary>The keys which scroll down long content.</summary>
        public KeybindList ScrollDown { get; set; } = KeybindList.ForSingle(SButton.Down);

        /// <summary>The keys which toggle the display of debug information.</summary>
        public KeybindList ToggleDebug { get; set; } = new();
    }
}
