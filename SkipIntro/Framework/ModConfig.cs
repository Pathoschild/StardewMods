using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Pathoschild.Stardew.SkipIntro.Framework
{
    /// <summary>The mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The screen to which to skip.</summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public Screen SkipTo { get; set; } = Screen.Title;
    }
}
