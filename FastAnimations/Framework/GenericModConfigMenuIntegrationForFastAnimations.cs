using System;
using Common.Integrations.GenericModConfigMenu;
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

        /// <summary>An API for fetching metadata about loaded mods.</summary>
        private readonly IModRegistry ModRegistry;


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
            this.ModRegistry = modRegistry;
            this.ConfigMenu = new GenericModConfigMenuIntegration<ModConfig>(modRegistry, monitor, manifest, getConfig, reset, saveAndApply);
        }

        /// <summary>Register the config menu if available.</summary>
        public void Register()
        {
            // get config menu
            var menu = this.ConfigMenu;
            if (!menu.IsLoaded)
                return;

            // register
            const int minSpeed = 1;
            const int maxSpeed = 20;
            menu
                .RegisterConfig()

                // main options
                .AddLabel("Choose how fast each animation runs. Each value is a multiple of the original speed, like 1 for normal speed or 2 for double speed.")

                .AddLabel("Player Animations")
                .AddCheckbox(
                    label: "Disable eat/drink prompt",
                    description: "Whether to skip the confirmation prompt asking if you really want to eat/drink something.",
                    get: config => config.DisableEatAndDrinkConfirmation,
                    set: (config, value) => config.DisableEatAndDrinkConfirmation = value
                )
                .AddNumberField(
                    label: "Eat/drink",
                    description: "How fast you eat and drink. Default 10.",
                    get: config => config.EatAndDrinkSpeed,
                    set: (config, value) => config.EatAndDrinkSpeed = value,
                    min: minSpeed,
                    max: maxSpeed
                )
                .AddNumberField(
                    label: "Fish",
                    description: "How fast you cast and reel when fishing (doesn't affect the minigame). Default 1, suggested 2.",
                    get: config => config.FishingSpeed,
                    set: (config, value) => config.FishingSpeed = value,
                    min: minSpeed,
                    max: maxSpeed
                )
                .AddNumberField(
                    label: "Harvest",
                    description: "How fast you harvest crops and forage by hand. Default 3.",
                    get: config => config.HarvestSpeed,
                    set: (config, value) => config.HarvestSpeed = value,
                    min: minSpeed,
                    max: maxSpeed
                )
                .AddNumberField(
                    label: "Milk",
                    description: "How fast you use the milk pail. Default 5.",
                    get: config => config.MilkSpeed,
                    set: (config, value) => config.MilkSpeed = value,
                    min: minSpeed,
                    max: maxSpeed
                )
                .AddNumberField(
                    label: "Mount/dismount",
                    description: "How fast you mount/dismount horses (including custom mounts like Tractor Mod). Default 2.",
                    get: config => config.MountOrDismountSpeed,
                    set: (config, value) => config.MountOrDismountSpeed = value,
                    min: minSpeed,
                    max: maxSpeed
                )
                .AddNumberField(
                    label: "Shear",
                    description: "How fast you use the shears. Default 5.",
                    get: config => config.ShearSpeed,
                    set: (config, value) => config.ShearSpeed = value,
                    min: minSpeed,
                    max: maxSpeed
                )
                .AddNumberField(
                    label: "Swing tool",
                    description: "How fast you swing your tools (except weapons & fishing rod). Default 1, suggested 2.",
                    get: config => config.ToolSwingSpeed,
                    set: (config, value) => config.ToolSwingSpeed = value,
                    min: minSpeed,
                    max: maxSpeed
                )
                .AddNumberField(
                    label: "Swing weapon",
                    description: "How fast you swing your weapons. Default 1, suggested 4.",
                    get: config => config.WeaponSwingSpeed,
                    set: (config, value) => config.WeaponSwingSpeed = value,
                    min: minSpeed,
                    max: maxSpeed
                )

                .AddLabel("World Animations")
                .AddNumberField(
                    label: "Break geodes",
                    description: "How fast the blacksmith breaks geodes for you. Default 20.",
                    get: config => config.BreakGeodeSpeed,
                    set: (config, value) => config.BreakGeodeSpeed = value,
                    min: minSpeed,
                    max: maxSpeed
                )
                .AddNumberField(
                    label: "Casino slots",
                    description: "How fast the casino slots turn. Default 8.",
                    get: config => config.CasinoSlotsSpeed,
                    set: (config, value) => config.CasinoSlotsSpeed = value,
                    min: minSpeed,
                    max: maxSpeed
                )
                .AddNumberField(
                    label: "Pam's bus",
                    description: "How fast Pam drives her bus to and from the desert. Default 6.",
                    get: config => config.PamBusSpeed,
                    set: (config, value) => config.PamBusSpeed = value,
                    min: minSpeed,
                    max: maxSpeed
                )
                .AddNumberField(
                    label: "Tree falling",
                    description: "How fast trees fall after you chop them down. Default 1, suggested 3.",
                    get: config => config.TreeFallSpeed,
                    set: (config, value) => config.TreeFallSpeed = value,
                    min: minSpeed,
                    max: maxSpeed
                )

                .AddLabel("UI Animations")
                .AddNumberField(
                    label: "Title menu transitions",
                    description: "How fast the title menu transitions between screens. Default 10.",
                    get: config => config.TitleMenuTransitionSpeed,
                    set: (config, value) => config.TitleMenuTransitionSpeed = value,
                    min: minSpeed,
                    max: maxSpeed
                )
                .AddNumberField(
                    label: "Load game blink",
                    description: "How fast the blinking-slot delay happens after you click a load-save slot. Default 2.",
                    get: config => config.LoadGameBlinkSpeed,
                    set: (config, value) => config.LoadGameBlinkSpeed = value,
                    min: minSpeed,
                    max: maxSpeed
                );
        }
    }
}
