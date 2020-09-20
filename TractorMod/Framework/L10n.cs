using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace Pathoschild.Stardew.TractorMod.Framework
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

        /// <summary>Get a translation equivalent to "Tractor Power".</summary>
        public static string Buff_Name()
        {
            return L10n.Translations.Get("buff.name");
        }

        /// <summary>Get a translation equivalent to "Tractor Garage".</summary>
        public static string Garage_Name()
        {
            return L10n.Translations.Get("garage.name");
        }

        /// <summary>Get a translation equivalent to "A garage to store your tractor. Tractor included!".</summary>
        public static string Garage_Description()
        {
            return L10n.Translations.Get("garage.description");
        }
    }
}
