using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using StardewModdingAPI;
using StardewValley;

namespace ContentPatcher.Framework.Tokens.ValueProviders
{
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
            this.ValidNamedArguments = null; // allow any named argument
            this.MarkReady(true);
        }

        /// <inheritdoc />
        public override bool UpdateContext(IContext context)
        {
            if (this.TranslationHelper.LocaleEnum == this.LastLocale)
                return false;

            this.LastLocale = this.TranslationHelper.LocaleEnum;
            return true;
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetValues(IInputArguments input)
        {
            this.AssertInput(input);

            // get translation
            Translation translation = this.TranslationHelper.Get(input.GetFirstPositionalArg());

            // add tokens
            if (input.HasNamedArgs)
            {
                translation = translation.Tokens(input
                    .NamedArgs
                    .ToDictionary(p => p.Key, p => this.Stringify(p.Value))
                );
            }

            // add default value
            if (input.NamedArgs.TryGetValue("default", out IInputArgumentValue defaultValue))
            {
                translation = translation
                    .Default(this.Stringify(defaultValue))
                    .UsePlaceholder(false); // allow setting a blank default
            }

            yield return translation;
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
}
