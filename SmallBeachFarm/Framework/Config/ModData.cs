using Microsoft.Xna.Framework;

namespace Pathoschild.Stardew.SmallBeachFarm.Framework.Config
{
    /// <summary>The mod's hardcoded data.</summary>
    internal class ModData
    {
        /// <summary>Tile areas in the farm map where both river and ocean fish can be caught.</summary>
        public Rectangle[] MixedFishAreas { get; set; } = new Rectangle[0];

        /// <summary>The farm maps that can be replaced.</summary>
        public ModFarmMapData[] FarmMaps { get; set; }
    }
}
