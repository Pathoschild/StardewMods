using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.DataLayers.Framework;
using Pathoschild.Stardew.DataLayers.Framework.Commands;
using Pathoschild.Stardew.DataLayers.Layers;
using Pathoschild.Stardew.DataLayers.Layers.Coverage;
using Pathoschild.Stardew.DataLayers.Layers.Crops;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
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
        private ModConfig Config = null!; // set in Entry

        /// <summary>The configured key bindings.</summary>
        private ModConfigKeys Keys => this.Config.Controls;

        /// <summary>The color schemes available to apply.</summary>
        private Dictionary<string, ColorScheme> ColorSchemes = null!; // loaded in Entry

        /// <summary>The display colors to use.</summary>
        private ColorScheme Colors = null!; // loaded in Entry

        /// <summary>The available data layers.</summary>
        private ILayer[] Layers = [];

        /// <summary>Maps key bindings to the layers they should activate.</summary>
        private readonly IDictionary<KeybindList, ILayer> ShortcutMap = new Dictionary<KeybindList, ILayer>();

        /// <summary>Handles access to the supported mod integrations.</summary>
        private ModIntegrations? Mods;

        /// <summary>The current overlay being displayed, if any.</summary>
        private readonly PerScreen<DataLayerOverlay?> CurrentOverlay = new();

        /// <summary>The last layer ID used by the player in this session.</summary>
        private string? LastLayerId;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
        {
            CommonHelper.RemoveObsoleteFiles(this, "DataLayers.pdb"); // removed in 1.15.8

            // read config
            this.Config = helper.ReadConfig<ModConfig>();
            this.ColorSchemes = this.LoadColorSchemes();
            this.Colors = this.LoadColorScheme();

            // validate config
            if (!this.Config.Layers.AnyLayersEnabled())
                this.Monitor.Log("You have all layers disabled in the mod settings, so the mod won't do anything currently.", LogLevel.Warn);

            // init
            I18n.Init(helper.Translation);

            // hook up events
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.ReturnedToTitle += this.OnReturnedToTitle;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;

            // hook up commands
            var commandHandler = new CommandHandler(this.Monitor, () => this.CurrentOverlay.Value?.CurrentLayer);
            commandHandler.RegisterWith(helper.ConsoleCommands);
        }


        /*********
        ** Private methods
        *********/
        /// <inheritdoc cref="IGameLoopEvents.GameLaunched"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            // init mod integrations
            this.Mods = new ModIntegrations(this.Monitor, this.Helper.ModRegistry, this.Helper.Reflection);

            // add Generic Mod Config Menu integration
            new GenericModConfigMenuIntegrationForDataLayers(
                getConfig: () => this.Config,
                reset: () =>
                {
                    this.Config = new ModConfig();
                    this.ReapplyConfig();
                },
                saveAndApply: () =>
                {
                    this.Helper.WriteConfig(this.Config);
                    this.ReapplyConfig();
                },
                modRegistry: this.Helper.ModRegistry,
                monitor: this.Monitor,
                manifest: this.ModManifest,
                colorSchemes: this.ColorSchemes
            ).Register();
        }

        /// <inheritdoc cref="IGameLoopEvents.SaveLoaded"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            // need to do this after the save is loaded so translations use the selected language
            this.ReapplyConfig();
        }

        /// <summary>Get the enabled data layers.</summary>
        /// <param name="config">The mod configuration.</param>
        /// <param name="mods">Handles access to the supported mod integrations.</param>
        private IEnumerable<ILayer> GetLayers(ModConfig config, ModIntegrations mods)
        {
            ModConfigLayers layers = config.Layers;
            var colors = this.Colors;

            if (layers.Accessible.IsEnabled())
                yield return new AccessibleLayer(layers.Accessible, colors);
            if (layers.Buildable.IsEnabled())
                yield return new BuildableLayer(layers.Buildable, colors);
            if (layers.CoverageForBeeHouses.IsEnabled())
                yield return new BeeHouseLayer(layers.CoverageForBeeHouses, colors);
            if (layers.CoverageForScarecrows.IsEnabled())
                yield return new ScarecrowLayer(layers.CoverageForScarecrows, colors);
            if (layers.CoverageForSprinklers.IsEnabled())
                yield return new SprinklerLayer(layers.CoverageForSprinklers, colors, mods);
            if (layers.CoverageForJunimoHuts.IsEnabled())
                yield return new JunimoHutLayer(layers.CoverageForJunimoHuts, colors, mods);
            if (layers.CropWater.IsEnabled())
                yield return new CropWaterLayer(layers.CropWater, colors);
            if (layers.CropPaddyWater.IsEnabled())
                yield return new CropPaddyWaterLayer(layers.CropPaddyWater, colors);
            if (layers.CropFertilizer.IsEnabled())
                yield return new CropFertilizerLayer(layers.CropFertilizer, colors, mods);
            if (layers.CropHarvest.IsEnabled())
                yield return new CropHarvestLayer(layers.CropHarvest, colors);
            if (layers.Machines.IsEnabled() && mods.Automate.IsLoaded)
                yield return new MachineLayer(layers.Machines, colors, mods);
            if (layers.Tillable.IsEnabled())
                yield return new TillableLayer(layers.Tillable, colors);

            // add separate grid layer if grid isn't enabled for all layers
            if (!config.ShowGrid && layers.TileGrid.IsEnabled())
                yield return new GridLayer(layers.TileGrid);
        }

        /// <inheritdoc cref="IGameLoopEvents.ReturnedToTitle"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
        {
            this.CurrentOverlay.Value?.Dispose();
            this.CurrentOverlay.Value = null;
            this.Layers = [];
        }

        /// <inheritdoc cref="IInputEvents.ButtonsChanged"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
        {
            if (this.Layers.Length == 0)
                return;

            // perform bound action
            this.Monitor.InterceptErrors("handling your input", () =>
            {
                // check context
                if (!this.CanOverlayNow())
                    return;
                bool overlayVisible = this.CurrentOverlay.Value != null;
                ModConfigKeys keys = this.Keys;

                // toggle overlay
                if (keys.ToggleLayer.JustPressed())
                {
                    this.ToggleLayers();
                    this.Helper.Input.SuppressActiveKeybinds(keys.ToggleLayer);
                }

                // cycle layers
                else if (overlayVisible && keys.NextLayer.JustPressed())
                {
                    this.CurrentOverlay.Value!.NextLayer();
                    this.Helper.Input.SuppressActiveKeybinds(keys.NextLayer);
                }
                else if (overlayVisible && keys.PrevLayer.JustPressed())
                {
                    this.CurrentOverlay.Value!.PrevLayer();
                    this.Helper.Input.SuppressActiveKeybinds(keys.PrevLayer);
                }

                // shortcut to layer
                else if (overlayVisible)
                {
                    foreach ((KeybindList key, ILayer layer) in this.ShortcutMap)
                    {
                        if (key.JustPressed())
                        {
                            if (layer != this.CurrentOverlay.Value!.CurrentLayer)
                            {
                                this.CurrentOverlay.Value.SetLayer(layer);
                                this.Helper.Input.SuppressActiveKeybinds(key);
                            }
                            break;
                        }
                    }
                }
            });
        }

        /// <inheritdoc cref="IGameLoopEvents.UpdateTicked"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            DataLayerOverlay? overlay = this.CurrentOverlay.Value;
            if (overlay != null)
            {
                overlay.UpdateDataLayer();
                this.LastLayerId = overlay.CurrentLayer.Id;
            }
        }

        /// <summary>Reload the mod state to match the current config options.</summary>
        private void ReapplyConfig()
        {
            // reset color scheme
            this.Colors = this.LoadColorScheme();

            // reset layers
            if (this.Mods is not null) // skip if we haven't initialized yet
            {
                this.Layers = this.GetLayers(this.Config, this.Mods).ToArray();
                this.ShortcutMap.Clear();
                foreach (ILayer layer in this.Layers)
                {
                    if (layer.ShortcutKey.IsBound)
                        this.ShortcutMap[layer.ShortcutKey] = layer;
                }
            }
        }

        /// <summary>Toggle the overlay.</summary>
        private void ToggleLayers()
        {
            if (this.CurrentOverlay.Value != null)
            {
                this.CurrentOverlay.Value.Dispose();
                this.CurrentOverlay.Value = null;
            }
            else
            {
                this.CurrentOverlay.Value = new DataLayerOverlay(this.Helper.Events, this.Helper.Input, this.Helper.Reflection, this.Layers, this.CanOverlayNow, this.Config.CombineOverlappingBorders, this.Config.ShowGrid);
                this.CurrentOverlay.Value.TrySetLayer(this.LastLayerId);
            }
        }

        /// <summary>Whether overlays are allowed in the current game context.</summary>
        private bool CanOverlayNow()
        {
            if (!Context.IsWorldReady)
                return false;

            return
                Context.IsPlayerFree // player is free to roam
                || (Game1.activeClickableMenu is CarpenterMenu carpenterMenu && carpenterMenu.onFarm) // on Robin's or Wizard's build screen
                || (this.Mods!.PelicanFiber.IsLoaded && this.Mods.PelicanFiber.IsBuildMenuOpen() && this.Helper.Reflection.GetField<bool>(Game1.activeClickableMenu, "onFarm").GetValue()); // on Pelican Fiber's build screen
        }

        /// <summary>Load the color schemes that can be applied.</summary>
        private Dictionary<string, ColorScheme> LoadColorSchemes()
        {
            // load raw data
            var rawData = this.Helper.Data.ReadJsonFile<Dictionary<string, Dictionary<string, string?>>>(ColorScheme.AssetName);
            rawData = rawData is not null
                ? new(rawData, StringComparer.OrdinalIgnoreCase)
                : new(StringComparer.OrdinalIgnoreCase);

            // load schemes
            Dictionary<string, ColorScheme> colorSchemes = new(StringComparer.OrdinalIgnoreCase);
            foreach ((string schemeId, Dictionary<string, string?> rawColors) in rawData)
            {
                Dictionary<string, Color> colors = new(StringComparer.OrdinalIgnoreCase);

                foreach ((string name, string? rawColor) in rawColors)
                {
                    Color? color = Utility.StringToColor(rawColor);

                    if (color is null)
                    {
                        this.Monitor.Log($"Can't load color '{name}' from{(!ColorScheme.IsDefaultColorScheme(this.Config.ColorScheme) ? $" color scheme '{this.Config.ColorScheme}'" : "")} '{ColorScheme.AssetName}'. The value '{rawColor}' isn't a valid color format.", LogLevel.Warn);
                        continue;
                    }

                    colors[name] = color.Value;
                }

                colorSchemes[schemeId] = new ColorScheme(schemeId, colors, this.Monitor);
            }
            return colorSchemes;
        }

        /// <summary>Load the configured color scheme.</summary>
        private ColorScheme LoadColorScheme()
        {
            // get requested scheme
            if (this.ColorSchemes.TryGetValue(this.Config.ColorScheme, out ColorScheme? scheme))
                return scheme;

            // fallback to default scheme
            if (!ColorScheme.IsDefaultColorScheme(this.Config.ColorScheme) && this.ColorSchemes.TryGetValue("Default", out scheme))
            {
                this.Monitor.Log($"Color scheme '{this.Config.ColorScheme}' not found in '{ColorScheme.AssetName}', reset to default.", LogLevel.Warn);
                this.Config.ColorScheme = "Default";
                this.Helper.WriteConfig(this.Config);

                return scheme;
            }

            // fallback to empty data
            this.Monitor.Log($"Color scheme '{this.Config.ColorScheme}' not found in '{ColorScheme.AssetName}'. The mod may be installed incorrectly.", LogLevel.Warn);
            return new ColorScheme("Default", new(), this.Monitor);
        }
    }
}
