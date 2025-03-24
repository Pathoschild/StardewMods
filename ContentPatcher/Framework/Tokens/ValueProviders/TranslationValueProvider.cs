using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using StardewModdingAPI;
using StardewValley;

namespace ContentPatcher.Framework.Tokens.ValueProviders;

/// <summary>A value provider which checks whether a file exists in the content pack's folder.</summary>
internal class TranslationValueProvider : BaseValueProvider
{
    /*********
    ** Fields
    *********/
    /// <summary>Gets translations from the content pack's translation folder.</summary>
    private readonly ITranslationHelper TranslationHelper;

    /// <summary>The game locale as of the last context update.</summary>
    private LocalizedContentManager.LanguageCode LastLocale;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="translationHelper">Gets translations from the content pack's translation folder.</param>
    public TranslationValueProvider(ITranslationHelper translationHelper)
        : base(ConditionType.I18n, mayReturnMultipleValuesForRoot: false)
    {
        this.TranslationHelper = translationHelper;
        this.LastLocale = translationHelper.LocaleEnum;

        this.EnableInputArguments(required: true, mayReturnMultipleValues: false, maxPositionalArgs: 1);
        this.AllowAnyNamedArguments = true;
        this.MarkReady(true);
    }

    /// <inheritdoc />
    public override bool UpdateContext(IContext context)
    {
        LocalizedContentManager.LanguageCode curLocale = this.TranslationHelper.LocaleEnum;

        if (curLocale == this.LastLocale)
            return false;

        this.LastLocale = curLocale;
        return true;
    }

    /// <inheritdoc />
    public override IEnumerable<string> GetValues(IInputArguments input)
    {
        this.AssertInput(input);

        // get key
        string? key = input.GetFirstPositionalArg();
        if (string.IsNullOrWhiteSpace(key))
            return InvariantSets.Empty;

        // get tokens
        object? tokens = input.HasNamedArgs
            ? input.NamedArgs.ToDictionary(p => p.Key, p => this.Stringify(p.Value))
            : null;

        // get translation
        Translation translation = this.TranslationHelper
            .Get(key, tokens)
            .ApplyGenderSwitchBlocks(false); // preprocessing gender switch blocks doesn't work with patch update rates (e.g. NPCs won't update their dialogue once the save is loaded)
        bool hasValue = translation.HasValue();

        // apply fallback keys
        if (!hasValue && input.NamedArgs.TryGetValue("defaultKeys", out IInputArgumentValue? defaultKeys))
        {
            foreach (string defaultKey in defaultKeys.Parsed)
            {
                Translation newTranslation = this.TranslationHelper
                    .Get(defaultKey, tokens)
                    .ApplyGenderSwitchBlocks(false);

                if (newTranslation.HasValue())
                {
                    translation = newTranslation;
                    hasValue = true;
                    break;
                }
            }
        }

        // add default value
        if (!hasValue && input.NamedArgs.TryGetValue("default", out IInputArgumentValue? defaultValue))
        {
            translation = translation
                .Default(this.Stringify(defaultValue))
                .UsePlaceholder(false); // allow setting a blank default
        }

        return InvariantSets.FromCaseSensitiveValue(translation);
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get the string representation for an input argument.</summary>
    /// <param name="input">The input argument.</param>
    private string Stringify(IInputArgumentValue input)
    {
        return string.Join(", ", input.Parsed);
    }
}
