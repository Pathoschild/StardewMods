using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework;

/// <summary>Handles reading, normalizing, and saving the configuration for a content pack.</summary>
internal class ConfigFileHandler
{
    /*********
    ** Fields
    *********/
    /// <summary>The name of the config file.</summary>
    private readonly string Filename;

    /// <summary>Parse a comma-delimited set of case-insensitive condition values.</summary>
    private readonly Func<string?, IInvariantSet> ParseCommaDelimitedField;

    /// <summary>A callback to invoke when a validation warning occurs. This is passed the content pack, label, and reason phrase respectively.</summary>
    private readonly Action<IContentPack, string, string> LogWarning;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="filename">The name of the config file.</param>
    /// <param name="parseCommandDelimitedField">Parse a comma-delimited set of case-insensitive condition values.</param>
    /// <param name="logWarning">A callback to invoke when a validation warning occurs. This is passed the content pack, label, and reason phrase respectively.</param>
    public ConfigFileHandler(string filename, Func<string?, IInvariantSet> parseCommandDelimitedField, Action<IContentPack, string, string> logWarning)
    {
        this.Filename = filename;
        this.ParseCommaDelimitedField = parseCommandDelimitedField;
        this.LogWarning = logWarning;
    }

    /// <summary>Read the configuration file for a content pack.</summary>
    /// <param name="contentPack">The content pack.</param>
    /// <param name="rawSchema">The raw config schema from the mod's <c>content.json</c>.</param>
    /// <param name="formatVersion">The content format version.</param>
    public InvariantDictionary<ConfigField> Read(IContentPack contentPack, InvariantDictionary<ConfigSchemaFieldConfig?>? rawSchema, ISemanticVersion formatVersion)
    {
        InvariantDictionary<ConfigField> config = this.LoadConfigSchema(rawSchema, logWarning: (field, reason) => this.LogWarning(contentPack, $"{nameof(ContentConfig.ConfigSchema)} field '{field}'", reason), formatVersion);
        this.LoadConfigValues(contentPack, config, logWarning: (field, reason) => this.LogWarning(contentPack, $"{this.Filename} > {field}", reason));
        return config;
    }

