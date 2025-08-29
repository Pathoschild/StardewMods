using System;
using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Common.Integrations.GenericModConfigMenu;
using StardewModdingAPI;

namespace Pathoschild.Stardew.DataLayers.Framework;

/// <summary>Registers the mod configuration with Generic Mod Config Menu.</summary>
internal class GenericModConfigMenuIntegrationForDataLayers : IGenericModConfigMenuIntegrationFor<ModConfig>
{
    /*********
    ** Fields
    *********/
    /// <summary>The default mod settings.</summary>
    private readonly ModConfig DefaultConfig = new();

    /// <summary>Manages available color schemes and colors.</summary>
    private readonly ColorRegistry ColorRegistry;

    /// <summary>Manages the data layers that should be available in-game.</summary>
    private readonly LayerRegistry LayerRegistry;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="layerRegistry">Manages the data layers that should be available in-game.</param>
    /// <param name="colorRegistry">Manages available color schemes and colors.</param>
    public GenericModConfigMenuIntegrationForDataLayers(LayerRegistry layerRegistry, ColorRegistry colorRegistry)
    {
        this.LayerRegistry = layerRegistry;
        this.ColorRegistry = colorRegistry;
    }

    /// <inheritdoc />
    public void Register(GenericModConfigMenuIntegration<ModConfig> menu, IMonitor monitor)
    {
        // add general settings
        menu
            .Register()
            .AddSectionTitle(I18n.Config_Section_MainOptions)
            .AddCheckbox(
                name: I18n.Config_ShowGrid_Name,
                tooltip: I18n.Config_ShowGrid_Desc,
                get: config => config.ShowGrid,
                set: (config, value) => config.ShowGrid = value
            )
            .AddCheckbox(
                name: I18n.Config_CombineBorders_Name,
                tooltip: I18n.Config_CombineBorders_Desc,
                get: config => config.CombineOverlappingBorders,
                set: (config, value) => config.CombineOverlappingBorders = value
            )
            .AddDropdown(
                name: I18n.Config_ColorScheme_Name,
                tooltip: I18n.Config_ColorScheme_Desc,
                get: config => config.ColorScheme,
                set: (config, value) => config.ColorScheme = value,
                allowedValues: this.ColorRegistry.SchemeIds.ToArray(),
                formatAllowedValue: key => I18n.GetByKey($"config.color-schemes.{key}").Default(key)
            )
            .AddNumberField(
                name: I18n.Config_LegendAlphaOnHover_Name,
                tooltip: I18n.Config_LegendAlphaOnHover_Desc,
                get: config => config.LegendAlphaOnHover,
                set: (config, value) => config.LegendAlphaOnHover = value,
                min: 0,
                max: 1,
                interval: 0.05f
            )


            .AddSectionTitle(I18n.Config_Section_MainControls)
            .AddKeyBinding(
                name: I18n.Config_ToggleLayerKey_Name,
                tooltip: I18n.Config_ToggleLayerKey_Desc,
                get: config => config.Controls.ToggleLayer,
                set: (config, value) => config.Controls.ToggleLayer = value
            )
            .AddKeyBinding(
                name: I18n.Config_PrevLayerKey_Name,
                tooltip: I18n.Config_PrevLayerKey_Desc,
                get: config => config.Controls.PrevLayer,
                set: (config, value) => config.Controls.PrevLayer = value
            )
            .AddKeyBinding(
                name: I18n.Config_NextLayerKey_Name,
                tooltip: I18n.Config_NextLayerKey_Desc,
                get: config => config.Controls.NextLayer,
                set: (config, value) => config.Controls.NextLayer = value
            );

        // add layer options
        List<LayerConfigSection> configSections = [
            this.GetBuiltInSection(config => config.Layers.Accessible, "accessible"),
            this.GetBuiltInSection(config => config.Layers.AutoLayer, "auto"),
            this.GetBuiltInSection(config => config.Layers.Buildable, "buildable"),
            this.GetBuiltInSection(config => config.Layers.CoverageForBeeHouses, "bee-houses"),
            this.GetBuiltInSection(config => config.Layers.CoverageForBombs, "bombs"),
            this.GetBuiltInSection(config => config.Layers.CoverageForJunimoHuts, "junimo-huts"),
            this.GetBuiltInSection(config => config.Layers.CoverageForScarecrows, "scarecrows"),
            this.GetBuiltInSection(config => config.Layers.CoverageForSprinklers, "sprinklers"),
            this.GetBuiltInSection(config => config.Layers.CropHarvest, "crop-harvest"),
            this.GetBuiltInSection(config => config.Layers.CropWater, "crop-water"),
            this.GetBuiltInSection(config => config.Layers.CropPaddyWater, "crop-paddy-water"),
            this.GetBuiltInSection(config => config.Layers.CropFertilizer, "crop-fertilizer"),
            this.GetBuiltInSection(config => config.Layers.Machines, "machines"),
            this.GetBuiltInSection(config => config.Layers.TileGrid, "grid"),
            this.GetBuiltInSection(config => config.Layers.Tillable, "tillable"),
        ];

        foreach (ApiDataLayer layer in this.LayerRegistry.GetCustomLayerData())
        {
            configSections.Add(new LayerConfigSection(
                GetConfig: config => config.GetModLayerConfig(layer),
                GetName: () => layer.Name()
            ));
        }

        // sort layers by alphabetical name.
        // This doesn't account for the language changing later, but there's no way to handle that through Generic Mod Config Menu.
        foreach (LayerConfigSection section in configSections.OrderBy(p => p.GetName()))
            this.AddLayerConfigSection(menu, section);
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Create a config section for a data layer provided by Data Layers itself.</summary>
    /// <param name="getLayer">Get the layer field from a config model.</param>
    /// <param name="translationKey">The translation key for this layer.</param>
    private LayerConfigSection GetBuiltInSection(Func<ModConfig, LayerConfig> getLayer, string translationKey)
    {
        return new LayerConfigSection(getLayer, () => I18n.GetByKey($"{translationKey}.name"));
    }

    /// <summary>Add the config section for a layer.</summary>
    /// <param name="menu">The integration API through which to register the config menu.</param>
    /// <param name="section">Contains the information about this layer/config section.</param>
    private void AddLayerConfigSection(GenericModConfigMenuIntegration<ModConfig> menu, LayerConfigSection section)
    {
        LayerConfig defaultConfig = section.GetConfig(this.DefaultConfig);

        menu
            .AddSectionTitle(() => I18n.Config_Section_Layer(section.GetName()))
            .AddCheckbox(
                name: I18n.Config_LayerEnabled_Name,
                tooltip: I18n.Config_LayerEnabled_Desc,
                get: config => section.GetConfig(config).Enabled,
                set: (config, value) => section.GetConfig(config).Enabled = value
            );

        if (defaultConfig is LayerConfigWithAutoSupport)
        {
            menu.AddCheckbox(
                name: () => I18n.Config_LayerEnabledForAutoLayer_Name(autoLayerName: I18n.Auto_Name()),
                tooltip: () => I18n.Config_LayerEnabledForAutoLayer_Desc(autoLayerName: I18n.Auto_Name()),
                get: config => this.GetConfigWithAutoSupport(config, section).EnabledForAutoLayer,
                set: (config, value) => this.GetConfigWithAutoSupport(config, section).EnabledForAutoLayer = value
            );
        }

        menu
            .AddCheckbox(
                name: I18n.Config_LayerUpdateOnViewChange_Name,
                tooltip: I18n.Config_LayerUpdateOnViewChange_Desc,
                get: config => section.GetConfig(config).UpdateWhenViewChange,
                set: (config, value) => section.GetConfig(config).UpdateWhenViewChange = value
            )
            .AddNumberField(
                name: I18n.Config_LayerUpdatesPerSecond_Name,
                tooltip: () => I18n.Config_LayerUpdatesPerSecond_Desc(defaultValue: defaultConfig.UpdatesPerSecond),
                get: config => (float)section.GetConfig(config).UpdatesPerSecond,
                set: (config, value) => section.GetConfig(config).UpdatesPerSecond = (decimal)value,
                min: 0.1f,
                max: 60f
            )
            .AddKeyBinding(
                name: I18n.Config_LayerShortcut_Name,
                tooltip: I18n.Config_LayerShortcut_Desc,
                get: config => section.GetConfig(config).ShortcutKey,
                set: (config, value) => section.GetConfig(config).ShortcutKey = value
            );
    }

    /// <summary>Get a section config with 'auto' layer support.</summary>
    /// <param name="config">The configuration model to read.</param>
    /// <param name="section">The section to read.</param>
    /// <exception cref="InvalidCastException">The selected config doesn't support the 'auto' layer.</exception>
    private LayerConfigWithAutoSupport GetConfigWithAutoSupport(ModConfig config, LayerConfigSection section)
    {
        LayerConfig layerConfig = section.GetConfig(config);
        return (LayerConfigWithAutoSupport)layerConfig;
    }

    /// <summary>A data layer's configuration settings.</summary>
    /// <param name="GetConfig">Get the layer field from a config model.</param>
    /// <param name="GetName">Get the translated section title.</param>
    private record LayerConfigSection(Func<ModConfig, LayerConfig> GetConfig, Func<string> GetName);
}
