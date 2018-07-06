using System;
using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.DataLayers.Framework;
using Pathoschild.Stardew.DataLayers.Layers;
using Pathoschild.Stardew.DataLayers.Layers.Coverage;
using Pathoschild.Stardew.DataLayers.Layers.Crops;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.DataLayers
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

        /// <summary>Handles access to the supported mod integrations.</summary>
        private ModIntegrations Mods;


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
            GameEvents.UpdateTick += this.GameEvents_UpdateTick;
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

            this.Mods = new ModIntegrations(this.Monitor, helper.ModRegistry, helper.Reflection);
            this.Maps = this.GetDataMaps(this.Config, this.Helper.Translation, this.Mods).ToArray();
        }

        /// <summary>Get the enabled data maps.</summary>
        /// <param name="config">The mod configuration.</param>
        /// <param name="translation">Provides translations for the mod.</param>
        /// <param name="mods">Handles access to the supported mod integrations.</param>
        private IEnumerable<IDataMap> GetDataMaps(ModConfig config, ITranslationHelper translation, ModIntegrations mods)
        {
            var maps = config.DataMaps;

            if (maps.Accessibility.IsEnabled())
                yield return new AccessibilityMap(translation, maps.Accessibility);
            if (maps.CoverageForBeeHouses.IsEnabled())
                yield return new BeeHouseMap(translation, maps.CoverageForBeeHouses);
            if (maps.CoverageForScarecrows.IsEnabled())
                yield return new ScarecrowMap(translation, maps.CoverageForScarecrows);
            if (maps.CoverageForSprinklers.IsEnabled())
                yield return new SprinklerMap(translation, maps.CoverageForSprinklers, mods);
            if (maps.CoverageForJunimoHuts.IsEnabled())
                yield return new JunimoHutMap(translation, maps.CoverageForJunimoHuts, mods);
            if (maps.CropWater.IsEnabled())
                yield return new CropWaterMap(translation, maps.CropWater);
            if (maps.CropFertilizer.IsEnabled())
                yield return new CropFertilizerMap(translation, maps.CropFertilizer);
        }

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
                        this.CurrentOverlay = new DataMapOverlay(this.Maps, this.CanOverlayNow, this.Config.CombineOverlappingBorders);
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
        private void GameEvents_UpdateTick(object sender, EventArgs e)
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
                || (this.Mods.PelicanFiber.IsLoaded && this.Mods.PelicanFiber.IsBuildMenuOpen() && this.Helper.Reflection.GetField<bool>(Game1.activeClickableMenu, "OnFarm").GetValue()); // on Pelican Fiber's build screen
        }
    }
}
