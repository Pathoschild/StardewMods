using System;
using System.Linq;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.DataMaps.DataMaps;
using Pathoschild.Stardew.DataMaps.Framework;
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

        /// <summary>The available data maps.</summary>
        private IDataMap[] Maps;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
        {
            // load config
            this.Config = helper.ReadConfig<ModConfig>();

            // load data maps
            this.Maps = new IDataMap[]
            {
                new AccessibilityMap(helper.Translation),
                new ScarecrowMap(helper.Translation),
                new SprinklerMap(helper.Translation),
                new JunimoHutMap(helper.Translation)
            };

            // hook up events
            SaveEvents.AfterReturnToTitle += this.SaveEvents_AfterReturnToTitle;
            GameEvents.SecondUpdateTick += this.GameEvents_SecondUpdateTick;
            InputEvents.ButtonPressed += this.InputEvents_ButtonPressed;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method invoked when the player returns to the title screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void SaveEvents_AfterReturnToTitle(object sender, EventArgs e)
        {
            this.CurrentOverlay?.Dispose();
            this.CurrentOverlay = null;
        }

        /// <summary>The method invoked when the player presses an input button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void InputEvents_ButtonPressed(object sender, EventArgsInput e)
        {
            // perform bound action
            this.Monitor.InterceptErrors("handling your input", $"handling input '{e.Button}'", () =>
            {
                // check context
                if (!DataMapOverlay.CanOverlayNow())
                    return;
                bool overlayVisible = this.CurrentOverlay != null;
                var controls = this.Config.Controls;

                // toggle overlay
                if (controls.ToggleMap.Contains(e.Button))
                {
                    if (overlayVisible)
                    {
                        this.CurrentOverlay.Dispose();
                        this.CurrentOverlay = null;
                    }
                    else
                        this.CurrentOverlay = new DataMapOverlay(this.Maps);
                    e.SuppressButton();
                }

                // cycle data maps
                else if (overlayVisible && controls.NextMap.Contains(e.Button))
                {
                    this.CurrentOverlay.NextMap();
                    e.SuppressButton();
                }
                else if (overlayVisible && controls.PrevMap.Contains(e.Button))
                {
                    this.CurrentOverlay.PrevMap();
                    e.SuppressButton();
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
