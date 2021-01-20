using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace Pathoschild.Stardew.Automate.Framework.Models
{
    /// <summary>A set of parsed key bindings.</summary>
    internal class ModConfigKeys
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The keys which toggle the automation overlay.</summary>
        public KeybindList ToggleOverlay { get; set; } = KeybindList.ForSingle(SButton.U);
    }
}
