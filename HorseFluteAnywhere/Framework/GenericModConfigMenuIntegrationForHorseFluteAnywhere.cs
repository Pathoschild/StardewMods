using System;
using System.Linq;
using Common.Integrations.GenericModConfigMenu;
using Pathoschild.Stardew.Common.Input;
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

        /// <summary>Get the summon key binding.</summary>
        private readonly Func<KeyBinding> GetSummonKey;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="manifest">The mod manifest.</param>
        /// <param name="getConfig">Get the current config model.</param>
        /// <param name="getSummonKey">Get the parsed key bindings.</param>
        /// <param name="reset">Reset the config model to the default values.</param>
        /// <param name="saveAndApply">Save and apply the current config model.</param>
        public GenericModConfigMenuIntegrationForHorseFluteAnywhere(IModRegistry modRegistry, IMonitor monitor, IManifest manifest, Func<ModConfig> getConfig, Func<KeyBinding> getSummonKey, Action reset, Action saveAndApply)
        {
            this.GetSummonKey = getSummonKey;
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
                    get: _ => this.GetSingleButton(this.GetSummonKey()),
                    set: (config, value) => config.SummonHorseKey = value.ToString()
                );
        }

        /// <summary>Get the first button in a key binding, if any.</summary>
        /// <param name="binding">The key binding.</param>
        private SButton GetSingleButton(KeyBinding binding)
        {
            SButton[] set = binding.ButtonSets.FirstOrDefault();
            return set?.FirstOrDefault() ?? SButton.None;
        }
    }
}
