using Newtonsoft.Json;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;

namespace Pathoschild.Stardew.NoclipMode.Framework
{
    /// <summary>The mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The button which toggles noclip mode.</summary>
        [JsonConverter(typeof(StringEnumArrayConverter))]
        public SButton[] ToggleKey { get; set; } = { SButton.F11 };
    }
}
