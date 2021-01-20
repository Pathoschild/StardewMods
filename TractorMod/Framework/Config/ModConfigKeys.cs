using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace Pathoschild.Stardew.TractorMod.Framework.Config
{
    /// <summary>A set of parsed key bindings.</summary>
    internal class ModConfigKeys
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The keys which summon the tractor.</summary>
        public KeybindList SummonTractor { get; set; } = KeybindList.ForSingle(SButton.Back);

        /// <summary>The keys which return the tractor to its home.</summary>
        public KeybindList DismissTractor { get; set; } = KeybindList.ForSingle(SButton.Back);

        /// <summary>The keys which activate the tractor when held, or none to activate automatically.</summary>
        public KeybindList HoldToActivate { get; set; } = new();
    }
}
