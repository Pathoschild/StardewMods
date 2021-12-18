using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>Get translations from the mod's <c>i18n</c> folder.</summary>
    /// <remarks>This is auto-generated from the <c>i18n/default.json</c> file when the T4 template is saved.</remarks>
    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Deliberately named for consistency and to match translation conventions.")]
    internal static partial class I18n
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

        /// <summary>Get a translation equivalent to "Main options".</summary>
        public static string Config_Title_MainOptions()
        {
            return I18n.GetByKey("config.title.main-options");
        }

        /// <summary>Get a translation equivalent to "Junimo huts output gems".</summary>
        public static string Config_JunimoHutsOutputGems_Name()
        {
            return I18n.GetByKey("config.junimo-huts-output-gems.name");
        }

        /// <summary>Get a translation equivalent to "Whether to pull gems out of Junimo huts. If true, you won't be able to change Junimo colors by placing gemstones in their hut.".</summary>
        public static string Config_JunimoHutsOutputGems_Desc()
        {
            return I18n.GetByKey("config.junimo-huts-output-gems.desc");
        }

        /// <summary>Get a translation equivalent to "Automation interval".</summary>
        public static string Config_AutomationInterval_Name()
        {
            return I18n.GetByKey("config.automation-interval.name");
        }

        /// <summary>Get a translation equivalent to "The number of ticks between each automation process (60 for once per second, 120 for every two seconds, etc).".</summary>
        public static string Config_AutomationInterval_Desc()
        {
            return I18n.GetByKey("config.automation-interval.desc");
        }

        /// <summary>Get a translation equivalent to "Toggle overlay key".</summary>
        public static string Config_ToggleOverlayKey_Name()
        {
            return I18n.GetByKey("config.toggle-overlay-key.name");
        }

        /// <summary>Get a translation equivalent to "The keys which toggle the automation overlay.".</summary>
        public static string Config_ToggleOverlayKey_Desc()
        {
            return I18n.GetByKey("config.toggle-overlay-key.desc");
        }

        /// <summary>Get a translation equivalent to "Mod compatibility".</summary>
        public static string Config_Title_ModCompatibility()
        {
            return I18n.GetByKey("config.title.mod-compatibility");
        }

        /// <summary>Get a translation equivalent to "Better Junimos".</summary>
        public static string Config_BetterJunimos_Name()
        {
            return I18n.GetByKey("config.better-junimos.name");
        }

        /// <summary>Get a translation equivalent to "Enable compatibility with Better Junimos. If it's installed, Junimo huts won't output fertilizer or seeds.".</summary>
        public static string Config_BetterJunimos_Desc()
        {
            return I18n.GetByKey("config.better-junimos.desc");
        }

        /// <summary>Get a translation equivalent to "Warn for missing bridge mod".</summary>
        public static string Config_WarnForMissingBridgeMod_Name()
        {
            return I18n.GetByKey("config.warn-for-missing-bridge-mod.name");
        }

        /// <summary>Get a translation equivalent to "Whether to log a warning on startup if you installed a custom-machine mod that requires a separate compatibility patch which isn't installed.".</summary>
        public static string Config_WarnForMissingBridgeMod_Desc()
        {
            return I18n.GetByKey("config.warn-for-missing-bridge-mod.desc");
        }

        /// <summary>Get a translation equivalent to "Enabled connectors".</summary>
        public static string Config_Title_Connectors()
        {
            return I18n.GetByKey("config.title.connectors");
        }

        /// <summary>Get a translation equivalent to "Whether {{itemName}} links machines and chests together.".</summary>
        /// <param name="itemName">The value to inject for the <c>{{itemName}}</c> token.</param>
        public static string Config_Connector_Desc(object itemName)
        {
            return I18n.GetByKey("config.connector.desc", new { itemName });
        }

        /// <summary>Get a translation equivalent to "Custom connectors".</summary>
        public static string Config_CustomConnectors_Name()
        {
            return I18n.GetByKey("config.custom-connectors.name");
        }

        /// <summary>Get a translation equivalent to "The exact English names for items which connect adjacent machines together. You can list multiple items with commas.".</summary>
        public static string Config_CustomConnectors_Desc()
        {
            return I18n.GetByKey("config.custom-connectors.desc");
        }

        /// <summary>Get a translation equivalent to "Machine overrides".</summary>
        public static string Config_Title_MachineOverrides()
        {
            return I18n.GetByKey("config.title.machine-overrides");
        }

        /// <summary>Get a translation equivalent to "Shipping Bin".</summary>
        public static string Config_Override_ShippingBinName()
        {
            return I18n.GetByKey("config.override.ShippingBin-name");
        }

        /// <summary>Get a translation equivalent to "Mini-Shipping Bin".</summary>
        public static string Config_Override_MiniShippingBinName()
        {
            return I18n.GetByKey("config.override.MiniShippingBin-name");
        }

        /// <summary>Get a translation equivalent to "Whether to enable {{machineName}} automation.".</summary>
        /// <param name="machineName">The value to inject for the <c>{{machineName}}</c> token.</param>
        public static string Config_Override_Desc(object machineName)
        {
            return I18n.GetByKey("config.override.desc", new { machineName });
        }

        /// <summary>Get a translation equivalent to "For custom overrides, see https://github.com/Pathoschild/StardewMods/tree/develop/Automate#configure for help editing the config.json file directly.".</summary>
        public static string Config_CustomOverridesNote()
        {
            return I18n.GetByKey("config.custom-overrides-note");
        }

        /// <summary>Get a translation by its key.</summary>
        /// <param name="key">The translation key.</param>
        /// <param name="tokens">An object containing token key/value pairs. This can be an anonymous object (like <c>new { value = 42, name = "Cranberries" }</c>), a dictionary, or a class instance.</param>
        public static Translation GetByKey(string key, object tokens = null)
        {
            if (I18n.Translations == null)
                throw new InvalidOperationException($"You must call {nameof(I18n)}.{nameof(I18n.Init)} from the mod's entry method before reading translations.");
            return I18n.Translations.Get(key, tokens);
        }
    }
}

