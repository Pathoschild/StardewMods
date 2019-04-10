using StardewModdingAPI;

namespace Pathoschild.Stardew.NoclipMode.Framework
{
    /// <summary>The mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The key which toggles noclip mode.</summary>
        public string ToggleKey { get; set; } = SButton.F11.ToString();
    }
}
