using System;
using System.Linq;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.DataMaps.DataMaps;
using Pathoschild.Stardew.DataMaps.Framework;
using Pathoschild.Stardew.DataMaps.Framework.Integrations.BetterSprinklers;
using Pathoschild.Stardew.DataMaps.Framework.Integrations.Cobalt;
using Pathoschild.Stardew.DataMaps.Framework.Integrations.PelicanFiber;
using Pathoschild.Stardew.DataMaps.Framework.Integrations.SimpleSprinkler;
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

        /// <summary>Handles access to the Pelican Fiber mod.</summary>
        private PelicanFiberIntegration PelicanFiber;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
        {
            // read config
            this.Config = helper.ReadConfig<ModConfig>();

            // hook up events
            SaveEvents.AfterReturnToTitle += this.SaveEvents_AfterReturnToTitle;
            GameEvents.FirstUpdateTick += this.GameEvents_FirstUpdateTick;
            GameEvents.SecondUpdateTick += this.GameEvents_SecondUpdateTick;
            InputEvents.ButtonPressed += this.InputEvents_ButtonPressed;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method invoked on the first game update tick.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void GameEvents_FirstUpdateTick(object sender, EventArgs e)
        {
            IModHelper helper = this.Helper;

            this.PelicanFiber = new PelicanFiberIntegration(helper.ModRegistry, helper.Reflection, this.Monitor);
            var betterSprinklers = new BetterSprinklersIntegration(helper.ModRegistry, this.Monitor);
            var cobalt = new CobaltIntegration(helper.ModRegistry, this.Monitor);
            var simpleSprinklers = new SimpleSprinklerIntegration(helper.ModRegistry, this.Monitor);

            this.Maps = new IDataMap[]
            {
                new AccessibilityMap(helper.Translation),
                new BeeHouseMap(helper.Translation),
                new ScarecrowMap(helper.Translation),
                new SprinklerMap(helper.Translation, betterSprinklers, cobalt, simpleSprinklers),
                new NeedsWateringMap(helper.Translation),
                new JunimoHutMap(helper.Translation, this.PelicanFiber)
            };
        }

        /// <summary>The method invoked when the player returns to the title screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void SaveEvents_AfterReturnToTitle(object sender, EventArgs e)
        {
            this.CurrentOverlay?.Dispose();
            this.CurrentOverlay = null;

            this.GameEvents_FirstUpdateTick(sender, e);
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
                        if (this.Maps.Count() > 0)
                            this.CurrentOverlay = new DataMapOverlay(this.Maps, this.CanOverlayNow, this.Config.CombineOverlappingBorders);
                    e.SuppressButton();
                }

                // toggle disable data map overlay
                else if (controls.ToggleDisableOverlay.Contains(e.Button))
                {
                    if (overlayVisible)
                    {
                        int index = Array.IndexOf(this.Maps, this.CurrentOverlay.CurrentMap);
                        for (int a = index; a < this.Maps.Length - 1; a++)
                        {
                            this.Maps[a] = this.Maps[a + 1];
                        }
                        Array.Resize(ref this.Maps, this.Maps.Length - 1);
                        this.CurrentOverlay.Dispose();
                        this.CurrentOverlay = null;
                        if (this.Maps.Count() > 0)
                            this.CurrentOverlay = new DataMapOverlay(this.Maps, this.CanOverlayNow, this.Config.CombineOverlappingBorders);
                    }
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
                || (Game1.activeClickableMenu is CarpenterMenu && this.Helper.Reflection.GetField<bool>(Game1.activeClickableMenu, "onFarm").GetValue()) // on Robin's or Wizard's build screen
                || (this.PelicanFiber.IsLoaded && this.PelicanFiber.IsBuildMenuOpen() && this.Helper.Reflection.GetField<bool>(Game1.activeClickableMenu, "OnFarm").GetValue()); // on Pelican Fiber's build screen
        }
    }
}
