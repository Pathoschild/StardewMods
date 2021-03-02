using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Common.Integrations.GenericModConfigMenu;
using ContentPatcher.Framework.ConfigModels;
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
        /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="manifest">The mod manifest.</param>
        /// <param name="parseCommaDelimitedField">The Generic Mod Config Menu integration.</param>
        /// <param name="config">The config model.</param>
        /// <param name="saveAndApply">Save and apply the current config model.</param>
        public GenericModConfigMenuIntegrationForContentPack(IModRegistry modRegistry, IMonitor monitor, IManifest manifest, Func<string, InvariantHashSet> parseCommaDelimitedField, InvariantDictionary<ConfigField> config, Action saveAndApply)
        {
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

            this.ConfigMenu.RegisterConfig();
            foreach (var pair in this.Config)
                this.AddField(pair.Key, pair.Value);
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

            // textbox if any values allowed
            if (!field.AllowValues.Any())
            {
                this.ConfigMenu.AddTextbox(
                    label: name,
                    description: field.Description,
                    get: _ => string.Join(", ", field.Value.ToArray()),
                    set: (_, newValue) =>
                    {
                        field.Value = this.ParseCommaDelimitedField(newValue);

                        if (!field.AllowMultiple && field.Value.Count > 1)
                            field.Value = new InvariantHashSet(field.Value.Take(1));
                    }
                );
            }

            // checkboxes if player can choose multiple values
            else if (field.AllowMultiple)
            {
                foreach (string value in field.AllowValues)
                {
                    this.ConfigMenu.AddCheckbox(
                        label: $"{name}.{value}",
                        description: field.Description,
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
                                field.Value = new InvariantHashSet(field.DefaultValues);
                        }
                    );
                }
            }

            // checkbox for single boolean
            else if (!field.AllowBlank && field.IsBoolean())
            {
                this.ConfigMenu.AddCheckbox(
                    label: name,
                    description: field.Description,
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
                    label: name,
                    description: field.Description,
                    get: _ => int.TryParse(field.Value.FirstOrDefault(), out int val) ? val : defaultValue,
                    set: (_, val) => field.Value = new InvariantHashSet(val.ToString(CultureInfo.InvariantCulture)),
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
                    label: name,
                    description: field.Description,
                    get: _ => field.Value.FirstOrDefault() ?? "",
                    set: (_, newValue) => field.Value = new InvariantHashSet(newValue),
                    choices.ToArray()
                );
            }
        }

        /// <summary>Reset the mod configuration.</summary>
        private void Reset()
        {
            foreach (ConfigField configField in this.Config.Values)
                configField.Value = new InvariantHashSet(configField.DefaultValues);
        }
    }
}
