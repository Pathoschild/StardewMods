using Microsoft.Xna.Framework;

namespace Pathoschild.Stardew.SmallBeachFarm.Framework.Config
{
    /// <summary>The mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /****
        ** farm options
        ****/
        /// <summary>Use the beach's background music (i.e. wave sounds) on the beach farm.</summary>
        public bool UseBeachMusic { get; set; } = false;

        /// <summary>The default value for the 'spawn monsters at night' option when creating a new save.</summary>
        public bool DefaultSpawnMonstersAtNight { get; set; } = false;

        /****
        ** Farm layout
        ****/
        /// <summary>Whether to add a functional campfire in front of the farmhouse.</summary>
        public bool AddCampfire { get; set; } = true;

        /// <summary>Whether to add a fishing pier.</summary>
        public bool AddFishingPier { get; set; } = false;

        /// <summary>Whether to add ocean islands with extra land area.</summary>
        public bool EnableIslands { get; set; } = false;

        /// <summary>Whether to place a stone path tiles in front of the default shipping bin position.</summary>
        public bool ShippingBinPath { get; set; } = true;

        /****
        ** Farm positions
        ****/
        /// <summary>The custom position at which to add the tier, or <see cref="Point.Zero"/> to position it automatically.</summary>
        public Point CustomFishingPierPosition { get; set; } = Point.Zero;
    }
}
