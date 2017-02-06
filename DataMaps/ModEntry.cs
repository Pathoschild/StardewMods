using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.DataMaps.Framework;
using Pathoschild.Stardew.DataMaps.Overlays;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace Pathoschild.Stardew.DataMaps
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
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
            GameEvents.SecondUpdateTick += (sender, e) => this.ReceiveUpdateTick();
            if (this.Config.Keyboard.HasAny())
                ControlEvents.KeyPressed += (sender, e) => this.ReceiveKeyPress(e.KeyPressed, this.Config.Keyboard);
            if (this.Config.Controller.HasAny())
            {
                ControlEvents.ControllerButtonPressed += (sender, e) => this.ReceiveKeyPress(e.ButtonPressed, this.Config.Controller);
                ControlEvents.ControllerTriggerPressed += (sender, e) => this.ReceiveKeyPress(e.ButtonPressed, this.Config.Controller);
            }
        }


        /*********
        ** Private methods
        *********/
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
        private void ReceiveUpdateTick()
        {
            this.CurrentOverlay?.Update();
        }
    }
}
