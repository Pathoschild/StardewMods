using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace Pathoschild.Stardew.DebugMode.Framework
{
    /// <summary>A set of parsed key bindings.</summary>
    internal class ModConfigKeys
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The keys which toggle debug mode.</summary>
        public KeybindList ToggleDebug { get; set; } = KeybindList.ForSingle(SButton.OemTilde);
    }
}
