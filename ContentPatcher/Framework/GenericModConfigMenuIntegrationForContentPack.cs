using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ContentPatcher.Framework.ConfigModels;
using Pathoschild.Stardew.Common.Integrations.GenericModConfigMenu;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using TConfigMenu = Pathoschild.Stardew.Common.Integrations.GenericModConfigMenu.GenericModConfigMenuIntegration<Pathoschild.Stardew.Common.Utilities.InvariantDictionary<ContentPatcher.Framework.ConfigModels.ConfigField>>;

namespace ContentPatcher.Framework;

/// <summary>Registers the mod configuration for a content pack with Generic Mod Config Menu.</summary>
internal class GenericModConfigMenuIntegrationForContentPack : IGenericModConfigMenuIntegrationFor<InvariantDictionary<ConfigField>>
{
    /*********
    ** Fields
    *********/
    /// <summary>The content pack whose config is being managed.</summary>
    private readonly IContentPack ContentPack;

    /// <summary>The config model.</summary>
    private readonly InvariantDictionary<ConfigField> Config;

    /// <summary>Parse a comma-delimited set of case-insensitive condition values.</summary>
    private readonly Func<string, IInvariantSet> ParseCommaDelimitedField;

    /// <summary>Game content helper.</summary>
    private readonly IGameContentHelper GameContent;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="contentPack">The content pack whose config is being managed.</param>
    /// <param name="parseCommaDelimitedField">The Generic Mod Config Menu integration.</param>
    /// <param name="config">The config model.</param>
    public GenericModConfigMenuIntegrationForContentPack(IContentPack contentPack, Func<string, IInvariantSet> parseCommaDelimitedField, InvariantDictionary<ConfigField> config, IGameContentHelper gameContent)
    {
        this.ContentPack = contentPack;
        this.Config = config;
        this.ParseCommaDelimitedField = parseCommaDelimitedField;
        this.GameContent = gameContent;
    }

