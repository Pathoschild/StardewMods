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

        /// <summary>Whether the overlay is currently visible.</summary>
        private bool IsOverlayVisible => Context.IsPlayerFree && this.CurrentOverlay != null;

        /// <summary>The available data maps.</summary>
        private readonly IDataMap[] Maps = {
            new TraversableMap(),
            new ScarecrowMap(),
            new SprinklerMap(),
            new JunimoHutMap()
        };


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
                var controls = this.Config.Controls;

                // toggle overlay
                if (Context.IsPlayerFree && controls.ToggleMap.Contains(e.Button))
                {
                    if (this.IsOverlayVisible)
                    {
                        this.CurrentOverlay.Dispose();
                        this.CurrentOverlay = null;
                    }
                    else
                        this.CurrentOverlay = new DataMapOverlay(this.Maps);
                }

                // cycle data maps
                else if (this.IsOverlayVisible && controls.NextMap.Contains(e.Button))
                    this.CurrentOverlay.NextMap();
                else if (this.IsOverlayVisible && controls.PrevMap.Contains(e.Button))
                    this.CurrentOverlay.PrevMap();
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
