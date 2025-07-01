using System;
using System.Linq;
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
        this.TooComplex =
            config.PlantRules.Any(rule => rule.HasConditions)
            || config.TillableRules.Any(rule => rule.HasConditions);
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
            // plant rule
            .AddCheckbox(
                name: I18n.Config_GrowCrops_Name,
                tooltip: I18n.Config_GrowCrops_Desc,
                get: config => this.GetPlantOption(config, p => p.CanPlant),
                set: (config, value) => this.SetPlantOption(config, rule => new PlantRule(rule, value, rule.CanGrowOutOfSeason, rule.UseFruitTreesSeasonalSprites))
            )
            .AddCheckbox(
                name: I18n.Config_GrowCropsOutOfSeason_Name,
                tooltip: I18n.Config_GrowCropsOutOfSeason_Desc,
                get: config => this.GetPlantOption(config, p => p.CanGrowOutOfSeason),
                set: (config, value) => this.SetPlantOption(config, rule => new PlantRule(rule, rule.CanPlant, value, rule.UseFruitTreesSeasonalSprites))
            )
            .AddCheckbox(
                name: I18n.Config_UseFruitTreesSeasonalSprites_Name,
                tooltip: I18n.Config_UseFruitTreesSeasonalSprites_Desc,
                get: config => this.GetPlantOption(config, p => p.UseFruitTreesSeasonalSprites),
                set: (config, value) => this.SetPlantOption(config, rule => new PlantRule(rule, rule.CanPlant, rule.CanGrowOutOfSeason, value))
            )

            // tillable rules
            .AddCheckbox(
                name: I18n.Config_ForceTillDirt_Name,
                tooltip: I18n.Config_ForceTillDirt_Desc,
                get: config => this.GetTillableOption(config, p => p.Dirt),
                set: (config, value) => this.SetTillableOption(config, rule => new TillableRule(rule, value, rule.Grass, rule.Stone, rule.Other))
            )
            .AddCheckbox(
                name: I18n.Config_ForceTillGrass_Name,
                tooltip: I18n.Config_ForceTillGrass_Desc,
                get: config => this.GetTillableOption(config, p => p.Grass),
                set: (config, value) => this.SetTillableOption(config, rule => new TillableRule(rule, rule.Dirt, value, rule.Stone, rule.Other))
            )
            .AddCheckbox(
                name: I18n.Config_ForceTillStone_Name,
                tooltip: I18n.Config_ForceTillStone_Desc,
                get: config => this.GetTillableOption(config, p => p.Stone),
                set: (config, value) => this.SetTillableOption(config, rule => new TillableRule(rule, rule.Dirt, rule.Grass, value, rule.Other))
            )
            .AddCheckbox(
                name: I18n.Config_ForceTillOther_Name,
                tooltip: I18n.Config_ForceTillOther_Desc,
                get: config => this.GetTillableOption(config, p => p.Other),
                set: (config, value) => this.SetTillableOption(config, rule => new TillableRule(rule, rule.Dirt, rule.Grass, rule.Stone, value))
            );
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get an option from the global plant rule.</summary>
    /// <param name="config">The mod configuration to read.</param>
    /// <param name="getValue">Get the value from the rule.</param>
    private bool GetPlantOption(ModConfig config, Func<PlantRule, bool> getValue)
    {
        PlantRule rule = config.PlantRules.FirstOrDefault() ?? this.DefaultConfig.PlantRules[0];
        return getValue(rule);
    }

    /// <summary>Set an option on the global plant rule.</summary>
    /// <param name="config">The mod configuration to update.</param>
    /// <param name="createNewConfig">Create the new config rule.</param>
    private void SetPlantOption(ModConfig config, Func<PlantRule, PlantRule> createNewConfig)
    {
        PlantRule rule = config.PlantRules.FirstOrDefault() ?? this.DefaultConfig.PlantRules[0];

        config.PlantRules.Clear();
        config.PlantRules.Add(createNewConfig(rule));
    }

    /// <summary>Get an option from the global force-tillable rule.</summary>
    /// <param name="config">The mod configuration to read.</param>
    /// <param name="getValue">Get the value from the rule.</param>
    private bool GetTillableOption(ModConfig config, Func<TillableRule, bool> getValue)
    {
        TillableRule rule = config.TillableRules.FirstOrDefault() ?? this.DefaultConfig.TillableRules[0];
        return getValue(rule);
    }

    /// <summary>Set an option on the global force-tillable rule.</summary>
    /// <param name="config">The mod configuration to update.</param>
    /// <param name="createNewConfig">Create the new config rule.</param>
    private void SetTillableOption(ModConfig config, Func<TillableRule, TillableRule> createNewConfig)
    {
        TillableRule rule = config.TillableRules.FirstOrDefault() ?? this.DefaultConfig.TillableRules[0];

        config.TillableRules.Clear();
        config.TillableRules.Add(createNewConfig(rule));
    }
}
