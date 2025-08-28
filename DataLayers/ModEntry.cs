using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Integrations.GenericModConfigMenu;
using Pathoschild.Stardew.Common.Integrations.IconicFramework;
using Pathoschild.Stardew.DataLayers.Framework;
using Pathoschild.Stardew.DataLayers.Framework.Commands;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.DataLayers;

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

    /// <summary>Manages available color schemes and colors.</summary>
    private ColorRegistry ColorRegistry = null!; // loaded in Entry

    /// <summary>The current display colors to use.</summary>
    private ColorScheme Colors = null!; // loaded in Entry

    /// <summary>Manages the data layers that should be available in-game.</summary>
    private LayerRegistry LayerRegistry = null!; // loaded in Entry

    /// <summary>Handles access to the supported mod integrations.</summary>
    private ModIntegrations? Mods;

    /// <summary>The current overlay being displayed, if any.</summary>
    private readonly PerScreen<DataLayerOverlay?> CurrentOverlay = new();

    /// <summary>The last layer ID used by the player in this session.</summary>
    private string? LastLayerId;


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        CommonHelper.RemoveObsoleteFiles(this, "DataLayers.pdb"); // removed in 1.15.8

        // load config
        this.Config = helper.ReadConfig<ModConfig>();
        if (!this.Config.Layers.AnyLayersEnabled())
            this.Monitor.Log("You have all layers disabled in the mod settings, so the mod won't do anything currently.", LogLevel.Warn);

        // load translations
        I18n.Init(helper.Translation);

        // load color scheme
        this.ColorRegistry = new(helper.Data, this.Monitor);
        this.Colors = this.LoadColorScheme();

        // init layers & API
        this.LayerRegistry = new(() => this.Colors, () => this.Config, () => this.Mods);

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

    /// <inheritdoc />
    public override object GetApi(IModInfo mod)
    {
        return new Api(mod.Manifest.UniqueID, this.LayerRegistry);
    }


    /*********
    ** Private methods
    *********/
    /// <inheritdoc cref="IGameLoopEvents.GameLaunched"/>
    [EventPriority(EventPriority.Low)] // run at low priority so other mods can register layers in their OnGameLaunched handlers.
    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        // init mod integrations
        this.Mods = new ModIntegrations(this.Monitor, this.Helper.ModRegistry, this.Helper.Reflection);

        // add config UI
        this.AddGenericModConfigMenu(
            new GenericModConfigMenuIntegrationForDataLayers(this.LayerRegistry, this.ColorRegistry),
            get: () => this.Config,
            set: config => this.Config = config,
            onSaved: this.ReapplyConfig
        );

        // add Iconic Framework icon
        IconicFrameworkIntegration iconicFramework = new(this.Helper.ModRegistry, this.Monitor);
        if (iconicFramework.IsLoaded)
        {
            iconicFramework.AddToolbarIcon(
                this.Helper.ModContent.GetInternalAssetName("assets/icon.png").BaseName,
                new Rectangle(0, 0, 16, 16),
                I18n.Icon_ToggleDataLayers_Name,
                I18n.Icon_ToggleDataLayers_Desc,
                this.ToggleLayers
            );
        }
    }

    /// <inheritdoc cref="IGameLoopEvents.SaveLoaded" />
    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        // need to do this after the save is loaded so translations use the selected language
        this.ReapplyConfig();
    }

    /// <inheritdoc cref="IGameLoopEvents.ReturnedToTitle" />
    private void OnReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
    {
        this.CurrentOverlay.Value?.Dispose();
        this.CurrentOverlay.Value = null;

        this.LayerRegistry.ResetCache();
    }

    /// <inheritdoc cref="IInputEvents.ButtonsChanged" />
    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (!this.LayerRegistry.IsReady)
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
                if (this.LayerRegistry.TryGetLayerByKeybind(out ILayer? layer, out KeybindList? key) && layer != this.CurrentOverlay.Value!.CurrentLayer)
                {
                    this.CurrentOverlay.Value.SetLayer(layer);
                    this.Helper.Input.SuppressActiveKeybinds(key);
                }
            }
        });
    }

    /// <inheritdoc cref="IGameLoopEvents.UpdateTicked" />
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
        // reset cached data
        this.Colors = this.LoadColorScheme();
        this.LayerRegistry.ResetCache();
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
            this.CurrentOverlay.Value = new DataLayerOverlay(this.Helper.Events, this.Helper.Input, this.Helper.Reflection, this.LayerRegistry.GetLayers(), this.CanOverlayNow, this.Config.CombineOverlappingBorders, this.Config.ShowGrid, this.Config.LegendAlphaOnHover);
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
            || Game1.activeClickableMenu is CarpenterMenu { onFarm: true } // on Robin's or Wizard's build screen
            || (this.Mods?.PelicanFiber.IsLoaded is true && this.Mods.PelicanFiber.IsBuildMenuOpen() && this.Helper.Reflection.GetField<bool>(Game1.activeClickableMenu, "onFarm").GetValue()); // on Pelican Fiber's build screen
    }

    /// <summary>Load the configured color scheme.</summary>
    private ColorScheme LoadColorScheme()
    {
        // get requested scheme
        if (this.ColorRegistry.TryGetScheme(this.Config.ColorScheme, out ColorScheme? scheme))
            return scheme;

        // fallback to default scheme
        if (!ColorScheme.IsDefaultColorScheme(this.Config.ColorScheme) && this.ColorRegistry.TryGetScheme("Default", out scheme))
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
