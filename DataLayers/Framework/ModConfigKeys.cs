using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace Pathoschild.Stardew.DataLayers.Framework
{
    /// <summary>A set of raw key bindings.</summary>
    internal class ModConfigKeys
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The keys which toggle the data layer overlay.</summary>
        public KeybindList ToggleLayer { get; set; } = KeybindList.ForSingle(SButton.F2);

        /// <summary>The keys which cycle backwards through data layers.</summary>
        public KeybindList PrevLayer { get; set; } = KeybindList.Parse($"{SButton.LeftControl}, {SButton.LeftShoulder}");

        /// <summary>The keys which cycle forward through data layers.</summary>
        public KeybindList NextLayer { get; set; } = KeybindList.Parse($"{SButton.RightControl}, {SButton.RightShoulder}");
    }
}
