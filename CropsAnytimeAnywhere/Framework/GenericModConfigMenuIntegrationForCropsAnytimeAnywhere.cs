using System;
using System.Collections.Generic;
using Pathoschild.Stardew.Common.Integrations.GenericModConfigMenu;
using StardewModdingAPI;

namespace Pathoschild.Stardew.CropsAnytimeAnywhere.Framework
{
    /// <summary>Registers the mod configuration with Generic Mod Config Menu.</summary>
    internal class GenericModConfigMenuIntegrationForCropsAnytimeAnywhere
    {
        /*********
        ** Fields
        *********/
        /// <summary>The Generic Mod Config Menu integration.</summary>
        private readonly GenericModConfigMenuIntegration<ModConfig> ConfigMenu;

        /// <summary>The default mod settings.</summary>
        private readonly ModConfig DefaultConfig = new();

        /// <summary>Whether the current settings are too complex to edit through Generic Mod Config Menu.</summary>
        private readonly bool TooComplex;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="manifest">The mod manifest.</param>
        /// <param name="getConfig">Get the current config model.</param>
        /// <param name="reset">Reset the config model to the default values.</param>
        /// <param name="saveAndApply">Save and apply the current config model.</param>
        public GenericModConfigMenuIntegrationForCropsAnytimeAnywhere(IModRegistry modRegistry, IMonitor monitor, IManifest manifest, Func<ModConfig> getConfig, Action reset, Action saveAndApply)
        {
            this.ConfigMenu = new GenericModConfigMenuIntegration<ModConfig>(modRegistry, monitor, manifest, getConfig, reset, saveAndApply);
            this.TooComplex = GenericModConfigMenuIntegrationForCropsAnytimeAnywhere.CheckIfTooComplexToEdit(getConfig());
        }

        /// <summary>Register the config menu if available.</summary>
        public void Register()
        {
            var menu = this.ConfigMenu;
            if (!menu.IsLoaded)
                return;

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
                    set: (config, value) => this.SetOption(config, p => new PerLocationConfig(value, p.GrowCropsOutOfSeason, p.UseFruitTreesSeasonalSprites, new ModConfigForceTillable(p.ForceTillable)))
                )
                .AddCheckbox(
                    name: I18n.Config_GrowCropsOutOfSeason_Name,
                    tooltip: I18n.Config_GrowCropsOutOfSeason_Desc,
                    get: config => this.GetOption(config, p => p.GrowCropsOutOfSeason),
                    set: (config, value) => this.SetOption(config, p => new PerLocationConfig(p.GrowCrops, value, p.UseFruitTreesSeasonalSprites, new ModConfigForceTillable(p.ForceTillable)))
                )
                .AddCheckbox(
                    name: I18n.Config_UseFruitTreesSeasonalSprites_Name,
                    tooltip: I18n.Config_UseFruitTreesSeasonalSprites_Desc,
                    get: config => this.GetOption(config, p => p.UseFruitTreesSeasonalSprites),
                    set: (config, value) => this.SetOption(config, p => new PerLocationConfig(p.GrowCrops, p.GrowCropsOutOfSeason, value, new ModConfigForceTillable(p.ForceTillable)))
                )
                .AddCheckbox(
                    name: I18n.Config_ForceTillDirt_Name,
                    tooltip: I18n.Config_ForceTillDirt_Desc,
                    get: config => this.GetTillableOption(config, p => p.Dirt),
                    set: (config, value) => this.SetTillableOption(config, p => new ModConfigForceTillable(value, p.Grass, p.Stone, p.Other))
                )
                .AddCheckbox(
                    name: I18n.Config_ForceTillGrass_Name,
                    tooltip: I18n.Config_ForceTillGrass_Desc,
                    get: config => this.GetTillableOption(config, p => p.Grass),
                    set: (config, value) => this.SetTillableOption(config, p => new ModConfigForceTillable(p.Dirt, value, p.Stone, p.Other))
                )
                .AddCheckbox(
                    name: I18n.Config_ForceTillStone_Name,
                    tooltip: I18n.Config_ForceTillStone_Desc,
                    get: config => this.GetTillableOption(config, p => p.Stone),
                    set: (config, value) => this.SetTillableOption(config, p => new ModConfigForceTillable(p.Dirt, p.Grass, value, p.Other))
                )
                .AddCheckbox(
                    name: I18n.Config_ForceTillOther_Name,
                    tooltip: I18n.Config_ForceTillOther_Desc,
                    get: config => this.GetTillableOption(config, p => p.Other),
                    set: (config, value) => this.SetTillableOption(config, p => new ModConfigForceTillable(p.Dirt, p.Grass, p.Stone, value))
                );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get whether settings are too complex to edit through Generic Mod Config Menu.</summary>
        /// <param name="config">The settings to check.</param>
        private static bool CheckIfTooComplexToEdit(ModConfig config)
        {
            switch (config.Locations.Count)
            {
                case 0:
                    return false; // we can re-add the default section

                case 1:
                    return !config.Locations.ContainsKey("*"); // only contains the default section

                default:
                    return true; // can't manage multiple sections through config UI
            }
        }

        private bool GetOption(ModConfig config, Func<PerLocationConfig, bool> getValue)
        {
            PerLocationConfig section = config.Locations.GetValueOrDefault("*") ?? this.DefaultConfig.Locations["*"];
            return getValue(section);
        }

        private void SetOption(ModConfig config, Func<PerLocationConfig, PerLocationConfig> createNewConfig)
        {
            PerLocationConfig section = config.Locations.GetValueOrDefault("*") ?? this.DefaultConfig.Locations["*"];

            config.Locations["*"] = createNewConfig(section);
        }

        private bool GetTillableOption(ModConfig config, Func<ModConfigForceTillable, bool> getValue)
        {
            PerLocationConfig section = config.Locations.GetValueOrDefault("*") ?? this.DefaultConfig.Locations["*"];
            return getValue(section.ForceTillable);
        }

        private void SetTillableOption(ModConfig config, Func<ModConfigForceTillable, ModConfigForceTillable> createNewConfig)
        {
            PerLocationConfig section = config.Locations.GetValueOrDefault("*") ?? this.DefaultConfig.Locations["*"];

            config.Locations["*"] = new PerLocationConfig(section.GrowCrops, section.GrowCropsOutOfSeason, section.UseFruitTreesSeasonalSprites, createNewConfig(section.ForceTillable));
        }
    }
}
