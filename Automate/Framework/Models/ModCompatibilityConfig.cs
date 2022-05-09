namespace Pathoschild.Stardew.Automate.Framework.Models
{
    /// <summary>Mod compatibility options.</summary>
    internal class ModCompatibilityConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether to enable compatibility with Better Junimos. If it's installed, Junimo huts won't output fertilizer or seeds.</summary>
        public bool BetterJunimos { get; set; } = true;

        /// <summary>Whether to log a warning if the player installs a custom-machine mod that requires a separate compatibility patch which isn't installed.</summary>
        public bool WarnForMissingBridgeMod { get; set; } = true;
    }
}
