namespace Pathoschild.Stardew.CropsAnytimeAnywhere.Framework
{
    /// <summary>The mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The seasons for which to enable all crops.</summary>
        public ModSeasonsConfig Seasons { get; set; } = new ModSeasonsConfig();

        /// <summary>Whether to enable crops in non-farm locations (as long as they have tillable dirt).</summary>
        public bool AllowCropsAnywhere { get; set; } = true;
    }
}
