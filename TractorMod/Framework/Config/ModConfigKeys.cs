using Pathoschild.Stardew.Common.Input;

namespace Pathoschild.Stardew.TractorMod.Framework.Config
{
    /// <summary>A set of parsed key bindings.</summary>
    internal class ModConfigKeys
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The keys which summon the tractor.</summary>
        public KeyBinding SummonTractor { get; }

        /// <summary>The keys which return the tractor to its home.</summary>
        public KeyBinding DismissTractor { get; }

        /// <summary>The keys which activate the tractor when held, or none to activate automatically.</summary>
        public KeyBinding HoldToActivate { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="summonTractor">The keys which summon the tractor.</param>
        /// <param name="dismissTractor">The keys which return the tractor to its home.</param>
        /// <param name="holdToActivate">The keys which activate the tractor when held, or none to activate automatically.</param>
        public ModConfigKeys(KeyBinding summonTractor, KeyBinding dismissTractor, KeyBinding holdToActivate)
        {
            this.SummonTractor = summonTractor;
            this.DismissTractor = dismissTractor;
            this.HoldToActivate = holdToActivate;
        }
    }
}
