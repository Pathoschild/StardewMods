using System;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace Pathoschild.Stardew.Common.Integrations.GenericModConfigMenu
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
        /// <param name="reset">Reset the mod's config to its default values.</param>
        /// <param name="saveAndApply">Save the mod's current config to the <c>config.json</c> file.</param>
        public GenericModConfigMenuIntegration(IModRegistry modRegistry, IMonitor monitor, IManifest consumerManifest, Func<TConfig> getConfig, Action reset, Action saveAndApply)
            : base("Generic Mod Config Menu", "spacechase0.GenericModConfigMenu", "1.5.1", modRegistry, monitor)
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
        /// <param name="titleScreenOnly">Whether the options can only be edited from the title screen.</param>
        public GenericModConfigMenuIntegration<TConfig> Register(bool titleScreenOnly = false)
        {
            this.AssertLoaded();

            this.ModApi.Register(this.ConsumerManifest, this.Reset, this.SaveAndApply, titleScreenOnly);

            return this;
        }

        /// <summary>Add a section title at the current position in the form.</summary>
        /// <param name="text">The title text shown in the form.</param>
        /// <param name="tooltip">The tooltip text shown when the cursor hovers on the title, or <c>null</c> to disable the tooltip.</param>
        public GenericModConfigMenuIntegration<TConfig> AddSectionTitle(Func<string> text, Func<string> tooltip = null)
        {
            this.AssertLoaded();

            this.ModApi.AddSectionTitle(this.ConsumerManifest, text, tooltip);

            return this;
        }

        /// <summary>Add a checkbox to the form.</summary>
        /// <param name="name">The label text to show in the form.</param>
        /// <param name="tooltip">The tooltip text shown when the cursor hovers on the field.</param>
        /// <param name="get">Get the current value from the mod config.</param>
        /// <param name="set">Set a new value in the mod config.</param>
        /// <param name="enable">Whether the field is enabled.</param>
        public GenericModConfigMenuIntegration<TConfig> AddCheckbox(Func<string> name, Func<string> tooltip, Func<TConfig, bool> get, Action<TConfig, bool> set, bool enable = true)
        {
            this.AssertLoaded();

            if (enable)
            {
                this.ModApi.AddBoolOption(
                    mod: this.ConsumerManifest,
                    name: name,
                    tooltip: tooltip,
                    getValue: () => get(this.GetConfig()),
                    setValue: val => set(this.GetConfig(), val)
                );
            }

            return this;
        }

        /// <summary>Add a dropdown to the form.</summary>
        /// <param name="name">The label text to show in the form.</param>
        /// <param name="tooltip">The tooltip text shown when the cursor hovers on the field.</param>
        /// <param name="get">Get the current value from the mod config.</param>
        /// <param name="set">Set a new value in the mod config.</param>
        /// <param name="allowedValues">The values that can be selected.</param>
        /// <param name="formatAllowedValue">Get the display text to show for a value from <paramref name="allowedValues"/>, or <c>null</c> to show the values as-is.</param>
        /// <param name="enable">Whether the field is enabled.</param>
        public GenericModConfigMenuIntegration<TConfig> AddDropdown(Func<string> name, Func<string> tooltip, Func<TConfig, string> get, Action<TConfig, string> set, string[] allowedValues, Func<string, string> formatAllowedValue, bool enable = true)
        {
            this.AssertLoaded();

            if (enable)
            {
                this.ModApi.AddTextOption(
                    mod: this.ConsumerManifest,
                    name: name,
                    tooltip: tooltip,
                    getValue: () => get(this.GetConfig()),
                    setValue: val => set(this.GetConfig(), val),
                    allowedValues: allowedValues,
                    formatAllowedValue: formatAllowedValue
                );
            }

            return this;
        }

        /// <summary>Add a checkbox to the form.</summary>
        /// <param name="name">The label text to show in the form.</param>
        /// <param name="tooltip">The tooltip text shown when the cursor hovers on the field.</param>
        /// <param name="get">Get the current value from the mod config.</param>
        /// <param name="set">Set a new value in the mod config.</param>
        /// <param name="enable">Whether the field is enabled.</param>
        public GenericModConfigMenuIntegration<TConfig> AddTextbox(Func<string> name, Func<string> tooltip, Func<TConfig, string> get, Action<TConfig, string> set, bool enable = true)
        {
            this.AssertLoaded();

            if (enable)
            {
                this.ModApi.AddTextOption(
                    mod: this.ConsumerManifest,
                    name: name,
                    tooltip: tooltip,
                    getValue: () => get(this.GetConfig()),
                    setValue: val => set(this.GetConfig(), val)
                );
            }

            return this;
        }

        /// <summary>Add a numeric field to the form.</summary>
        /// <param name="name">The label text to show in the form.</param>
        /// <param name="tooltip">The tooltip text shown when the cursor hovers on the field.</param>
        /// <param name="get">Get the current value from the mod config.</param>
        /// <param name="set">Set a new value in the mod config.</param>
        /// <param name="min">The minimum allowed value.</param>
        /// <param name="max">The maximum allowed value.</param>
        /// <param name="enable">Whether the field is enabled.</param>
        public GenericModConfigMenuIntegration<TConfig> AddNumberField(Func<string> name, Func<string> tooltip, Func<TConfig, int> get, Action<TConfig, int> set, int min, int max, bool enable = true)
        {
            this.AssertLoaded();

            if (enable)
            {
                this.ModApi.AddNumberOption(
                    mod: this.ConsumerManifest,
                    name: name,
                    tooltip: tooltip,
                    getValue: () => get(this.GetConfig()),
                    setValue: val => set(this.GetConfig(), val),
                    min: min,
                    max: max
                );
            }

            return this;
        }

        /// <summary>Add a numeric field to the form.</summary>
        /// <param name="name">The label text to show in the form.</param>
        /// <param name="tooltip">The tooltip text shown when the cursor hovers on the field.</param>
        /// <param name="get">Get the current value from the mod config.</param>
        /// <param name="set">Set a new value in the mod config.</param>
        /// <param name="min">The minimum allowed value.</param>
        /// <param name="max">The maximum allowed value.</param>
        /// <param name="enable">Whether the field is enabled.</param>
        /// <param name="interval">The interval of values that can be selected.</param>
        public GenericModConfigMenuIntegration<TConfig> AddNumberField(Func<string> name, Func<string> tooltip, Func<TConfig, float> get, Action<TConfig, float> set, float min, float max, bool enable = true, float interval = 0.1f)
        {
            this.AssertLoaded();

            if (enable)
            {
                this.ModApi.AddNumberOption(
                    mod: this.ConsumerManifest,
                    name: name,
                    tooltip: tooltip,
                    getValue: () => get(this.GetConfig()),
                    setValue: val => set(this.GetConfig(), val),
                    min: min,
                    max: max,
                    interval: interval
                );
            }

            return this;
        }

        /// <summary>Add a key binding field to the form.</summary>
        /// <param name="name">The label text to show in the form.</param>
        /// <param name="tooltip">The tooltip text shown when the cursor hovers on the field.</param>
        /// <param name="get">Get the current value from the mod config.</param>
        /// <param name="set">Set a new value in the mod config.</param>
        /// <param name="enable">Whether the field is enabled.</param>
        public GenericModConfigMenuIntegration<TConfig> AddKeyBinding(Func<string> name, Func<string> tooltip, Func<TConfig, KeybindList> get, Action<TConfig, KeybindList> set, bool enable = true)
        {
            this.AssertLoaded();

            if (enable)
            {
                this.ModApi.AddKeybindList(
                    mod: this.ConsumerManifest,
                    name: name,
                    tooltip: tooltip,
                    getValue: () => get(this.GetConfig()),
                    setValue: val => set(this.GetConfig(), val)
                );
            }

            return this;
        }
    }
}
