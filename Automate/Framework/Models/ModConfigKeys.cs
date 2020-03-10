using Pathoschild.Stardew.Common.Input;

namespace Pathoschild.Stardew.Automate.Framework.Models
{
    /// <summary>A set of parsed key bindings.</summary>
    internal class ModConfigKeys
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The keys which toggle the automation overlay.</summary>
        public KeyBinding ToggleOverlay { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="toggleOverlay">The keys which toggle the automation overlay.</param>
        public ModConfigKeys(KeyBinding toggleOverlay)
        {
            this.ToggleOverlay = toggleOverlay;
        }
    }
}
