using Pathoschild.Stardew.Common.Integrations.GenericModConfigMenu;
using StardewModdingAPI;

namespace Pathoschild.Stardew.FastAnimations.Framework;

/// <summary>Registers the mod configuration with Generic Mod Config Menu.</summary>
internal class GenericModConfigMenuIntegrationForFastAnimations : IGenericModConfigMenuIntegrationFor<ModConfig>
{
    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public void Register(GenericModConfigMenuIntegration<ModConfig> menu, IMonitor monitor)
    {
        var defaultConfig = new ModConfig();

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
                name: I18n.Config_HoldUpItem_Name,
                tooltip: () => I18n.Config_HoldUpItem_Tooltip(defaultValue: defaultConfig.HoldUpItemSpeed),
                get: config => config.HoldUpItemSpeed,
                set: (config, value) => config.HoldUpItemSpeed = value,
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
                name: I18n.Config_ReadBook_Name,
                tooltip: () => I18n.Config_ReadBook_Tooltip(defaultValue: defaultConfig.ReadBookSpeed),
                get: config => config.ReadBookSpeed,
                set: (config, value) => config.ReadBookSpeed = value,
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
                name: I18n.Config_UseTotem_Name,
                tooltip: () => I18n.Config_UseTotem_Tooltip(defaultValue: defaultConfig.UseTotemSpeed),
                get: config => config.UseTotemSpeed,
                set: (config, value) => config.UseTotemSpeed = value,
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
                name: I18n.Config_Fade_Name,
                tooltip: () => I18n.Config_Fade_Tooltip(defaultValue: defaultConfig.FadeSpeed),
                get: config => config.FadeSpeed,
                set: (config, value) => config.FadeSpeed = value,
                min: minSpeed,
                max: maxSpeed
            )
            .AddNumberField(
                name: I18n.Config_FishingText_Name,
                tooltip: () => I18n.Config_FishingText_Tooltip(defaultValue: defaultConfig.FishingTextSpeed),
                get: config => config.FishingTextSpeed,
                set: (config, value) => config.FishingTextSpeed = value,
                min: minSpeed,
                max: maxSpeed
            )
            .AddNumberField(
                name: I18n.Config_FishingTreasure_Name,
                tooltip: () => I18n.Config_FishingTreasure_Tooltip(defaultValue: defaultConfig.FishingTreasureSpeed),
                get: config => config.FishingTreasureSpeed,
                set: (config, value) => config.FishingTreasureSpeed = value,
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
                name: I18n.Config_OpenChest_Name,
                tooltip: () => I18n.Config_OpenChest_Tooltip(defaultValue: defaultConfig.OpenChestSpeed),
                get: config => config.OpenChestSpeed,
                set: (config, value) => config.OpenChestSpeed = value,
                min: minSpeed,
                max: maxSpeed
            )
            .AddNumberField(
                name: I18n.Config_ParrotExpress_Name,
                tooltip: () => I18n.Config_ParrotExpress_Tooltip(defaultValue: defaultConfig.ParrotExpressSpeed),
                get: config => config.ParrotExpressSpeed,
                set: (config, value) => config.ParrotExpressSpeed = value,
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
                name: I18n.Config_Tailor_Name,
                tooltip: () => I18n.Config_Tailor_Tooltip(defaultValue: defaultConfig.TailorSpeed),
                get: config => config.TailorSpeed,
                set: (config, value) => config.TailorSpeed = value,
                min: minSpeed,
                max: maxSpeed
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

            .AddSectionTitle(I18n.Config_UiSpeeds)
            .AddNumberField(
                name: I18n.Config_DialogueTyping_Name,
                tooltip: () => I18n.Config_DialogueTyping_Tooltip(defaultValue: defaultConfig.DialogueTypeSpeed),
                get: config => config.DialogueTypeSpeed,
                set: (config, value) => config.DialogueTypeSpeed = value,
                min: minSpeed,
                max: maxSpeed
            )
            .AddNumberField(
                name: I18n.Config_OpenDialogueBox_Name,
                tooltip: () => I18n.Config_OpenDialogueBox_Tooltip(defaultValue: defaultConfig.OpenDialogueBoxSpeed),
                get: config => config.OpenDialogueBoxSpeed,
                set: (config, value) => config.OpenDialogueBoxSpeed = value,
                min: minSpeed,
                max: maxSpeed
            )
            .AddNumberField(
                name: I18n.Config_ShippingMenu_Name,
                tooltip: () => I18n.Config_ShippingMenu_Tooltip(defaultValue: defaultConfig.ShippingMenuTransitionSpeed),
                get: config => config.ShippingMenuTransitionSpeed,
                set: (config, value) => config.ShippingMenuTransitionSpeed = value,
                min: minSpeed,
                max: maxSpeed
            )
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
            )

            .AddSectionTitle(I18n.Config_Experimental)
            .AddParagraph(I18n.Config_ExperimentalWarning)
            .AddNumberField(
                name: I18n.Config_Event_Name,
                tooltip: () => I18n.Config_Event_Tooltip(defaultValue: defaultConfig.Experimental_EventSpeed, suggestedValue: 4),
                get: config => config.Experimental_EventSpeed,
                set: (config, value) => config.Experimental_EventSpeed = value,
                min: minSpeed,
                max: maxSpeed
            );
    }
}
