using System;
using Pathoschild.Stardew.Common.Integrations.GenericModConfigMenu;
using StardewModdingAPI;

namespace Pathoschild.Stardew.DebugMode.Framework
{
    /// <summary>Registers the mod configuration with Generic Mod Config Menu.</summary>
    internal class GenericModConfigMenuIntegrationForDebugMode
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
        public GenericModConfigMenuIntegrationForDebugMode(IModRegistry modRegistry, IMonitor monitor, IManifest manifest, Func<ModConfig> getConfig, Action reset, Action saveAndApply)
        {
            this.ConfigMenu = new GenericModConfigMenuIntegration<ModConfig>(modRegistry, monitor, manifest, getConfig, reset, saveAndApply);
        }

        /// <summary>Register the config menu if available.</summary>
        public void Register()
        {
            var menu = this.ConfigMenu;
            if (!menu.IsLoaded)
                return;

            menu.Register();
            menu
                .AddSectionTitle(I18n.Config_Title_GeneralOptions)
                .AddCheckbox(
                    name: I18n.Config_EnableGameDebug_Name,
                    tooltip: I18n.Config_EnableGameDebug_Desc,
                    get: config => config.AllowGameDebug,
                    set: (config, value) => config.AllowGameDebug = value
                )
                .AddCheckbox(
                    name: I18n.Config_EnableDangerousHotkeys_Name,
                    tooltip: I18n.Config_EnableDangerousHotkeys_Desc,
                    get: config => config.AllowDangerousCommands,
                    set: (config, value) => config.AllowDangerousCommands = value
                )

                .AddSectionTitle(I18n.Config_Title_Controls)
                .AddKeyBinding(
                    name: I18n.Config_ToggleDebugKey_Name,
                    tooltip: I18n.Config_ToggleDebugKey_Desc,
                    get: config => config.Controls.ToggleDebug,
                    set: (config, value) => config.Controls.ToggleDebug = value
                );
        }
    }
}
