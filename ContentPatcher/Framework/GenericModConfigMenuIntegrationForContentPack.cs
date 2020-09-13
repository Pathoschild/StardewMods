using System;
using System.Collections.Generic;
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
        public void Register(Action<string, ConfigField> addConfigToken)
        {
            if (!this.ConfigMenu.IsLoaded || !this.Config.Any())
                return;

            this.ConfigMenu.RegisterConfig();
            foreach (var pair in this.Config)
                this.AddField(pair.Key, pair.Value, resetToken: () => addConfigToken(pair.Key, pair.Value));
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Register a config menu field with Generic Mod Config Menu.</summary>
        /// <param name="name">The config field name.</param>
        /// <param name="field">The config field instance.</param>
        /// <param name="resetToken">Remove and re-register the config token.</param>
        private void AddField(string name, ConfigField field, Action resetToken)
        {
            if (!this.ConfigMenu.IsLoaded)
                return;

            if (field.AllowValues.Any())
            {
                if (field.AllowMultiple)
                {
                    // Whitelist + multiple options = fake with multiple checkboxes
                    foreach (string value in field.AllowValues)
                    {
                        this.ConfigMenu.AddCheckbox(
                            label: $"{name}.{value}",
                            description: field.Description,
                            get: config => field.Value.Contains(value),
                            set: (config, selected) =>
                            {
                                // toggle value
                                if (selected)
                                    field.Value.Add(value);
                                else
                                    field.Value.Remove(value);

                                // set default if blank
                                if (!field.AllowBlank && !field.Value.Any())
                                    field.Value = new InvariantHashSet(field.DefaultValues);

                                // update token
                                resetToken();
                            }
                        );
                    }
                }
                else if (field.IsBoolean() && !field.AllowBlank)
                {
                    // true/false only = checkbox
                    this.ConfigMenu.AddCheckbox(
                        label: name,
                        description: field.Description,
                        get: config => field.Value.Contains(true.ToString()),
                        set: (config, selected) =>
                        {
                            field.Value.Clear();
                            field.Value.Add(selected.ToString().ToLower());

                            resetToken();
                        }
                    );
                }
                else
                {
                    // Whitelist + single value = drop down
                    // Need an extra option when blank is allowed
                    List<string> choices = new List<string>(field.AllowValues);
                    if (field.AllowBlank)
                        choices.Insert(0, "");

                    this.ConfigMenu.AddDropdown(
                        label: name,
                        description: field.Description,
                        get: config => field.Value.FirstOrDefault() ?? "",
                        set: (config, newValue) =>
                        {
                            field.Value = new InvariantHashSet(newValue);
                            resetToken();
                        },
                        choices.ToArray()
                    );
                }
            }
            else
            {
                // No whitelist = text field
                this.ConfigMenu.AddTextbox(
                    label: name,
                    description: field.Description,
                    get: config => string.Join(", ", field.Value.ToArray()),
                    set: (config, newValue) =>
                    {
                        field.Value = this.ParseCommaDelimitedField(newValue);

                        if (!field.AllowMultiple && field.Value.Count > 1)
                            field.Value = new InvariantHashSet(field.Value.Take(1));

                        resetToken();
                    }
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
