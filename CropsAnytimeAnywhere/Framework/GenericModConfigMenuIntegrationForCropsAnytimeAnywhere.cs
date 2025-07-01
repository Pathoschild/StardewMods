using System;
using System.Collections.Generic;
using Pathoschild.Stardew.Common.Integrations.GenericModConfigMenu;
using StardewModdingAPI;

namespace Pathoschild.Stardew.CropsAnytimeAnywhere.Framework;

/// <summary>Registers the mod configuration with Generic Mod Config Menu.</summary>
internal class GenericModConfigMenuIntegrationForCropsAnytimeAnywhere : IGenericModConfigMenuIntegrationFor<ModConfig>
{
    /*********
    ** Fields
    *********/
    /// <summary>The default mod settings.</summary>
    private readonly ModConfig DefaultConfig = new();

    /// <summary>Whether the current settings are too complex to edit through Generic Mod Config Menu.</summary>
    private readonly bool TooComplex;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="config">The current config model.</param>
    public GenericModConfigMenuIntegrationForCropsAnytimeAnywhere(ModConfig config)
    {
        this.TooComplex = config.Locations.Count switch
        {
            0 => false, // we can re-add the default section
            1 => !config.Locations.ContainsKey("*"), // only contains the default section
            _ => true
        };
    }

    /// <inheritdoc />
    public void Register(GenericModConfigMenuIntegration<ModConfig> menu, IMonitor monitor)
    {
        menu.Register();

        if (this.TooComplex)
        {
            menu.AddParagraph(I18n.Config_TooComplex);
            return;
        }

        menu
            .AddCheckbox(
                name: I18n.Config_GrowCrops_Name,
                tooltip: I18n.Config_GrowCrops_Desc,
                get: config => this.GetOption(config, p => p.GrowCrops),
                set: (config, value) => this.SetOption(config, p => new PlantRule(value, p.GrowCropsOutOfSeason, p.UseFruitTreesSeasonalSprites, new TillableRule(p.ForceTillable)))
            )
            .AddCheckbox(
                name: I18n.Config_GrowCropsOutOfSeason_Name,
                tooltip: I18n.Config_GrowCropsOutOfSeason_Desc,
                get: config => this.GetOption(config, p => p.GrowCropsOutOfSeason),
                set: (config, value) => this.SetOption(config, p => new PlantRule(p.GrowCrops, value, p.UseFruitTreesSeasonalSprites, new TillableRule(p.ForceTillable)))
            )
            .AddCheckbox(
                name: I18n.Config_UseFruitTreesSeasonalSprites_Name,
                tooltip: I18n.Config_UseFruitTreesSeasonalSprites_Desc,
                get: config => this.GetOption(config, p => p.UseFruitTreesSeasonalSprites),
                set: (config, value) => this.SetOption(config, p => new PlantRule(p.GrowCrops, p.GrowCropsOutOfSeason, value, new TillableRule(p.ForceTillable)))
            )
            .AddCheckbox(
                name: I18n.Config_ForceTillDirt_Name,
                tooltip: I18n.Config_ForceTillDirt_Desc,
                get: config => this.GetTillableOption(config, p => p.Dirt),
                set: (config, value) => this.SetTillableOption(config, p => new TillableRule(value, p.Grass, p.Stone, p.Other))
            )
            .AddCheckbox(
                name: I18n.Config_ForceTillGrass_Name,
                tooltip: I18n.Config_ForceTillGrass_Desc,
                get: config => this.GetTillableOption(config, p => p.Grass),
                set: (config, value) => this.SetTillableOption(config, p => new TillableRule(p.Dirt, value, p.Stone, p.Other))
            )
            .AddCheckbox(
                name: I18n.Config_ForceTillStone_Name,
                tooltip: I18n.Config_ForceTillStone_Desc,
                get: config => this.GetTillableOption(config, p => p.Stone),
                set: (config, value) => this.SetTillableOption(config, p => new TillableRule(p.Dirt, p.Grass, value, p.Other))
            )
            .AddCheckbox(
                name: I18n.Config_ForceTillOther_Name,
                tooltip: I18n.Config_ForceTillOther_Desc,
                get: config => this.GetTillableOption(config, p => p.Other),
                set: (config, value) => this.SetTillableOption(config, p => new TillableRule(p.Dirt, p.Grass, p.Stone, value))
            );
    }


    /*********
    ** Private methods
    *********/
    private bool GetOption(ModConfig config, Func<PlantRule, bool> getValue)
    {
        PlantRule section = config.Locations.GetValueOrDefault("*") ?? this.DefaultConfig.Locations["*"];
        return getValue(section);
    }

    private void SetOption(ModConfig config, Func<PlantRule, PlantRule> createNewConfig)
    {
        PlantRule section = config.Locations.GetValueOrDefault("*") ?? this.DefaultConfig.Locations["*"];

        config.Locations["*"] = createNewConfig(section);
    }

    private bool GetTillableOption(ModConfig config, Func<TillableRule, bool> getValue)
    {
        PlantRule section = config.Locations.GetValueOrDefault("*") ?? this.DefaultConfig.Locations["*"];
        return getValue(section.ForceTillable);
    }

    private void SetTillableOption(ModConfig config, Func<TillableRule, TillableRule> createNewConfig)
    {
        PlantRule section = config.Locations.GetValueOrDefault("*") ?? this.DefaultConfig.Locations["*"];

        config.Locations["*"] = new PlantRule(section.GrowCrops, section.GrowCropsOutOfSeason, section.UseFruitTreesSeasonalSprites, createNewConfig(section.ForceTillable));
    }
}
