using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace Pathoschild.Stardew.DebugMode.Framework
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

        /// <summary>Get a translation equivalent to "tile".</summary>
        public static string Label_Tile()
        {
            return L10n.Translations.Get("label.tile");
        }

        /// <summary>Get a translation equivalent to "map".</summary>
        public static string Label_Map()
        {
            return L10n.Translations.Get("label.map");
        }

        /// <summary>Get a translation equivalent to "menu".</summary>
        public static string Label_Menu()
        {
            return L10n.Translations.Get("label.menu");
        }

        /// <summary>Get a translation equivalent to "submenu".</summary>
        public static string Label_Submenu()
        {
            return L10n.Translations.Get("label.submenu");
        }

        /// <summary>Get a translation equivalent to "minigame".</summary>
        public static string Label_Minigame()
        {
            return L10n.Translations.Get("label.minigame");
        }

        /// <summary>Get a translation equivalent to "festival".</summary>
        public static string Label_FestivalName()
        {
            return L10n.Translations.Get("label.festival-name");
        }

        /// <summary>Get a translation equivalent to "event ID".</summary>
        public static string Label_EventId()
        {
            return L10n.Translations.Get("label.event-id");
        }

        /// <summary>Get a translation equivalent to "event script".</summary>
        public static string Label_EventScript()
        {
            return L10n.Translations.Get("label.event-script");
        }

        /// <summary>Get a translation equivalent to "song".</summary>
        public static string Label_Song()
        {
            return L10n.Translations.Get("label.song");
        }
    }
}
