using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.RotateToolbar.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace Pathoschild.Stardew.RotateToolbar
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod
    {
        /*********
        ** Fields
        *********/
        /// <summary>The mod configuration.</summary>
        private ModConfig Config;

        /// <summary>The configured key bindings.</summary>
        private ModConfigKeys Keys => this.Config.Controls;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
        {
            // read config
            this.Config = helper.ReadConfig<ModConfig>();

            // hook events
            helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Event handlers
        ****/
        /// <summary>Raised after the player presses any buttons on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            // perform bound action
            this.Monitor.InterceptErrors("handling your input", () =>
            {
                ModConfigKeys keys = this.Keys;
                if (keys.ShiftToNext.JustPressed())
                    this.RotateToolbar(true, this.Config.DeselectItemOnRotate);
                else if (keys.ShiftToPrevious.JustPressed())
                    this.RotateToolbar(false, this.Config.DeselectItemOnRotate);
            });
        }

        /// <summary>Rotate the row shown in the toolbar.</summary>
        /// <param name="next">Whether to show the next inventory row (else the previous).</param>
        /// <param name="deselectSlot">Whether to deselect the current slot.</param>
        private void RotateToolbar(bool next, bool deselectSlot)
        {
            Game1.player.shiftToolbar(next);
            if (deselectSlot)
                Game1.player.CurrentToolIndex = int.MaxValue; // Farmer::CurrentItem/Tool ignore the index if it's higher than the inventory size
        }
    }
}