    /// <inheritdoc />
    public void Register(TConfigMenu menu, IMonitor monitor)
    {
        // get fields by section
        InvariantDictionary<InvariantDictionary<InvariantDictionary<ConfigField>>> fieldsByPageBySection = [];
        foreach (var (name, config) in this.Config)
        {
            string pageId = config.Page?.Trim() ?? "";
            if (!fieldsByPageBySection.TryGetValue(pageId, out InvariantDictionary<InvariantDictionary<ConfigField>>? page))
                fieldsByPageBySection[pageId] = page = new();

            string sectionId = config.Section?.Trim() ?? "";
            if (!page.TryGetValue(sectionId, out InvariantDictionary<ConfigField>? section))
                page[sectionId] = section = new();

            section[name] = config;
        }

        // no configs to show, exit
        if (fieldsByPageBySection.Count == 0)
            return;

        menu.Register();

        // add section/field elements
        foreach ((string pageId, InvariantDictionary<InvariantDictionary<ConfigField>> pages) in fieldsByPageBySection.OrderBy(kv => kv.Key))
        {
            if (pageId == "")
            {
                this.AddPageElements(menu, pages);
            }
            else
            {
                this.AddPage(menu, pageId, pages);
            }
        }
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Register a config menu field with Generic Mod Config Menu.</summary>
    /// <param name="menu">The integration API through which to register the config menu.</param>
    /// <param name="name">The config field name.</param>
    /// <param name="field">The config field instance.</param>
    private void AddField(TConfigMenu menu, string name, ConfigField field)
    {
        // get translation logic
        string GetName() => this.TryTranslate($"config.{name}.name", name);
        string GetDescription() => this.TryTranslate($"config.{name}.description", field.Description);
        string GetValueText(string value) => this.TryTranslate($"config.{name}.values.{value}", value);

        // textbox if any values allowed
        if (!field.AllowValues.Any())
        {
            menu.AddTextbox(
                name: GetName,
                tooltip: GetDescription,
                get: _ => string.Join(", ", field.Value),
                set: (_, newValue) =>
                {
                    IInvariantSet values = this.ParseCommaDelimitedField(newValue);

                    field.SetValue(field.AllowMultiple || values.Count <= 1
                        ? values
                        : InvariantSets.FromValue(values.First())
                    );
                }
            );
        }

        // checkboxes if player can choose multiple values
        else if (field.AllowMultiple)
        {
            foreach (string value in field.AllowValues)
            {
                menu.AddCheckbox(
                    name: () => $"{GetName()}: {GetValueText(value)}",
                    tooltip: GetDescription,
                    get: _ => field.Value.Contains(value),
                    set: (_, selected) =>
                    {
                        // toggle value
                        field.SetValue(selected
                            ? field.Value.GetWith(value)
                            : field.Value.GetWithout(value)
                        );

                        // set default if blank
                        if (!field.AllowBlank && !field.Value.Any())
                            field.SetValue(field.DefaultValues);
                    }
                );
            }
        }

        // checkbox for single boolean
        else if (!field.AllowBlank && field.IsBoolean())
        {
            menu.AddCheckbox(
                name: GetName,
                tooltip: GetDescription,
                get: _ => field.Value.Contains(true.ToString()),
                set: (_, selected) => field.SetValue(
                    InvariantSets.FromValue(selected)
                )
            );
        }

        // slider for single numeric range
        else if (!field.AllowBlank && field.IsNumericRange(out int min, out int max))
        {
            if (!int.TryParse(field.DefaultValues.FirstOrDefault(), out int defaultValue))
                defaultValue = min;

            // number slider
            menu.AddNumberField(
                name: GetName,
                tooltip: GetDescription,
                get: _ => int.TryParse(field.Value.FirstOrDefault(), out int val) ? val : defaultValue,
                set: (_, val) => field.SetValue(
                    InvariantSets.FromValue(
                        val.ToString(CultureInfo.InvariantCulture)
                    )
                ),
                min: min,
                max: max
            );
        }

        // dropdown for single multiple-choice value
        else
        {
            List<string> choices = [.. field.AllowValues];
            if (field.AllowBlank)
                choices.Insert(0, "");

            menu.AddDropdown(
                name: GetName,
                tooltip: GetDescription,
                get: _ => field.Value.FirstOrDefault() ?? "",
                set: (_, newValue) => field.SetValue(
                    InvariantSets.FromValue(newValue)
                ),
                allowedValues: choices.ToArray(),
                formatAllowedValue: GetValueText
            );
        }

        // image for preview image
        if (field.PreviewImages != null)
        {
            foreach (var image in field.PreviewImages)
            {
                menu.AddImage(
                    texture: () => image.GetPreviewTexture(this.GameContent),
                    texturePixelArea: image.SourceRect,
                    scale: image.Scale
                );
            }
        }
    }

    /// <summary>Register a config menu section with Generic Mod Config Menu.</summary>
    /// <param name="menu">The integration API through which to register the config menu.</param>
    /// <param name="name">The config section name.</param>
    private void AddSection(TConfigMenu menu, string name)
    {
        menu.AddSectionTitle(
            text: () => this.TryTranslate($"config.section.{name}.name", name),
            tooltip: () => this.TryTranslate($"config.section.{name}.description", null)
        );
    }

    private void AddPage(TConfigMenu menu, string pageId, InvariantDictionary<InvariantDictionary<ConfigField>> pages)
    {
        menu.AddPage(
            pageId,
            title: () => this.TryTranslate($"config.page.{pageId}.name", pageId),
            tooltip: () => this.TryTranslate($"config.page.{pageId}.description", null),
            (pageMenu) => this.AddPageElements(pageMenu, pages)
        );
    }

    private void AddPageElements(TConfigMenu menu, InvariantDictionary<InvariantDictionary<ConfigField>> pages)
    {
        foreach ((string sectionId, InvariantDictionary<ConfigField> fields) in pages)
        {
            if (!fields.Any())
                continue;

            if (sectionId != "")
                this.AddSection(menu, sectionId);

            foreach ((string name, ConfigField config) in fields)
                this.AddField(menu, name, config);
        }
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
