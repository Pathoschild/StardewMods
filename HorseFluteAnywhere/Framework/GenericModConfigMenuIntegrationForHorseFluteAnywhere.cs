using System;
using System.Linq;
using Common.Integrations.GenericModConfigMenu;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

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
            // get config menu
            var menu = this.ConfigMenu;
            if (!menu.IsLoaded)
                return;

            // register
            menu
                .RegisterConfig()
                .AddCheckbox(
                    label: "Require Horse Flute",
                    description: "Whether you need the horse flute item in your inventory to summon a horse. Default false.",
                    get: config => config.RequireHorseFlute,
                    set: (config, value) => config.RequireHorseFlute = value
                )
                .AddKeyBinding(
                    label: "Summon Horse Button",
                    description: "The button to press which plays the flute and summons a horse. Default H.",
                    get: config => this.GetSingleButton(config.SummonHorseKey),
                    set: (config, value) => config.SummonHorseKey = KeybindList.ForSingle(value)
                );
        }

        /// <summary>Get the first button in a key binding, if any.</summary>
        /// <param name="keybindList">The key binding list.</param>
        private SButton GetSingleButton(KeybindList keybindList)
        {
            foreach (Keybind keybind in keybindList.Keybinds)
            {
                if (keybind.IsBound)
                    return keybind.Buttons.First();
            }

            return SButton.None;
        }
    }
}
