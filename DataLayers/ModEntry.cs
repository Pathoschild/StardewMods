using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.DataLayers.Framework;
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
        private ModConfig Config;

        /// <summary>The configured key bindings.</summary>
        private ModConfigKeys Keys => this.Config.Controls;

        /// <summary>The available data layers.</summary>
        private ILayer[] Layers;

        /// <summary>Maps key bindings to the layers they should activate.</summary>
        private readonly IDictionary<KeybindList, ILayer> ShortcutMap = new Dictionary<KeybindList, ILayer>();

        /// <summary>Handles access to the supported mod integrations.</summary>
        private ModIntegrations Mods;

        /// <summary>The current overlay being displayed, if any.</summary>
        private readonly PerScreen<DataLayerOverlay> CurrentOverlay = new();

        /// <summary>The last layer ID used by the player in this session.</summary>
        private string LastLayerId;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
        {
            // read config
            this.Config = helper.ReadConfig<ModConfig>();

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
            helper.ConsoleCommands.Add(commandHandler.CommandName, $"Starts a Data Layers command. Type '{commandHandler.CommandName} help' for details.", (_, args) => commandHandler.Handle(args));
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method invoked on the first game update tick.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // init mod integrations
            this.Mods = new ModIntegrations(this.Monitor, this.Helper.ModRegistry, this.Helper.Reflection);
        }

        /// <summary>The method invoked when the save is loaded.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            // init layers
            // need to do this after the save is loaded so translations use the selected language
            this.Layers = this.GetLayers(this.Config, this.Mods).ToArray();
            foreach (ILayer layer in this.Layers)
            {
                if (layer.ShortcutKey.IsBound)
                    this.ShortcutMap[layer.ShortcutKey] = layer;
            }
        }

        /// <summary>Get the enabled data layers.</summary>
        /// <param name="config">The mod configuration.</param>
        /// <param name="mods">Handles access to the supported mod integrations.</param>
        private IEnumerable<ILayer> GetLayers(ModConfig config, ModIntegrations mods)
        {
            ModConfig.LayerConfigs layers = config.Layers;

            if (layers.Accessible.IsEnabled())
                yield return new AccessibleLayer(layers.Accessible);
            if (layers.Buildable.IsEnabled())
                yield return new BuildableLayer(layers.Buildable);
            if (layers.CoverageForBeeHouses.IsEnabled())
                yield return new BeeHouseLayer(layers.CoverageForBeeHouses);
            if (layers.CoverageForScarecrows.IsEnabled())
                yield return new ScarecrowLayer(layers.CoverageForScarecrows, mods);
            if (layers.CoverageForSprinklers.IsEnabled())
                yield return new SprinklerLayer(layers.CoverageForSprinklers, mods);
            if (layers.CoverageForJunimoHuts.IsEnabled())
                yield return new JunimoHutLayer(layers.CoverageForJunimoHuts, mods);
            if (layers.CropWater.IsEnabled())
                yield return new CropWaterLayer(layers.CropWater);
            if (layers.CropPaddyWater.IsEnabled())
                yield return new CropPaddyWaterLayer(layers.CropPaddyWater);
            if (layers.CropFertilizer.IsEnabled())
                yield return new CropFertilizerLayer(layers.CropFertilizer);
            if (layers.CropHarvest.IsEnabled())
                yield return new CropHarvestLayer(layers.CropHarvest);
            if (layers.Machines.IsEnabled() && mods.Automate.IsLoaded)
                yield return new MachineLayer(layers.Machines, mods);
            if (layers.Tillable.IsEnabled())
                yield return new TillableLayer(layers.Tillable);

            // add separate grid layer if grid isn't enabled for all layers
            if (!config.ShowGrid && layers.TileGrid.IsEnabled())
                yield return new GridLayer(layers.TileGrid);
        }

        /// <summary>The method invoked when the player returns to the title screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            this.CurrentOverlay.Value?.Dispose();
            this.CurrentOverlay.Value = null;
            this.Layers = null;
        }

        /// <summary>Raised after the player presses any buttons on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (this.Layers == null)
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
                    if (overlayVisible)
                    {
                        this.CurrentOverlay.Value.Dispose();
                        this.CurrentOverlay.Value = null;
                    }
                    else
                    {
                        this.CurrentOverlay.Value = new DataLayerOverlay(this.Helper.Events, this.Helper.Input, this.Helper.Reflection, this.Layers, this.CanOverlayNow, this.Config.CombineOverlappingBorders, this.Config.ShowGrid);
                        this.CurrentOverlay.Value.TrySetLayer(this.LastLayerId);
                    }
                    this.Helper.Input.SuppressActiveKeybinds(keys.ToggleLayer);
                }

                // cycle layers
                else if (overlayVisible && keys.NextLayer.JustPressed())
                {
                    this.CurrentOverlay.Value.NextLayer();
                    this.Helper.Input.SuppressActiveKeybinds(keys.NextLayer);
                }
                else if (overlayVisible && keys.PrevLayer.JustPressed())
                {
                    this.CurrentOverlay.Value.PrevLayer();
                    this.Helper.Input.SuppressActiveKeybinds(keys.PrevLayer);
                }

                // shortcut to layer
                else if (overlayVisible)
                {
                    foreach (var pair in this.ShortcutMap)
                    {
                        if (pair.Key.JustPressed())
                        {
                            if (pair.Value != this.CurrentOverlay.Value.CurrentLayer)
                            {
                                this.CurrentOverlay.Value.SetLayer(pair.Value);
                                this.Helper.Input.SuppressActiveKeybinds(pair.Key);
                            }
                            break;
                        }
                    }
                }
            });
        }

        /// <summary>Receive an update tick.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            var overlay = this.CurrentOverlay.Value;
            if (overlay != null)
            {
                overlay.Update();
                this.LastLayerId = overlay.CurrentLayer.Id;
            }
        }

        /// <summary>Whether overlays are allowed in the current game context.</summary>
        private bool CanOverlayNow()
        {
            if (!Context.IsWorldReady)
                return false;

            return
                Context.IsPlayerFree // player is free to roam
                || (Game1.activeClickableMenu is CarpenterMenu && this.Helper.Reflection.GetField<bool>(Game1.activeClickableMenu, "onFarm").GetValue()) // on Robin's or Wizard's build screen
                || (this.Mods.PelicanFiber.IsLoaded && this.Mods.PelicanFiber.IsBuildMenuOpen() && this.Helper.Reflection.GetField<bool>(Game1.activeClickableMenu, "onFarm").GetValue()); // on Pelican Fiber's build screen
        }
    }
}
