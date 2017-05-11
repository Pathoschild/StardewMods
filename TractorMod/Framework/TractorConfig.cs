using Microsoft.Xna.Framework.Input;

namespace TractorMod.Framework
{
    /// <summary>The mod configuration model.</summary>
    internal class TractorConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether the tractor can harvest crops, fruit trees, or forage when the scythe is selected.</summary>
        public bool CanHarvest = true;

        /// <summary>Whether the tractor can hoe dirt tiles when the hoe is selected.</summary>
        public bool CanHoeDirt = true;

        /// <summary>Whether the tractor can water tiles when the watering can is selected.</summary>
        public bool CanWater = true;

        /// <summary>The number of tiles on each side of the tractor to affect (in addition to the tile under it).</summary>
        public int Distance { get; set; } = 1;

        /// <summary>The button which summons the tractor to your position.</summary>
        public Keys TractorKey { get; set; } = Keys.B;

        /// <summary>The speed modifier when riding the tractor.</summary>
        public int TractorSpeed { get; set; } = -2;

        /// <summary>The gold price to buy a tractor garage.</summary>
        public int TractorHousePrice { get; set; } = 150000;

        /// <summary>The custom tool names to allow. These must match the exact in-game tool name.</summary>
        public string[] CustomTools { get; set; } = new string[0];
    }
}
