using Microsoft.Xna.Framework.Input;

namespace TractorMod.Framework
{
    /// <summary>The mod configuration model.</summary>
    internal class TractorConfig
    {
        /*********
        ** Accessors
        *********/
        public string Info1 = "Add tool with exact name you would like to use with Tractor Mode.";
        public string Info2 = "Also custom minLevel and effective radius for each tool.";
        public string Info3 = "Ingame tools included: Pickaxe, Axe, Hoe, Watering Can.";
        public string Info4 = "I haven't tried tools like Shears or Milk Pail but you can :)";
        public string Info5 = "Delete Scythe entry if you don't want to harvest stuff.";

        /// <summary>The enabled tools.</summary>
        public ToolConfig[] Tool { get; set; } = {
            new ToolConfig("Scythe", 0, 2, 1),
            new ToolConfig("Hoe"),
            new ToolConfig("Watering Can")
        };

        /// <summary>The number of tiles on each side of the tractor to affect (in addition to the tile under it).</summary>
        public int ItemRadius { get; set; } = 1;

        /// <summary>The button to hold to activate the tractor tool. The possible values are <c>0</c> (always active), <c>1</c> (left mouse button), <c>2</c> (right mouse button), or <c>3</c> (right mouse button).</summary>
        public int HoldActivate { get; set; } = 0;

        /// <summary>The button which summons the tractor to your position.</summary>
        public Keys TractorKey { get; set; } = Keys.B;

        /// <summary>The speed modifier when riding the tractor.</summary>
        public int TractorSpeed { get; set; } = -2;

        /// <summary>The button which summons the horse to your position.</summary>
        public Keys HorseKey { get; set; } = Keys.None;

        /// <summary>The button which calls to buy a tractor garage.</summary>
        public Keys PhoneKey { get; set; } = Keys.N;

        /// <summary>The gold price to buy a tractor garage.</summary>
        public int TractorHousePrice { get; set; } = 150000;

        /// <summary>The button which reloads the mod configuration.</summary>
        public Keys UpdateConfig { get; set; } = Keys.P;
    }
}
