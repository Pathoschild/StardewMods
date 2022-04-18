#nullable disable

using System;
using Pathoschild.Stardew.Common.Integrations.GenericModConfigMenu;
using StardewModdingAPI;

namespace Pathoschild.Stardew.NoclipMode.Framework
{
    /// <summary>Registers the mod configuration with Generic Mod Config Menu.</summary>
    internal class GenericModConfigMenuIntegrationForNoclipMode
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
        public GenericModConfigMenuIntegrationForNoclipMode(IModRegistry modRegistry, IMonitor monitor, IManifest manifest, Func<ModConfig> getConfig, Action reset, Action saveAndApply)
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
                    name: I18n.Config_EnabledMessage_Name,
                    tooltip: I18n.Config_EnabledMessage_Desc,
                    get: config => config.ShowEnabledMessage,
                    set: (config, value) => config.ShowEnabledMessage = value
                )
                .AddCheckbox(
                    name: I18n.Config_DisabledMessage_Name,
                    tooltip: I18n.Config_DisabledMessage_Desc,
                    get: config => config.ShowDisabledMessage,
                    set: (config, value) => config.ShowDisabledMessage = value
                )

                .AddSectionTitle(I18n.Config_Title_Controls)
                .AddKeyBinding(
                    name: I18n.Config_ToggleKey_Name,
                    tooltip: I18n.Config_ToggleKey_Desc,
                    get: config => config.ToggleKey,
                    set: (config, value) => config.ToggleKey = value
                );
        }
    }
}
