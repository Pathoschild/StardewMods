using System;
using Pathoschild.Stardew.Common.Integrations.GenericModConfigMenu;
using StardewModdingAPI;

namespace Pathoschild.Stardew.SkipIntro.Framework
{
    /// <summary>Registers the mod configuration with Generic Mod Config Menu.</summary>
    internal class GenericModConfigMenuIntegrationForSkipIntro
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
        public GenericModConfigMenuIntegrationForSkipIntro(IModRegistry modRegistry, IMonitor monitor, IManifest manifest, Func<ModConfig> getConfig, Action reset, Action saveAndApply)
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
                .Register()
                .AddDropdown(
                    name: I18n.Config_SkipTo_Name,
                    tooltip: I18n.Config_SkipTo_Tooltip,
                    allowedValues: Enum.GetNames(typeof(Screen)),
                    formatAllowedValue: this.TranslateScreen,
                    get: config => config.SkipTo.ToString(),
                    set: (config, value) => config.SkipTo = (Screen)Enum.Parse(typeof(Screen), value)
                );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the translated text for a screen value.</summary>
        /// <param name="rawScreen">The raw screen value.</param>
        private string TranslateScreen(string rawScreen)
        {
            if (!Enum.TryParse(rawScreen, out Screen screen))
                return rawScreen;

            return screen switch
            {
                Screen.Title => I18n.Config_SkipTo_Values_TitleMenu(),
                Screen.Load => I18n.Config_SkipTo_Values_LoadMenu(),
                Screen.JoinCoop => I18n.Config_SkipTo_Values_JoinCoop(),
                Screen.HostCoop => I18n.Config_SkipTo_Values_HostCoop(),
                _ => screen.ToString()
            };
        }
    }
}
