using Microsoft.Xna.Framework.Input;

namespace Pathoschild.Stardew.Automate.Framework.Models
{
    /// <summary>The raw mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Write more trace information to the log.</summary>
        public bool VerboseLogging { get; set; } = false;

        /// <summary>The number of ticks between each automation process (60 = once per second).</summary>
        public int AutomationInterval { get; set; } = 60;

        /// <summary>The button which toggles the automation overlay.</summary>
        public Keys ToggleOverlayKey { get; set; } = Keys.U;
    }
}