    /// <summary>Save the configuration file for a content pack.</summary>
    /// <param name="contentPack">The content pack.</param>
    /// <param name="config">The configuration to save.</param>
    /// <param name="modHelper">The mod helper through which to save the file.</param>
    public void Save(IContentPack contentPack, InvariantDictionary<ConfigField> config, IModHelper modHelper)
    {
        // save if settings valid
        if (config.Any())
        {
            InvariantDictionary<string> data = new(config.ToDictionary(p => p.Key, p => string.Join(", ", p.Value.Value)));
            contentPack.WriteJsonFile(this.Filename, data);
        }

        // delete if no settings
        else
        {
            FileInfo file = new(Path.Combine(contentPack.GetFullPath(this.Filename)));
            if (file.Exists)
                file.Delete();
        }
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Parse a raw config schema for a content pack.</summary>
    /// <param name="rawSchema">The raw config schema.</param>
    /// <param name="logWarning">The callback to invoke on each validation warning, passed the field name and reason respectively.</param>
    /// <param name="formatVersion">The content format version.</param>
    private InvariantDictionary<ConfigField> LoadConfigSchema(InvariantDictionary<ConfigSchemaFieldConfig?>? rawSchema, Action<string, string> logWarning, ISemanticVersion formatVersion)
    {
        InvariantDictionary<ConfigField> schema = new();
        if (rawSchema == null || !rawSchema.Any())
            return schema;

        foreach (string rawKey in rawSchema.Keys)
        {
            ConfigSchemaFieldConfig? field = rawSchema[rawKey];
            if (field is null)
                continue;

            // validate format
            if (string.IsNullOrWhiteSpace(rawKey))
            {
                logWarning(rawKey, "the config field name can't be empty.");
                continue;
            }
            if (rawKey.Contains(InternalConstants.PositionalInputArgSeparator) || rawKey.Contains(InternalConstants.NamedInputArgSeparator))
            {
                logWarning(rawKey, $"the name '{rawKey}' can't have input arguments ({InternalConstants.PositionalInputArgSeparator} or {InternalConstants.NamedInputArgSeparator} character).");
                continue;
            }

            // validate reserved keys
            if (Enum.TryParse<ConditionType>(rawKey, true, out _))
            {
                logWarning(rawKey, $"can't use {rawKey} as a config field, because it's a reserved condition key.");
                continue;
            }

            // read allowed/default values
            IInvariantSet allowValues = this.ParseCommaDelimitedField(field.AllowValues);
            IInvariantSet defaultValues = this.ParseCommaDelimitedField(field.Default);

            // pre-1.7 behaviour
            if (formatVersion.IsOlderThan("1.7"))
            {
                // allowed values are required
                if (!allowValues.Any())
                {
                    logWarning(rawKey, $"no {nameof(ConfigSchemaFieldConfig.AllowValues)} specified (and format version is less than 1.7).");
                    continue;
                }

                // inject default if needed
                if (!defaultValues.Any() && !field.AllowBlank)
                    defaultValues = InvariantSets.FromValue(allowValues.First());
            }

            // validate allowed values
            if (!field.AllowBlank && !defaultValues.Any())
            {
                logWarning(rawKey, $"if {nameof(field.AllowBlank)} is false, you must specify {nameof(field.Default)}.");
                continue;
            }
            if (allowValues.Any() && defaultValues.Any())
            {
                IInvariantSet invalidValues = defaultValues.GetWithout(allowValues);
                if (invalidValues.Any())
                {
                    logWarning(rawKey, $"default values '{string.Join(", ", invalidValues)}' are not allowed according to {nameof(ConfigSchemaFieldConfig.AllowValues)}.");
                    continue;
                }
            }

            // validate allow multiple
            if (!field.AllowMultiple && defaultValues.Count > 1)
            {
                logWarning(rawKey, $"can't have multiple default values because {nameof(ConfigSchemaFieldConfig.AllowMultiple)} is false.");
                continue;
            }

            // add to schema
            schema[rawKey] = new ConfigField(
                allowValues: allowValues,
                defaultValues: defaultValues,
                value: InvariantSets.Empty,
                allowBlank: field.AllowBlank,
                allowMultiple: field.AllowMultiple,
                description: field.Description,
                section: field.Section
            );
        }

        return schema;
    }

    /// <summary>Load config values from the content pack.</summary>
    /// <param name="contentPack">The content pack whose config file to read.</param>
    /// <param name="config">The config schema.</param>
    /// <param name="logWarning">The callback to invoke on each validation warning, passed the field name and reason respectively.</param>
    private void LoadConfigValues(IContentPack contentPack, InvariantDictionary<ConfigField> config, Action<string, string> logWarning)
    {
        if (!config.Any())
            return;

        // read raw config
        InvariantDictionary<IInvariantSet> configValues = new(
            from entry in (contentPack.ReadJsonFile<InvariantDictionary<string>>(this.Filename) ?? new())
            let key = entry.Key.Trim()
            let value = this.ParseCommaDelimitedField(entry.Value)
            select new KeyValuePair<string, IInvariantSet>(key, value)
        );

        // remove invalid values
        foreach (string key in configValues.Keys.ExceptIgnoreCase(config.Keys).ToArray())
        {
            logWarning(key, "no such field supported by this content pack.");
            configValues.Remove(key);
        }

        // inject default values
        foreach (string key in config.Keys)
        {
            ConfigField field = config[key];
            if (!configValues.TryGetValue(key, out IInvariantSet? values) || (!field.AllowBlank && !values.Any()))
                configValues[key] = field.DefaultValues;
        }

        // parse each field
        foreach (string key in config.Keys)
        {
            ConfigField field = config[key];

            // validate allow-multiple
            if (field is { AllowMultiple: false, Value.Count: > 1 })
            {
                logWarning(key, "field only allows a single value.");
                field.SetValue(field.DefaultValues);
                continue;
            }

            // validate allow-values
            if (field.AllowValues.Any())
            {
                IInvariantSet invalidValues = field.Value.GetWithout(field.AllowValues);
                if (invalidValues.Any())
                {
                    logWarning(key, $"found invalid values ({string.Join(", ", invalidValues)}), expected: {string.Join(", ", field.AllowValues)}.");
                    field.SetValue(field.DefaultValues);
                    continue;
                }
            }

            // save values
            field.SetValue(configValues[key]);
        }
    }
}
