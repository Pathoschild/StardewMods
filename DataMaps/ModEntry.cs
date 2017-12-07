using System;
using System.Linq;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.DataMaps.Framework;
using Pathoschild.Stardew.DataMaps.Overlays;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace Pathoschild.Stardew.DataMaps
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod
    {
        /*********
        ** Properties
        *********/
        /// <summary>The mod configuration.</summary>
        private ModConfig Config;

        /// <summary>The current overlay being displayed, if any.</summary>
        private DataMapOverlay CurrentOverlay;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
        {
            // load config
            this.Config = helper.ReadConfig<ModConfig>();

            // hook up events
            GameEvents.SecondUpdateTick += this.GameEvents_SecondUpdateTick;
            InputEvents.ButtonPressed += this.InputEvents_ButtonPressed;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method invoked when the player presses an input button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void InputEvents_ButtonPressed(object sender, EventArgsInput e)
        {
            // perform bound action
            this.Monitor.InterceptErrors("handling your input", $"handling input '{e.Button}'", () =>
            {
                var controls = this.Config.Controls;

                if (controls.ToggleMap.Contains(e.Button))
                {
                    if (this.CurrentOverlay != null)
                    {
                        this.CurrentOverlay.Dispose();
                        this.CurrentOverlay = null;
                    }
                    else
                        this.CurrentOverlay = new TraversableOverlay();

                    this.Monitor.Log($"set overlay: {this.CurrentOverlay?.GetType().Name ?? "none"}", LogLevel.Trace);
                }
            });
        }

        /// <summary>Receive an update tick.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void GameEvents_SecondUpdateTick(object sender, EventArgs e)
        {
            this.CurrentOverlay?.Update();
        }
    }
}
