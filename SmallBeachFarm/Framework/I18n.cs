using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace Pathoschild.Stardew.SmallBeachFarm.Framework
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

        /// <summary>Get a translation equivalent to "Add campfire".</summary>
        public static string Config_Campfire_Name()
        {
            return I18n.GetByKey("config.campfire.name");
        }

        /// <summary>Get a translation equivalent to "Add a functional campfire in front of the farmhouse.".</summary>
        public static string Config_Campfire_Tooltip()
        {
            return I18n.GetByKey("config.campfire.tooltip");
        }

        /// <summary>Get a translation equivalent to "Add islands".</summary>
        public static string Config_Islands_Name()
        {
            return I18n.GetByKey("config.islands.name");
        }

        /// <summary>Get a translation equivalent to "Add ocean islands with extra land area.".</summary>
        public static string Config_Islands_Tooltip()
        {
            return I18n.GetByKey("config.islands.tooltip");
        }

        /// <summary>Get a translation equivalent to "Play beach sounds".</summary>
        public static string Config_BeachSounds_Name()
        {
            return I18n.GetByKey("config.beach-sounds.name");
        }

        /// <summary>Get a translation equivalent to "Play the beach's background music (i.e. wave sounds) on the beach farm.".</summary>
        public static string Config_BeachSounds_Tooltip()
        {
            return I18n.GetByKey("config.beach-sounds.tooltip");
        }

        /// <summary>Get a translation equivalent to "Replace farm type".</summary>
        public static string Config_FarmType_Name()
        {
            return I18n.GetByKey("config.farm-type.name");
        }

        /// <summary>Get a translation equivalent to "The farm layout to replace.".</summary>
        public static string Config_FarmType_Tooltip()
        {
            return I18n.GetByKey("config.farm-type.tooltip");
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

