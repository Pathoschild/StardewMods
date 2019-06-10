using StardewModdingAPI;

namespace Pathoschild.Stardew.Automate.Framework.Models
{
    /// <summary>A set of parsed key bindings.</summary>
    internal class ModConfigKeys
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The key which toggles the automation overlay.</summary>
        public SButton[] ToggleOverlay { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="toggleOverlay">The key which toggles the automation overlay.</param>
        public ModConfigKeys(SButton[] toggleOverlay)
        {
            this.ToggleOverlay = toggleOverlay;
        }
    }
}
