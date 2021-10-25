using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace Pathoschild.Stardew.SkipIntro.Framework
{
    /// <summary>Get translations from the mod's <c>i18n</c> folder.</summary>
    /// <remarks>This is auto-generated from the <c>i18n/default.json</c> file when the T4 template is saved.</remarks>
    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Deliberately named for consistency and to match translation conventions.")]
    internal static class I18n
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
            I18n.Translations = translations;
        }

        /// <summary>Get a translation equivalent to "Skip to".</summary>
        public static string Config_SkipTo_Name()
        {
            return I18n.GetByKey("config.skip-to.name");
        }

        /// <summary>Get a translation equivalent to "Which screen to skip to. Default Title.".</summary>
        public static string Config_SkipTo_Tooltip()
        {
            return I18n.GetByKey("config.skip-to.tooltip");
        }

        /// <summary>Get a translation equivalent to "Title menu".</summary>
        public static string Config_SkipTo_Values_TitleMenu()
        {
            return I18n.GetByKey("config.skip-to.values.title-menu");
        }

        /// <summary>Get a translation equivalent to "Load menu".</summary>
        public static string Config_SkipTo_Values_LoadMenu()
        {
            return I18n.GetByKey("config.skip-to.values.load-menu");
        }

        /// <summary>Get a translation equivalent to "Join co-op".</summary>
        public static string Config_SkipTo_Values_JoinCoop()
        {
            return I18n.GetByKey("config.skip-to.values.join-coop");
        }

        /// <summary>Get a translation equivalent to "Host co-op".</summary>
        public static string Config_SkipTo_Values_HostCoop()
        {
            return I18n.GetByKey("config.skip-to.values.host-coop");
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get a translation by its key.</summary>
        /// <param name="key">The translation key.</param>
        /// <param name="tokens">An object containing token key/value pairs. This can be an anonymous object (like <c>new { value = 42, name = "Cranberries" }</c>), a dictionary, or a class instance.</param>
        private static Translation GetByKey(string key, object tokens = null)
        {
            if (I18n.Translations == null)
                throw new InvalidOperationException($"You must call {nameof(I18n)}.{nameof(I18n.Init)} from the mod's entry method before reading translations.");
            return I18n.Translations.Get(key, tokens);
        }
    }
}

