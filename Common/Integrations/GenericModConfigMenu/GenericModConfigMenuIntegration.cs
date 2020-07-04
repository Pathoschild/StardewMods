using System;
using Pathoschild.Stardew.Common.Integrations;
using StardewModdingAPI;

namespace Common.Integrations.GenericModConfigMenu
{
    /// <summary>Handles the logic for integrating with the Generic Mod Configuration Menu mod.</summary>
    /// <typeparam name="TConfig">The mod configuration type.</typeparam>
    internal class GenericModConfigMenuIntegration<TConfig> : BaseIntegration
        where TConfig : new()
    {
        /*********
        ** Fields
        *********/
        /// <summary>The mod's public API.</summary>
        private readonly IGenericModConfigMenuApi ModApi;

        /// <summary>The manifest for the mod consuming the API.</summary>
        private readonly IManifest ConsumerManifest;

        /// <summary>Get the current config model.</summary>
        private readonly Func<TConfig> GetConfig;

        /// <summary>Reset the config model to the default values.</summary>
        private readonly Action Reset;

        /// <summary>Save and apply the current config model.</summary>
        private readonly Action SaveAndApply;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="consumerManifest">The manifest for the mod consuming the API.</param>
        /// <param name="getConfig">Get the current config model.</param>
        /// <param name="reset">Reset the config model to the default values.</param>
        /// <param name="saveAndApply">Save and apply the current config model.</param>
        public GenericModConfigMenuIntegration(IModRegistry modRegistry, IMonitor monitor, IManifest consumerManifest, Func<TConfig> getConfig, Action reset, Action saveAndApply)
            : base("Generic Mod Config Menu", "spacechase0.GenericModConfigMenu", "1.1.0", modRegistry, monitor)
        {
            // init
            this.ConsumerManifest = consumerManifest;
            this.GetConfig = getConfig;
            this.Reset = reset;
            this.SaveAndApply = saveAndApply;

            // get mod API
            if (this.IsLoaded)
            {
                this.ModApi = this.GetValidatedApi<IGenericModConfigMenuApi>();
                this.IsLoaded = this.ModApi != null;
            }
        }

        /// <summary>Register the mod config.</summary>
        public GenericModConfigMenuIntegration<TConfig> RegisterConfig()
        {
            this.AssertLoaded();

            this.ModApi.RegisterModConfig(this.ConsumerManifest, this.Reset, this.SaveAndApply);

            return this;
        }

        /// <summary>Add a label to the form.</summary>
        /// <param name="label">The label text.</param>
        /// <param name="description">A description shown on hover, if any.</param>
        public GenericModConfigMenuIntegration<TConfig> AddLabel(string label, string description = null)
        {
            this.AssertLoaded();

            this.ModApi.RegisterLabel(this.ConsumerManifest, label, description);

            return this;
        }

        /// <summary>Add a checkbox to the form.</summary>
        /// <param name="label">The label text.</param>
        /// <param name="description">A description shown on hover, if any.</param>
        /// <param name="get">Get the current value.</param>
        /// <param name="set">Set a new value.</param>
        /// <param name="enable">Whether the field is enabled.</param>
        public GenericModConfigMenuIntegration<TConfig> AddCheckbox(string label, string description, Func<TConfig, bool> get, Action<TConfig, bool> set, bool enable = true)
        {
            this.AssertLoaded();

            if (enable)
            {
                this.ModApi.RegisterSimpleOption(
                    mod: this.ConsumerManifest,
                    optionName: label,
                    optionDesc: description,
                    optionGet: () => get(this.GetConfig()),
                    optionSet: val => set(this.GetConfig(), val)
                );
            }

            return this;
        }

        /// <summary>Add a checkbox to the form.</summary>
        /// <param name="label">The label text.</param>
        /// <param name="description">A description shown on hover, if any.</param>
        /// <param name="get">Get the current value.</param>
        /// <param name="set">Set a new value.</param>
        /// <param name="enable">Whether the field is enabled.</param>
        public GenericModConfigMenuIntegration<TConfig> AddTextbox(string label, string description, Func<TConfig, string> get, Action<TConfig, string> set, bool enable = true)
        {
            this.AssertLoaded();

            if (enable)
            {
                this.ModApi.RegisterSimpleOption(
                    mod: this.ConsumerManifest,
                    optionName: label,
                    optionDesc: description,
                    optionGet: () => get(this.GetConfig()),
                    optionSet: val => set(this.GetConfig(), val)
                );
            }

            return this;
        }

        /// <summary>Add a numeric field to the form.</summary>
        /// <param name="label">The label text.</param>
        /// <param name="description">A description shown on hover, if any.</param>
        /// <param name="get">Get the current value.</param>
        /// <param name="set">Set a new value.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="enable">Whether the field is enabled.</param>
        public GenericModConfigMenuIntegration<TConfig> AddNumberField(string label, string description, Func<TConfig, int> get, Action<TConfig, int> set, int min, int max, bool enable = true)
        {
            this.AssertLoaded();

            if (enable)
            {
                this.ModApi.RegisterClampedOption(
                    mod: this.ConsumerManifest,
                    optionName: label,
                    optionDesc: description,
                    optionGet: () => get(this.GetConfig()),
                    optionSet: val => set(this.GetConfig(), val),
                    min: min,
                    max: max
                );
            }

            return this;
        }

        /// <summary>Add a numeric field to the form.</summary>
        /// <param name="label">The label text.</param>
        /// <param name="description">A description shown on hover, if any.</param>
        /// <param name="get">Get the current value.</param>
        /// <param name="set">Set a new value.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="enable">Whether the field is enabled.</param>
        public GenericModConfigMenuIntegration<TConfig> AddNumberField(string label, string description, Func<TConfig, float> get, Action<TConfig, float> set, float min, float max, bool enable = true)
        {
            this.AssertLoaded();

            if (enable)
            {
                this.ModApi.RegisterClampedOption(
                    mod: this.ConsumerManifest,
                    optionName: label,
                    optionDesc: description,
                    optionGet: () => get(this.GetConfig()),
                    optionSet: val => set(this.GetConfig(), val),
                    min: min,
                    max: max
                );
            }

            return this;
        }

        /// <summary>Add a key binding field to the form.</summary>
        /// <param name="label">The label text.</param>
        /// <param name="description">A description shown on hover, if any.</param>
        /// <param name="get">Get the current value.</param>
        /// <param name="set">Set a new value.</param>
        /// <param name="enable">Whether the field is enabled.</param>
        public GenericModConfigMenuIntegration<TConfig> AddKeyBinding(string label, string description, Func<TConfig, SButton> get, Action<TConfig, SButton> set, bool enable = true)
        {
            this.AssertLoaded();

            if (enable)
            {
                this.ModApi.RegisterSimpleOption(
                    mod: this.ConsumerManifest,
                    optionName: label,
                    optionDesc: description,
                    optionGet: () => get(this.GetConfig()),
                    optionSet: val => set(this.GetConfig(), val)
                );
            }

            return this;
        }
    }
}
