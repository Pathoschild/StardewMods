using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ContentPatcher.Framework.ConfigModels;
using Pathoschild.Stardew.Common.Integrations.GenericModConfigMenu;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework
{
    /// <summary>Registers the mod configuration for a content pack with Generic Mod Config Menu.</summary>
    internal class GenericModConfigMenuIntegrationForContentPack
    {
        /*********
        ** Fields
        *********/
        /// <summary>The content pack whose config is being managed.</summary>
        private readonly IContentPack ContentPack;

        /// <summary>The config model.</summary>
        private readonly InvariantDictionary<ConfigField> Config;

        /// <summary>The Generic Mod Config Menu integration.</summary>
        private readonly GenericModConfigMenuIntegration<InvariantDictionary<ConfigField>> ConfigMenu;

        /// <summary>Parse a comma-delimited set of case-insensitive condition values.</summary>
        private readonly Func<string, InvariantHashSet> ParseCommaDelimitedField;


        /*********
        ** Accessors
        *********/
        /// <summary>Whether Generic Mod Config Menu is available.</summary>
        public bool IsLoaded => this.ConfigMenu.IsLoaded;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="contentPack">The content pack whose config is being managed.</param>
        /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="manifest">The mod manifest.</param>
        /// <param name="parseCommaDelimitedField">The Generic Mod Config Menu integration.</param>
        /// <param name="config">The config model.</param>
        /// <param name="saveAndApply">Save and apply the current config model.</param>
        public GenericModConfigMenuIntegrationForContentPack(IContentPack contentPack, IModRegistry modRegistry, IMonitor monitor, IManifest manifest, Func<string, InvariantHashSet> parseCommaDelimitedField, InvariantDictionary<ConfigField> config, Action saveAndApply)
        {
            this.ContentPack = contentPack;
            this.Config = config;
            this.ConfigMenu = new GenericModConfigMenuIntegration<InvariantDictionary<ConfigField>>(
                modRegistry: modRegistry,
                monitor: monitor,
                consumerManifest: manifest,
                getConfig: () => config,
                reset: () =>
                {
                    this.Reset();
                    saveAndApply();
                },
                saveAndApply
            );
            this.ParseCommaDelimitedField = parseCommaDelimitedField;
        }

        /// <summary>Register the config menu if available.</summary>
        public void Register()
        {
            if (!this.ConfigMenu.IsLoaded || !this.Config.Any())
                return;

            this.ConfigMenu.Register();

            // get fields by section
            InvariantDictionary<InvariantDictionary<ConfigField>> fieldsBySection = new() { [""] = new() };
            foreach (var (name, config) in this.Config)
            {
                string sectionId = config.Section?.Trim() ?? "";

                if (!fieldsBySection.TryGetValue(sectionId, out InvariantDictionary<ConfigField>? section))
                    fieldsBySection[sectionId] = section = new();

                section[name] = config;
            }

            // add section/field elements
            foreach ((string sectionId, InvariantDictionary<ConfigField> fields) in fieldsBySection)
            {
                if (!fields.Any())
                    continue;

                if (sectionId != "")
                    this.AddSection(sectionId);

                foreach ((string name, ConfigField config) in fields)
                    this.AddField(name, config);
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Register a config menu field with Generic Mod Config Menu.</summary>
        /// <param name="name">The config field name.</param>
        /// <param name="field">The config field instance.</param>
        private void AddField(string name, ConfigField field)
        {
            if (!this.ConfigMenu.IsLoaded)
                return;

            // get translation logic
            string GetName() => this.TryTranslate($"config.{name}.name", name);
            string GetDescription() => this.TryTranslate($"config.{name}.description", field.Description);
            string GetValueText(string value) => this.TryTranslate($"config.{name}.values.{value}", value);

            // textbox if any values allowed
            if (!field.AllowValues.Any())
            {
                this.ConfigMenu.AddTextbox(
                    name: GetName,
                    tooltip: GetDescription,
                    get: _ => string.Join(", ", field.Value.ToArray()),
                    set: (_, newValue) =>
                    {
                        InvariantHashSet values = this.ParseCommaDelimitedField(newValue);

                        field.Value.ReplaceWith(field.AllowMultiple
                            ? values
                            : values.Take(1)
                        );
                    }
                );
            }

            // checkboxes if player can choose multiple values
            else if (field.AllowMultiple)
            {
                foreach (string value in field.AllowValues)
                {
                    this.ConfigMenu.AddCheckbox(
                        name: () => $"{GetName()}: {GetValueText(value)}",
                        tooltip: GetDescription,
                        get: _ => field.Value.Contains(value),
                        set: (_, selected) =>
                        {
                            // toggle value
                            if (selected)
                                field.Value.Add(value);
                            else
                                field.Value.Remove(value);

                            // set default if blank
                            if (!field.AllowBlank && !field.Value.Any())
                                field.Value.ReplaceWith(new InvariantHashSet(field.DefaultValues));
                        }
                    );
                }
            }

            // checkbox for single boolean
            else if (!field.AllowBlank && field.IsBoolean())
            {
                this.ConfigMenu.AddCheckbox(
                    name: GetName,
                    tooltip: GetDescription,
                    get: _ => field.Value.Contains(true.ToString()),
                    set: (_, selected) =>
                    {
                        field.Value.Clear();
                        field.Value.Add(selected.ToString().ToLower());
                    }
                );
            }

            // slider for single numeric range
            else if (!field.AllowBlank && field.IsNumericRange(out int min, out int max))
            {
                if (!int.TryParse(field.DefaultValues.FirstOrDefault(), out int defaultValue))
                    defaultValue = min;

                // number slider
                this.ConfigMenu.AddNumberField(
                    name: GetName,
                    tooltip: GetDescription,
                    get: _ => int.TryParse(field.Value.FirstOrDefault(), out int val) ? val : defaultValue,
                    set: (_, val) => field.Value.ReplaceWith(new[] { val.ToString(CultureInfo.InvariantCulture) }),
                    min: min,
                    max: max
                );
            }

            // dropdown for single multiple-choice value
            else
            {
                List<string> choices = new List<string>(field.AllowValues);
                if (field.AllowBlank)
                    choices.Insert(0, "");

                this.ConfigMenu.AddDropdown(
                    name: GetName,
                    tooltip: GetDescription,
                    get: _ => field.Value.FirstOrDefault() ?? "",
                    set: (_, newValue) => field.Value.ReplaceWith(new[] { newValue }),
                    allowedValues: choices.ToArray(),
                    formatAllowedValue: GetValueText
                );
            }
        }

        /// <summary>Register a config menu section with Generic Mod Config Menu.</summary>
        /// <param name="name">The config section name.</param>
        private void AddSection(string name)
        {
            this.ConfigMenu.AddSectionTitle(
                text: () => this.TryTranslate($"config.section.{name}.name", name),
                tooltip: () => this.TryTranslate($"config.section.{name}.description", null)
            );
        }

        /// <summary>Reset the mod configuration.</summary>
        private void Reset()
        {
            foreach (ConfigField configField in this.Config.Values)
                configField.Value.ReplaceWith(configField.DefaultValues);
        }

        /// <summary>Get a translation if it exists, else get the fallback text.</summary>
        /// <param name="key">The translation key to find.</param>
        /// <param name="fallback">The fallback text.</param>
        private string TryTranslate(string key, string? fallback)
        {
            string translation = this.ContentPack.Translation.Get(key).UsePlaceholder(false);

            return string.IsNullOrWhiteSpace(translation)
                ? (fallback ?? "")
                : translation;
        }
    }
}
