using Pathoschild.Stardew.Common;
using StardewModdingAPI;

namespace Pathoschild.Stardew.TractorMod.Framework.Config
{
    /// <summary>A set of raw key bindings.</summary>
    internal class ModConfigRawKeys
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The key which summons the tractor.</summary>
        public string SummonTractor { get; set; } = SButton.Back.ToString();

        /// <summary>The key which returns the tractor to its home.</summary>
        public string DismissTractor { get; set; } = SButton.Back.ToString();

        /// <summary>A key which activates the tractor when held, or none to activate automatically.</summary>
        public string HoldToActivate { get; set; } = "";


        /*********
        ** Public fields
        *********/
        /// <summary>Get a parsed representation of the configured controls.</summary>
        /// <param name="monitor">The monitor through which to log an error if a button value is invalid.</param>
        public ModConfigKeys ParseControls(IMonitor monitor)
        {
            return new ModConfigKeys(
                summonTractor: CommonHelper.ParseButtons(this.SummonTractor, monitor, nameof(this.SummonTractor)),
                dismissTractor: CommonHelper.ParseButtons(this.DismissTractor, monitor, nameof(this.DismissTractor)),
                holdToActivate: CommonHelper.ParseButtons(this.HoldToActivate, monitor, nameof(this.HoldToActivate))
            );
        }
    }
}
