using System.Linq;
using Pathoschild.Stardew.ChestsAnywhere.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace Pathoschild.Stardew.ChestsAnywhere
{
    /// <summary>
    /// Encapsulates state and logic for tracking the registered <see cref="ModConfig.ModConfigControls.CategoryScrollModifier"/>
    /// </summary>
    /// <remarks>This is a shim that can be removed when SMAPI Input support for IsDown is available.</remarks>
    internal class ScrollModifierController
    {
        /// <summary> The modifier buttons to watch </summary>
        private readonly SButton[] Modifiers;

        /// <summary> If true scrolling should navigate through categories </summary>
        public bool ScrollCategory { get; private set; }

        /// <summary>
        /// Construct an instance
        /// </summary>
        /// <param name="config">The configuration.</param>
        public ScrollModifierController(ModConfig config)
        {
            if (config.EnableScrollNavigation)
            {
                InputEvents.ButtonPressed += this.Handle_ButtonPressed;
                InputEvents.ButtonReleased += this.Handle_ButtonReleased;
                this.Modifiers = config.Controls.CategoryScrollModifier;
            }
        }

        /// <summary>The method invoked when the player presses a button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void Handle_ButtonPressed(object sender, EventArgsInput e)
        {
            if (this.Modifiers.Contains(e.Button))
                this.ScrollCategory = true;
        }

        /// <summary>The method invoked when the player releases a button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void Handle_ButtonReleased(object sender, EventArgsInput e)
        {
            if (this.Modifiers.Contains(e.Button))
                this.ScrollCategory = false;
        }
    }
}
