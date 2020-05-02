namespace Pathoschild.Stardew.SmallBeachFarm.Framework
{
    /// <summary>The mod configuration.</summary>
    internal class ModConfig
    {
        /// <summary>Whether to enable the campfire.</summary>
        public bool AddCampfire { get; set; } = true;

        /// <summary>Whether to use a map with islands for added space.</summary>
        public bool EnableIslands { get; set; } = false;

        /// <summary>Use the beach's background music (i.e. wave sounds) on the beach farm.</summary>
        public bool UseBeachMusic { get; set; } = false;
    }
}
