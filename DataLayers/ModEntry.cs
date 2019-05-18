using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        ** Fields
        *********/
        /// <summary>The mod configuration.</summary>
        private ModConfig Config;

        /// <summary>The configured key bindings.</summary>
        private ModConfigKeys Keys;

        /// <summary>The current overlay being displayed, if any.</summary>
        private DataLayerOverlay CurrentOverlay;

        /// <summary>The available data layers.</summary>
        private ILayer[] Layers;

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
            this.Keys = this.Config.Controls.ParseControls(this.Monitor);

            // hook up events
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.ReturnedToTitle += this.OnReturnedToTitle;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method invoked on the first game update tick.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // init
            this.Mods = new ModIntegrations(this.Monitor, this.Helper.ModRegistry, this.Helper.Reflection);
            this.Layers = this.GetLayers(this.Config, this.Helper.Translation, this.Mods).ToArray();

            // disable Data Maps if present
            this.TryDisableLegacyMod();
        }

        /// <summary>Disable the legacy Data Maps mod, if present.</summary>
        private bool TryDisableLegacyMod()
        {
            // check if installed
            if (!this.Helper.ModRegistry.IsLoaded("Pathoschild.DataMaps"))
                return false;
            this.Monitor.Log("You also have Data Maps installed, but that's been renamed to Data Layers. You should remove the old mod to avoid issues. Data Maps will be disabled now.", LogLevel.Warn);

            // set error logic
            bool LogError(string reason)
            {
                this.Monitor.Log($"Couldn't disable Data Maps because {reason}.", LogLevel.Error);
                return false;
            }

            // get SMAPI's internal mod registry
            object registry = this.Helper.ModRegistry.GetType().GetField("Registry", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(this.Helper.ModRegistry);
            if (registry == null)
                return LogError("SMAPI's internal mod registry couldn't be accessed");

            // get Get(modID) method
            MethodInfo getByID = registry.GetType().GetMethod("Get");
            if (getByID == null)
                return LogError("SMAPI's registry get method couldn't be accessed");

            // get mod metadata
            object modMetadata;
            try
            {
                modMetadata = getByID.Invoke(registry, new object[] { "Pathoschild.DataMaps" });
            }
            catch (Exception ex)
            {
                return LogError($"SMAPI's registry get method returned an error: {ex.Message}");
            }

            // get mod entry class
            object modEntry = modMetadata.GetType().GetProperty("Mod", BindingFlags.Instance | BindingFlags.Public)?.GetValue(modMetadata);
            if (modEntry == null)
                return LogError("its entry class couldn't be loaded");

            // get config instance
            object config = modEntry.GetType().GetField("Config", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(modEntry);
            if (config == null)
                return LogError("its config instance couldn't be loaded");

            // get controls instance
            object controls = config.GetType().GetProperty("Controls", BindingFlags.Instance | BindingFlags.Public)?.GetValue(config);
            if (controls == null)
                return LogError("its control settings couldn't be loaded");

            // get key binding field
            PropertyInfo bindingProperty = controls.GetType().GetProperty("ToggleMap", BindingFlags.Instance | BindingFlags.Public);
            if (bindingProperty == null)
                return LogError("its key binding property couldn't be loaded");

            // clear key bindings
            bindingProperty.SetValue(controls, new SButton[0]);
            return true;
        }

        /// <summary>Get the enabled data layers.</summary>
        /// <param name="config">The mod configuration.</param>
        /// <param name="translation">Provides translations for the mod.</param>
        /// <param name="mods">Handles access to the supported mod integrations.</param>
        private IEnumerable<ILayer> GetLayers(ModConfig config, ITranslationHelper translation, ModIntegrations mods)
        {
            ModConfig.LayerConfigs layers = config.Layers;

            if (layers.Accessible.IsEnabled())
                yield return new AccessibleLayer(translation, layers.Accessible);
            if (layers.Buildable.IsEnabled())
                yield return new BuildableLayer(translation, layers.Buildable);
            if (layers.CoverageForBeeHouses.IsEnabled())
                yield return new BeeHouseLayer(translation, layers.CoverageForBeeHouses);
            if (layers.CoverageForScarecrows.IsEnabled())
                yield return new ScarecrowLayer(translation, layers.CoverageForScarecrows, mods);
            if (layers.CoverageForSprinklers.IsEnabled())
                yield return new SprinklerLayer(translation, layers.CoverageForSprinklers, mods);
            if (layers.CoverageForJunimoHuts.IsEnabled())
                yield return new JunimoHutLayer(translation, layers.CoverageForJunimoHuts, mods);
            if (layers.CropWater.IsEnabled())
                yield return new CropWaterLayer(translation, layers.CropWater);
            if (layers.CropFertilizer.IsEnabled())
                yield return new CropFertilizerLayer(translation, layers.CropFertilizer);
            if (layers.CropHarvest.IsEnabled())
                yield return new CropHarvestLayer(translation, layers.CropHarvest);
            if (layers.Machines.IsEnabled() && mods.Automate.IsLoaded)
                yield return new MachineLayer(translation, layers.Machines, mods);
            if (layers.Tillable.IsEnabled())
                yield return new TillableLayer(translation, layers.Tillable);
        }

        /// <summary>The method invoked when the player returns to the title screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            this.CurrentOverlay?.Dispose();
            this.CurrentOverlay = null;
        }

        /// <summary>The method invoked when the player presses an input button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // perform bound action
            this.Monitor.InterceptErrors("handling your input", $"handling input '{e.Button}'", () =>
            {
                // check context
                if (!this.CanOverlayNow())
                    return;
                bool overlayVisible = this.CurrentOverlay != null;
                ModConfigKeys keys = this.Keys;

                // toggle overlay
                if (keys.ToggleLayer.Contains(e.Button))
                {
                    if (overlayVisible)
                    {
                        this.CurrentOverlay.Dispose();
                        this.CurrentOverlay = null;
                    }
                    else
                        this.CurrentOverlay = new DataLayerOverlay(this.Helper.Events, this.Helper.Input, this.Layers, this.CanOverlayNow, this.Config.CombineOverlappingBorders);
                    this.Helper.Input.Suppress(e.Button);
                }

                // cycle layers
                else if (overlayVisible && keys.NextLayer.Contains(e.Button))
                {
                    this.CurrentOverlay.NextLayer();
                    this.Helper.Input.Suppress(e.Button);
                }
                else if (overlayVisible && keys.PrevLayer.Contains(e.Button))
                {
                    this.CurrentOverlay.PrevLayer();
                    this.Helper.Input.Suppress(e.Button);
                }
            });
        }

        /// <summary>Receive an update tick.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
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
