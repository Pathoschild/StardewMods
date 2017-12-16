using System;
using System.Linq;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.DataMaps.DataMaps;
using Pathoschild.Stardew.DataMaps.Framework;
using Pathoschild.Stardew.DataMaps.Framework.Integrations;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

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

        /// <summary>Handles the logic for integrating with the Pelican Fiber mod.</summary>
        private PelicanFiberIntegration PelicanFiber;

        /// <summary>Handles the logic for integrating with the Better Sprinklers mod.</summary>
        private BetterSprinklersIntegrations BetterSprinklers;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
        {
            // initialise
            this.Config = helper.ReadConfig<ModConfig>();
            this.PelicanFiber = new PelicanFiberIntegration(helper.ModRegistry, helper.Reflection);
            this.BetterSprinklers = new BetterSprinklersIntegrations(helper.ModRegistry, helper.Reflection, this.Monitor);
            this.Maps = new IDataMap[]
            {
                new AccessibilityMap(helper.Translation),
                new ScarecrowMap(helper.Translation),
                new SprinklerMap(helper.Translation, this.BetterSprinklers),
                new JunimoHutMap(helper.Translation, this.PelicanFiber)
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
                if (!this.CanOverlayNow())
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
                        this.CurrentOverlay = new DataMapOverlay(this.Maps, this.CanOverlayNow);
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

        /// <summary>Whether overlays are allowed in the current game context.</summary>
        private bool CanOverlayNow()
        {
            if (!Context.IsWorldReady)
                return false;

            return
                Context.IsPlayerFree // player is free to roam
                || (Game1.activeClickableMenu is CarpenterMenu && this.Helper.Reflection.GetPrivateValue<bool>(Game1.activeClickableMenu, "onFarm")) // on Robin's or Wizard's build screen
                || (this.PelicanFiber.IsBuildMenuOpen() && this.Helper.Reflection.GetPrivateValue<bool>(Game1.activeClickableMenu, "OnFarm")); // on Pelican Fiber's build screen
        }
    }
}
