namespace Pathoschild.Stardew.CropsInAnySeason.Framework
{
    /// <summary>The mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The seasons for which to enable all crops.</summary>
        public ModSeasonsConfig EnableInSeasons { get; set; } = new ModSeasonsConfig();
    }
}
