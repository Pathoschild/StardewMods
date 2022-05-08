using System;
using Pathoschild.Stardew.Common.Integrations.GenericModConfigMenu;
using Pathoschild.Stardew.SmallBeachFarm.Framework.Config;
using StardewModdingAPI;

namespace Pathoschild.Stardew.SmallBeachFarm.Framework
{
    /// <summary>Registers the mod configuration with Generic Mod Config Menu.</summary>
    internal class GenericModConfigMenuIntegrationForSmallBeachFarm
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
        public GenericModConfigMenuIntegrationForSmallBeachFarm(IModRegistry modRegistry, IMonitor monitor, IManifest manifest, Func<ModConfig> getConfig, Action reset, Action saveAndApply)
        {
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
            menu
                .Register(titleScreenOnly: true) // configuring in-game would have unintended effects like small beach farm logic being half-applied

                .AddCheckbox(
                    name: I18n.Config_Campfire_Name,
                    tooltip: I18n.Config_Campfire_Tooltip,
                    get: config => config.AddCampfire,
                    set: (config, value) => config.AddCampfire = value
                )
                .AddCheckbox(
                    name: I18n.Config_Islands_Name,
                    tooltip: I18n.Config_Islands_Tooltip,
                    get: config => config.EnableIslands,
                    set: (config, value) => config.EnableIslands = value
                )
                .AddCheckbox(
                    name: I18n.Config_BeachSounds_Name,
                    tooltip: I18n.Config_BeachSounds_Tooltip,
                    get: config => config.UseBeachMusic,
                    set: (config, value) => config.UseBeachMusic = value
                )
                .AddCheckbox(
                    name: I18n.Config_ShippingBinPath_Name,
                    tooltip: I18n.Config_ShippingBinPath_Tooltip,
                    get: config => config.ShippingBinPath,
                    set: (config, value) => config.ShippingBinPath = value
                );
        }
    }
}
