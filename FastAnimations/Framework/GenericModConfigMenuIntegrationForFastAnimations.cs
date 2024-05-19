using System;
using Pathoschild.Stardew.Common.Integrations.GenericModConfigMenu;
using StardewModdingAPI;

namespace Pathoschild.Stardew.FastAnimations.Framework
{
    /// <summary>Registers the mod configuration with Generic Mod Config Menu.</summary>
    internal class GenericModConfigMenuIntegrationForFastAnimations
    {
        /*********
        ** Fields
        *********/
        /// <summary>The Generic Mod Config Menu integration.</summary>
        private readonly GenericModConfigMenuIntegration<ModConfig> ConfigMenu;


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
        public GenericModConfigMenuIntegrationForFastAnimations(IModRegistry modRegistry, IMonitor monitor, IManifest manifest, Func<ModConfig> getConfig, Action reset, Action saveAndApply)
        {
            this.ConfigMenu = new GenericModConfigMenuIntegration<ModConfig>(modRegistry, monitor, manifest, getConfig, reset, saveAndApply);
        }

        /// <summary>Register the config menu if available.</summary>
        public void Register()
        {
            var defaultConfig = new ModConfig();

            // get config menu
            var menu = this.ConfigMenu;
            if (!menu.IsLoaded)
                return;

            // register
            const int minSpeed = 1;
            const int maxSpeed = 20;
            menu
                .Register()

                .AddSectionTitle(I18n.Config_GeneralOptions)
                .AddCheckbox(
                    name: I18n.Config_DisableEatPrompt_Name,
                    tooltip: I18n.Config_DisableEatPrompt_Tooltip,
                    get: config => config.DisableEatAndDrinkConfirmation,
                    set: (config, value) => config.DisableEatAndDrinkConfirmation = value
                )

                .AddSectionTitle(I18n.Config_AnimationSpeeds)
                .AddNumberField(
                    name: I18n.Config_EatOrDrink_Name,
                    tooltip: () => I18n.Config_EatOrDrink_Tooltip(defaultValue: defaultConfig.EatAndDrinkSpeed),
                    get: config => config.EatAndDrinkSpeed,
                    set: (config, value) => config.EatAndDrinkSpeed = value,
                    min: minSpeed,
                    max: maxSpeed
                )
                .AddNumberField(
                    name: I18n.Config_Fish_Name,
                    tooltip: () => I18n.Config_Fish_Tooltip(defaultValue: defaultConfig.FishingSpeed, suggestedValue: 2),
                    get: config => config.FishingSpeed,
                    set: (config, value) => config.FishingSpeed = value,
                    min: minSpeed,
                    max: maxSpeed
                )
                .AddNumberField(
                    name: I18n.Config_Harvest_Name,
                    tooltip: () => I18n.Config_Harvest_Tooltip(defaultValue: defaultConfig.HarvestSpeed),
                    get: config => config.HarvestSpeed,
                    set: (config, value) => config.HarvestSpeed = value,
                    min: minSpeed,
                    max: maxSpeed
                )
                .AddNumberField(
                    name: I18n.Config_Milk_Name,
                    tooltip: () => I18n.Config_Milk_Tooltip(defaultValue: defaultConfig.MilkSpeed),
                    get: config => config.MilkSpeed,
                    set: (config, value) => config.MilkSpeed = value,
                    min: minSpeed,
                    max: maxSpeed
                )
                .AddNumberField(
                    name: I18n.Config_Mount_Name,
                    tooltip: () => I18n.Config_Mount_Tooltip(defaultValue: defaultConfig.MountOrDismountSpeed),
                    get: config => config.MountOrDismountSpeed,
                    set: (config, value) => config.MountOrDismountSpeed = value,
                    min: minSpeed,
                    max: maxSpeed
                )
                .AddNumberField(
                    name: I18n.Config_HorseFlute_Name,
                    tooltip: () => I18n.Config_HorseFlute_Tooltip(defaultValue: defaultConfig.HorseFluteSpeed),
                    get: config => config.HorseFluteSpeed,
                    set: (config, value) => config.HorseFluteSpeed = value,
                    min: minSpeed,
                    max: maxSpeed
                )
                .AddNumberField(
                    name: I18n.Config_Shear_Name,
                    tooltip: () => I18n.Config_Shear_Tooltip(defaultValue: defaultConfig.ShearSpeed),
                    get: config => config.ShearSpeed,
                    set: (config, value) => config.ShearSpeed = value,
                    min: minSpeed,
                    max: maxSpeed
                )
                .AddNumberField(
                    name: I18n.Config_Tool_Name,
                    tooltip: () => I18n.Config_Tool_Tooltip(defaultValue: defaultConfig.ToolSwingSpeed, suggestedValue: 2),
                    get: config => config.ToolSwingSpeed,
                    set: (config, value) => config.ToolSwingSpeed = value,
                    min: minSpeed,
                    max: maxSpeed
                )
                .AddNumberField(
                    name: I18n.Config_Slingshot_Name,
                    tooltip: () => I18n.Config_Slingshot_Tooltip(defaultValue: defaultConfig.UseSlingshotSpeed, suggestedValue: 1.1),
                    get: config => config.UseSlingshotSpeed,
                    set: (config, value) => config.UseSlingshotSpeed = value,
                    min: minSpeed,
                    max: maxSpeed
                )
                .AddNumberField(
                    name: I18n.Config_Weapon_Name,
                    tooltip: () => I18n.Config_Weapon_Tooltip(defaultValue: defaultConfig.WeaponSwingSpeed, suggestedValue: 4),
                    get: config => config.WeaponSwingSpeed,
                    set: (config, value) => config.WeaponSwingSpeed = value,
                    min: minSpeed,
                    max: maxSpeed
                )

                .AddSectionTitle(I18n.Config_WorldSpeeds)
                .AddNumberField(
                    name: I18n.Config_BreakGeodes_Name,
                    tooltip: () => I18n.Config_BreakGeodes_Tooltip(defaultValue: defaultConfig.BreakGeodeSpeed),
                    get: config => config.BreakGeodeSpeed,
                    set: (config, value) => config.BreakGeodeSpeed = value,
                    min: minSpeed,
                    max: maxSpeed
                )
                .AddNumberField(
                    name: I18n.Config_Bus_Name,
                    tooltip: () => I18n.Config_Bus_Tooltip(defaultValue: defaultConfig.PamBusSpeed),
                    get: config => config.PamBusSpeed,
                    set: (config, value) => config.PamBusSpeed = value,
                    min: minSpeed,
                    max: maxSpeed
                )
                .AddNumberField(
                    name: I18n.Config_CasinoSlots_Name,
                    tooltip: () => I18n.Config_CasinoSlots_Tooltip(defaultValue: defaultConfig.CasinoSlotsSpeed),
                    get: config => config.CasinoSlotsSpeed,
                    set: (config, value) => config.CasinoSlotsSpeed = value,
                    min: minSpeed,
                    max: maxSpeed
                )
                .AddNumberField(
                    name: I18n.Config_Forge_Name,
                    tooltip: () => I18n.Config_Forge_Tooltip(defaultValue: defaultConfig.ForgeSpeed),
                    get: config => config.ForgeSpeed,
                    set: (config, value) => config.ForgeSpeed = value,
                    min: minSpeed,
                    max: maxSpeed
                )
                .AddNumberField(
                    name: I18n.Config_PrizeTicketMachine_Name,
                    tooltip: () => I18n.Config_PrizeTicketMachine_Tooltip(defaultValue: defaultConfig.PrizeTicketMachineSpeed),
                    get: config => config.PrizeTicketMachineSpeed,
                    (config, value) => config.PrizeTicketMachineSpeed = value,
                    minSpeed,
                    maxSpeed
                )
                .AddNumberField(
                    name: I18n.Config_TreeFall_Name,
                    tooltip: () => I18n.Config_TreeFall_Tooltip(defaultValue: defaultConfig.TreeFallSpeed, suggestedValue: 3),
                    get: config => config.TreeFallSpeed,
                    set: (config, value) => config.TreeFallSpeed = value,
                    min: minSpeed,
                    max: maxSpeed
                )
                .AddNumberField(
                    name: I18n.Config_WheelSpin_Name,
                    tooltip: () => I18n.Config_WheelSpin_Tooltip(defaultValue: defaultConfig.WheelSpinSpeed),
                    get: config => config.WheelSpinSpeed,
                    set: (config, value) => config.WheelSpinSpeed = value,
                    min: minSpeed,
                    max: maxSpeed
                )
                .AddNumberField(
                    name: I18n.Config_ChestOpen_Name,
                    tooltip: () => I18n.Config_ChestOpen_Tooltip(defaultValue: defaultConfig.OpenChestSpeed),
                    get: config => config.OpenChestSpeed,
                    set: (config, value) => config.OpenChestSpeed = value,
                    min: minSpeed,
                    max: maxSpeed
                )

                .AddSectionTitle(I18n.Config_UiSpeeds)
                .AddNumberField(
                    name: I18n.Config_TitleMenu_Name,
                    tooltip: () => I18n.Config_TitleMenu_Tooltip(defaultValue: defaultConfig.TitleMenuTransitionSpeed),
                    get: config => config.TitleMenuTransitionSpeed,
                    set: (config, value) => config.TitleMenuTransitionSpeed = value,
                    min: minSpeed,
                    max: maxSpeed
                )
                .AddNumberField(
                    name: I18n.Config_LoadGameBlink_Name,
                    tooltip: () => I18n.Config_LoadGameBlink_Tooltip(defaultValue: defaultConfig.LoadGameBlinkSpeed),
                    get: config => config.LoadGameBlinkSpeed,
                    set: (config, value) => config.LoadGameBlinkSpeed = value,
                    min: minSpeed,
                    max: maxSpeed
                );
        }
    }
}
