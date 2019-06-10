using StardewModdingAPI;

namespace Pathoschild.Stardew.TractorMod.Framework.Config
{
    /// <summary>A set of parsed key bindings.</summary>
    internal class ModConfigKeys
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The key which summons the tractor.</summary>
        public SButton[] SummonTractor { get; }

        /// <summary>The key which returns the tractor to its home.</summary>
        public SButton[] DismissTractor { get; }

        /// <summary>A key which activates the tractor when held, or none to activate automatically.</summary>
        public SButton[] HoldToActivate { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="summonTractor">The key which summons the tractor.</param>
        /// <param name="dismissTractor">The key which returns the tractor to its home.</param>
        /// <param name="holdToActivate">A key which activates the tractor when held, or none to activate automatically.</param>
        public ModConfigKeys(SButton[] summonTractor, SButton[] dismissTractor, SButton[] holdToActivate)
        {
            this.SummonTractor = summonTractor;
            this.DismissTractor = dismissTractor;
            this.HoldToActivate = holdToActivate;
        }
    }
}
