using StardewValley;

namespace Pathoschild.Stardew.SmallBeachFarm.Framework.Config
{
    /// <summary>The mod configuration.</summary>
    internal class ModConfig
    {
        /// <summary>Whether to add a functional campfire in front of the farmhouse.</summary>
        public bool AddCampfire { get; set; } = true;

        /// <summary>Whether to add ocean islands with extra land area.</summary>
        public bool EnableIslands { get; set; } = false;

        /// <summary>Use the beach's background music (i.e. wave sounds) on the beach farm.</summary>
        public bool UseBeachMusic { get; set; } = false;

        /// <summary>The farm layout ID to replace.</summary>
        public int ReplaceFarmID { get; set; } = Farm.riverlands_layout;
    }
}
