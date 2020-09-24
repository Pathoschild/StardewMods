using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace Pathoschild.Stardew.DebugMode.Framework
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

        /// <summary>Get a translation equivalent to "tile".</summary>
        public static string Label_Tile()
        {
            return I18n.GetByKey("label.tile");
        }

        /// <summary>Get a translation equivalent to "map".</summary>
        public static string Label_Map()
        {
            return I18n.GetByKey("label.map");
        }

        /// <summary>Get a translation equivalent to "menu".</summary>
        public static string Label_Menu()
        {
            return I18n.GetByKey("label.menu");
        }

        /// <summary>Get a translation equivalent to "submenu".</summary>
        public static string Label_Submenu()
        {
            return I18n.GetByKey("label.submenu");
        }

        /// <summary>Get a translation equivalent to "minigame".</summary>
        public static string Label_Minigame()
        {
            return I18n.GetByKey("label.minigame");
        }

        /// <summary>Get a translation equivalent to "festival".</summary>
        public static string Label_FestivalName()
        {
            return I18n.GetByKey("label.festival-name");
        }

        /// <summary>Get a translation equivalent to "event ID".</summary>
        public static string Label_EventId()
        {
            return I18n.GetByKey("label.event-id");
        }

        /// <summary>Get a translation equivalent to "event script".</summary>
        public static string Label_EventScript()
        {
            return I18n.GetByKey("label.event-script");
        }

        /// <summary>Get a translation equivalent to "song".</summary>
        public static string Label_Song()
        {
            return I18n.GetByKey("label.song");
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

