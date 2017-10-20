using System;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.DataMaps.Framework;
using Pathoschild.Stardew.DataMaps.Overlays;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace Pathoschild.Stardew.DataMaps
{
    /// <summary>The mod entry point.</summary>
    public class DataMapsMod : Mod
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
            this.Config = helper.ReadConfig<RawModConfig>().GetParsed();

            // hook up events
            GameEvents.SecondUpdateTick += this.GameEvents_SecondUpdateTick;
            if (this.Config.Keyboard.HasAny())
                ControlEvents.KeyPressed += this.ControlEvents_KeyPressed;
            if (this.Config.Controller.HasAny())
            {
                ControlEvents.ControllerButtonPressed += this.ControlEvents_ControllerButtonPressed;
                ControlEvents.ControllerTriggerPressed += this.ControlEvents_ControllerTriggerPressed;
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method invoked when the player presses a keyboard button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void ControlEvents_KeyPressed(object sender, EventArgsKeyPressed e)
        {
            this.ReceiveKeyPress(e.KeyPressed, this.Config.Keyboard);
        }

        /// <summary>The method invoked when the player presses a controller button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void ControlEvents_ControllerButtonPressed(object sender, EventArgsControllerButtonPressed e)
        {
            this.ReceiveKeyPress(e.ButtonPressed, this.Config.Controller);
        }

        /// <summary>The method invoked when the player presses a controller trigger button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void ControlEvents_ControllerTriggerPressed(object sender, EventArgsControllerTriggerPressed e)
        {
            this.ReceiveKeyPress(e.ButtonPressed, this.Config.Controller);
        }

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
                if (key.Equals(map.ToggleMap))
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
