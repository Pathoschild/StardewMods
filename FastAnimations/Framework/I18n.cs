using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace Pathoschild.Stardew.FastAnimations.Framework
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

        /// <summary>Get a translation equivalent to "General Options".</summary>
        public static string Config_GeneralOptions()
        {
            return I18n.GetByKey("config.general-options");
        }

        /// <summary>Get a translation equivalent to "Disable eat/drink prompt".</summary>
        public static string Config_DisableEatPrompt_Name()
        {
            return I18n.GetByKey("config.disable-eat-prompt.name");
        }

        /// <summary>Get a translation equivalent to "Whether to skip the confirmation prompt asking if you really want to eat/drink something.".</summary>
        public static string Config_DisableEatPrompt_Tooltip()
        {
            return I18n.GetByKey("config.disable-eat-prompt.tooltip");
        }

        /// <summary>Get a translation equivalent to "Player Animation Speeds".</summary>
        public static string Config_AnimationSpeeds()
        {
            return I18n.GetByKey("config.animation-speeds");
        }

        /// <summary>Get a translation equivalent to "Eat/drink".</summary>
        public static string Config_EatOrDrink_Name()
        {
            return I18n.GetByKey("config.eat-or-drink.name");
        }

        /// <summary>Get a translation equivalent to "How fast you eat and drink. Default {{defaultValue}}x.".</summary>
        /// <param name="defaultValue">The value to inject for the <c>{{defaultValue}}</c> token.</param>
        public static string Config_EatOrDrink_Tooltip(object defaultValue)
        {
            return I18n.GetByKey("config.eat-or-drink.tooltip", new { defaultValue });
        }

        /// <summary>Get a translation equivalent to "Fish".</summary>
        public static string Config_Fish_Name()
        {
            return I18n.GetByKey("config.fish.name");
        }

        /// <summary>Get a translation equivalent to "How fast you cast and reel when fishing (doesn't affect the minigame). Default {{defaultValue}}x, suggested {{suggestedValue}}x.".</summary>
        /// <param name="defaultValue">The value to inject for the <c>{{defaultValue}}</c> token.</param>
        /// <param name="suggestedValue">The value to inject for the <c>{{suggestedValue}}</c> token.</param>
        public static string Config_Fish_Tooltip(object defaultValue, object suggestedValue)
        {
            return I18n.GetByKey("config.fish.tooltip", new { defaultValue, suggestedValue });
        }

        /// <summary>Get a translation equivalent to "Harvest".</summary>
        public static string Config_Harvest_Name()
        {
            return I18n.GetByKey("config.harvest.name");
        }

        /// <summary>Get a translation equivalent to "How fast you harvest crops and forage by hand. Default {{defaultValue}}x.".</summary>
        /// <param name="defaultValue">The value to inject for the <c>{{defaultValue}}</c> token.</param>
        public static string Config_Harvest_Tooltip(object defaultValue)
        {
            return I18n.GetByKey("config.harvest.tooltip", new { defaultValue });
        }

        /// <summary>Get a translation equivalent to "Milk".</summary>
        public static string Config_Milk_Name()
        {
            return I18n.GetByKey("config.milk.name");
        }

        /// <summary>Get a translation equivalent to "How fast you use the milk pail. Default {{defaultValue}}x.".</summary>
        /// <param name="defaultValue">The value to inject for the <c>{{defaultValue}}</c> token.</param>
        public static string Config_Milk_Tooltip(object defaultValue)
        {
            return I18n.GetByKey("config.milk.tooltip", new { defaultValue });
        }

        /// <summary>Get a translation equivalent to "Mount/dismount".</summary>
        public static string Config_Mount_Name()
        {
            return I18n.GetByKey("config.mount.name");
        }

        /// <summary>Get a translation equivalent to "How fast you mount/dismount horses (including custom mounts like Tractor Mod). Default {{defaultValue}}x.".</summary>
        /// <param name="defaultValue">The value to inject for the <c>{{defaultValue}}</c> token.</param>
        public static string Config_Mount_Tooltip(object defaultValue)
        {
            return I18n.GetByKey("config.mount.tooltip", new { defaultValue });
        }

        /// <summary>Get a translation equivalent to "Shear".</summary>
        public static string Config_Shear_Name()
        {
            return I18n.GetByKey("config.shear.name");
        }

        /// <summary>Get a translation equivalent to "How fast you use the shears. Default {{defaultValue}}x.".</summary>
        /// <param name="defaultValue">The value to inject for the <c>{{defaultValue}}</c> token.</param>
        public static string Config_Shear_Tooltip(object defaultValue)
        {
            return I18n.GetByKey("config.shear.tooltip", new { defaultValue });
        }

        /// <summary>Get a translation equivalent to "Swing tool".</summary>
        public static string Config_Tool_Name()
        {
            return I18n.GetByKey("config.tool.name");
        }

        /// <summary>Get a translation equivalent to "How fast you swing your tools (except weapons & fishing rod). Default {{defaultValue}}x, suggested {{suggestedValue}}x.".</summary>
        /// <param name="defaultValue">The value to inject for the <c>{{defaultValue}}</c> token.</param>
        /// <param name="suggestedValue">The value to inject for the <c>{{suggestedValue}}</c> token.</param>
        public static string Config_Tool_Tooltip(object defaultValue, object suggestedValue)
        {
            return I18n.GetByKey("config.tool.tooltip", new { defaultValue, suggestedValue });
        }

        /// <summary>Get a translation equivalent to "Swing weapon".</summary>
        public static string Config_Weapon_Name()
        {
            return I18n.GetByKey("config.weapon.name");
        }

        /// <summary>Get a translation equivalent to "How fast you swing your weapons. Default {{defaultValue}}x, suggested {{suggestedValue}}x.".</summary>
        /// <param name="defaultValue">The value to inject for the <c>{{defaultValue}}</c> token.</param>
        /// <param name="suggestedValue">The value to inject for the <c>{{suggestedValue}}</c> token.</param>
        public static string Config_Weapon_Tooltip(object defaultValue, object suggestedValue)
        {
            return I18n.GetByKey("config.weapon.tooltip", new { defaultValue, suggestedValue });
        }

        /// <summary>Get a translation equivalent to "World Animation Speeds".</summary>
        public static string Config_WorldSpeeds()
        {
            return I18n.GetByKey("config.world-speeds");
        }

        /// <summary>Get a translation equivalent to "Break geodes".</summary>
        public static string Config_BreakGeodes_Name()
        {
            return I18n.GetByKey("config.break-geodes.name");
        }

        /// <summary>Get a translation equivalent to "How fast the blacksmith breaks geodes for you. Default {{defaultValue}}x.".</summary>
        /// <param name="defaultValue">The value to inject for the <c>{{defaultValue}}</c> token.</param>
        public static string Config_BreakGeodes_Tooltip(object defaultValue)
        {
            return I18n.GetByKey("config.break-geodes.tooltip", new { defaultValue });
        }

        /// <summary>Get a translation equivalent to "Casino slots".</summary>
        public static string Config_CasinoSlots_Name()
        {
            return I18n.GetByKey("config.casino-slots.name");
        }

        /// <summary>Get a translation equivalent to "How fast the casino slots turn. Default {{defaultValue}}x.".</summary>
        /// <param name="defaultValue">The value to inject for the <c>{{defaultValue}}</c> token.</param>
        public static string Config_CasinoSlots_Tooltip(object defaultValue)
        {
            return I18n.GetByKey("config.casino-slots.tooltip", new { defaultValue });
        }

        /// <summary>Get a translation equivalent to "Pam's bus".</summary>
        public static string Config_Bus_Name()
        {
            return I18n.GetByKey("config.bus.name");
        }

        /// <summary>Get a translation equivalent to "How fast Pam drives her bus to and from the desert. Default {{defaultValue}}x.".</summary>
        /// <param name="defaultValue">The value to inject for the <c>{{defaultValue}}</c> token.</param>
        public static string Config_Bus_Tooltip(object defaultValue)
        {
            return I18n.GetByKey("config.bus.tooltip", new { defaultValue });
        }

        /// <summary>Get a translation equivalent to "Tree falling".</summary>
        public static string Config_TreeFall_Name()
        {
            return I18n.GetByKey("config.tree-fall.name");
        }

        /// <summary>Get a translation equivalent to "How fast trees fall after you chop them down. Default {{defaultValue}}x, suggested {{suggestedValue}}x.".</summary>
        /// <param name="defaultValue">The value to inject for the <c>{{defaultValue}}</c> token.</param>
        /// <param name="suggestedValue">The value to inject for the <c>{{suggestedValue}}</c> token.</param>
        public static string Config_TreeFall_Tooltip(object defaultValue, object suggestedValue)
        {
            return I18n.GetByKey("config.tree-fall.tooltip", new { defaultValue, suggestedValue });
        }

        /// <summary>Get a translation equivalent to "UI Animation Speeds".</summary>
        public static string Config_UiSpeeds()
        {
            return I18n.GetByKey("config.ui-speeds");
        }

        /// <summary>Get a translation equivalent to "Title menu transitions".</summary>
        public static string Config_TitleMenu_Name()
        {
            return I18n.GetByKey("config.title-menu.name");
        }

        /// <summary>Get a translation equivalent to "How fast the title menu transitions between screens. Default {{defaultValue}}x.".</summary>
        /// <param name="defaultValue">The value to inject for the <c>{{defaultValue}}</c> token.</param>
        public static string Config_TitleMenu_Tooltip(object defaultValue)
        {
            return I18n.GetByKey("config.title-menu.tooltip", new { defaultValue });
        }

        /// <summary>Get a translation equivalent to "Load game blink".</summary>
        public static string Config_LoadGameBlink_Name()
        {
            return I18n.GetByKey("config.load-game-blink.name");
        }

        /// <summary>Get a translation equivalent to "How fast the blinking-slot delay happens after you click a load-save slot. Default {{defaultValue}}x.".</summary>
        /// <param name="defaultValue">The value to inject for the <c>{{defaultValue}}</c> token.</param>
        public static string Config_LoadGameBlink_Tooltip(object defaultValue)
        {
            return I18n.GetByKey("config.load-game-blink.tooltip", new { defaultValue });
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

