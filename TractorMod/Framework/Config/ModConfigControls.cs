using Newtonsoft.Json;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;

namespace Pathoschild.Stardew.TractorMod.Framework.Config
{
    /// <summary>A set of control bindings.</summary>
    internal class ModConfigControls
    {
        /// <summary>The control which toggles the chest UI.</summary>
        [JsonConverter(typeof(StringEnumArrayConverter))]
        public SButton[] SummonTractor { get; set; } = { SButton.T };

        /// <summary>A button which activates the tractor when held, or none to activate automatically.</summary>
        [JsonConverter(typeof(StringEnumArrayConverter))]
        public SButton[] HoldToActivate { get; set; } = new SButton[0];
    }
}
