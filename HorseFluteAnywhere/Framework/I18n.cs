using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace Pathoschild.Stardew.HorseFluteAnywhere.Framework
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

        /// <summary>Get a translation equivalent to "Require Horse Flute".</summary>
        public static string Config_RequireFlute_Name()
        {
            return I18n.GetByKey("config.require-flute.name");
        }

        /// <summary>Get a translation equivalent to "Whether you need the horse flute item in your inventory to summon a horse. Default false.".</summary>
        public static string Config_RequireFlute_Description()
        {
            return I18n.GetByKey("config.require-flute.description");
        }

        /// <summary>Get a translation equivalent to "Summon Horse Button".</summary>
        public static string Config_SummonHorseButton_Name()
        {
            return I18n.GetByKey("config.summon-horse-button.name");
        }

        /// <summary>Get a translation equivalent to "The button to press which plays the flute and summons a horse. Default {{defaultValue}}.".</summary>
        /// <param name="defaultValue">The value to inject for the <c>{{defaultValue}}</c> token.</param>
        public static string Config_SummonHorseButton_Description(object defaultValue)
        {
            return I18n.GetByKey("config.summon-horse-button.description", new { defaultValue });
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

