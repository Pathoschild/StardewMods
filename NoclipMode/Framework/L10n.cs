using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace Pathoschild.Stardew.NoclipMode.Framework
{
    /// <summary>Get translations from the mod's <c>i18n</c> folder.</summary>
    /// <remarks>This is auto-generated from the <c>i18n/default.json</c> file when the T4 template is saved.</remarks>
    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Deliberately named for consistency and to match translation conventions.")]
    internal class L10n
    {
        /*********
        ** Fields
        *********/
        /// <summary>The mod's translation helper.</summary>
        private static ITranslationHelper Translations;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="translations">The mod's translation helper.</param>
        public static void Init(ITranslationHelper translations)
        {
            L10n.Translations = translations;
        }

        /// <summary>Get a translation equivalent to "Enabled noclip mode; press {{button}} again to turn it off.".</summary>
        /// <param name="button">The value to inject for the <c>{{button}}</c> token.</param>
        public static string EnabledMessage(object button)
        {
            return L10n.Translations.Get("enabled-message", new { button });
        }

        /// <summary>Get a translation equivalent to "Disabled noclip mode; press {{button}} again to turn it on.".</summary>
        /// <param name="button">The value to inject for the <c>{{button}}</c> token.</param>
        public static string DisabledMessage(object button)
        {
            return L10n.Translations.Get("disabled-message", new { button });
        }
    }
}
