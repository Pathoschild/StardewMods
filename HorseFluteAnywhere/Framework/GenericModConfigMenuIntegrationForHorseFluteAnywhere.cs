using System;
using Pathoschild.Stardew.Common.Integrations.GenericModConfigMenu;
using StardewModdingAPI;

namespace Pathoschild.Stardew.HorseFluteAnywhere.Framework
{
    /// <summary>Registers the mod configuration with Generic Mod Config Menu.</summary>
    internal class GenericModConfigMenuIntegrationForHorseFluteAnywhere
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
        public GenericModConfigMenuIntegrationForHorseFluteAnywhere(IModRegistry modRegistry, IMonitor monitor, IManifest manifest, Func<ModConfig> getConfig, Action reset, Action saveAndApply)
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
            menu
                .Register()
                .AddCheckbox(
                    name: I18n.Config_RequireFlute_Name,
                    tooltip: I18n.Config_RequireFlute_Description,
                    get: config => config.RequireHorseFlute,
                    set: (config, value) => config.RequireHorseFlute = value
                )
                .AddKeyBinding(
                    name: I18n.Config_SummonHorseButton_Name,
                    tooltip: () => I18n.Config_SummonHorseButton_Description(defaultValue: defaultConfig.SummonHorseKey.ToString()),
                    get: config => config.SummonHorseKey,
                    set: (config, value) => config.SummonHorseKey = value
                );
        }
    }
}
