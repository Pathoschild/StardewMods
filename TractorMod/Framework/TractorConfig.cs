using Microsoft.Xna.Framework.Input;

namespace TractorMod.Framework
{
    /// <summary>The mod configuration model.</summary>
    internal class TractorConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The enabled tools.</summary>
        public ToolConfig[] Tool { get; set; } = {
            new ToolConfig("Scythe", 0, 2),
            new ToolConfig("Hoe"),
            new ToolConfig("Watering Can")
        };

        /// <summary>The number of tiles on each side of the tractor to affect (in addition to the tile under it).</summary>
        public int ItemRadius { get; set; } = 1;

        /// <summary>The button which summons the tractor to your position.</summary>
        public Keys TractorKey { get; set; } = Keys.B;

        /// <summary>The speed modifier when riding the tractor.</summary>
        public int TractorSpeed { get; set; } = -2;

        /// <summary>The button which calls to buy a tractor garage.</summary>
        public Keys PhoneKey { get; set; } = Keys.N;

        /// <summary>The gold price to buy a tractor garage.</summary>
        public int TractorHousePrice { get; set; } = 150000;

        /// <summary>The button which reloads the mod configuration.</summary>
        public Keys UpdateConfig { get; set; } = Keys.P;
    }
}
