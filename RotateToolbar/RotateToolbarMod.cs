using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.RotateToolbar.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace Pathoschild.Stardew.RotateToolbar
{
    /// <summary>The mod entry point.</summary>
    public class RotateToolbarMod : Mod
    {
        /*********
        ** Properties
        *********/
        /// <summary>The mod configuration.</summary>
        private ModConfig Config;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
        {
            this.Config = helper.ReadConfig<RawModConfig>().GetParsed(this.Monitor);

            ControlEvents.KeyPressed += this.ControlEvents_KeyPressed;
            ControlEvents.ControllerButtonPressed += this.ControlEvents_ControllerButtonPressed;
            ControlEvents.ControllerTriggerPressed += this.ControlEvents_ControllerTriggerPressed;
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Event handlers
        ****/
        /// <summary>The method invoked when the player presses a keyboard button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ControlEvents_KeyPressed(object sender, EventArgsKeyPressed e)
        {
            if (!Context.IsWorldReady)
                return;

            this.ReceiveKeyPress(e.KeyPressed, this.Config.Keyboard);
        }

        /// <summary>The method invoked when the player presses a controller button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ControlEvents_ControllerTriggerPressed(object sender, EventArgsControllerTriggerPressed e)
        {
            if (!Context.IsWorldReady)
                return;

            this.ReceiveKeyPress(e.ButtonPressed, this.Config.Controller);
        }

        /// <summary>The method invoked when the player presses a controller trigger button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ControlEvents_ControllerButtonPressed(object sender, EventArgsControllerButtonPressed e)
        {
            if (!Context.IsWorldReady)
                return;

            this.ReceiveKeyPress(e.ButtonPressed, this.Config.Controller);
        }

        /****
        ** Methods
        ****/
        /// <summary>The method invoked when the player presses an input button.</summary>
        /// <typeparam name="TKey">The input type.</typeparam>
        /// <param name="key">The pressed input.</param>
        /// <param name="map">The configured input mapping.</param>
        private void ReceiveKeyPress<TKey>(TKey key, InputMapConfiguration<TKey> map)
        {
            if (!map.IsValidKey(key))
                return;

            // perform bound action
            this.Monitor.InterceptErrors("handling your input", $"handling input '{key}'", () =>
            {
                if (key.Equals(map.ShiftToNext))
                    this.RotateToolbar(true, this.Config.DeselectItemOnRotate);
                else if (key.Equals(map.ShiftToPrevious))
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
