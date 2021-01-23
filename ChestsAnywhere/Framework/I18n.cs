using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework
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

        /// <summary>Get a translation equivalent to "Fridge".</summary>
        public static string DefaultName_Fridge()
        {
            return I18n.GetByKey("default-name.fridge");
        }

        /// <summary>Get a translation equivalent to "{{name}} #{{number}}".</summary>
        /// <param name="name">The value to inject for the <c>{{name}}</c> token.</param>
        /// <param name="number">The value to inject for the <c>{{number}}</c> token.</param>
        public static string DefaultName_Other(object name, object number)
        {
            return I18n.GetByKey("default-name.other", new { name, number });
        }

        /// <summary>Get a translation equivalent to "ship items".</summary>
        public static string DefaultName_ShippingBin_Store()
        {
            return I18n.GetByKey("default-name.shipping-bin.store");
        }

        /// <summary>Get a translation equivalent to "retrieve items".</summary>
        public static string DefaultName_ShippingBin_Take()
        {
            return I18n.GetByKey("default-name.shipping-bin.take");
        }

        /// <summary>Get a translation equivalent to "Cabin ({{owner}})".</summary>
        /// <param name="owner">The value to inject for the <c>{{owner}}</c> token.</param>
        public static string DefaultCategory_OwnedCabin(object owner)
        {
            return I18n.GetByKey("default-category.owned-cabin", new { owner });
        }

        /// <summary>Get a translation equivalent to "Cabin (empty)".</summary>
        public static string DefaultCategory_UnownedCabin()
        {
            return I18n.GetByKey("default-category.unowned-cabin");
        }

        /// <summary>Get a translation equivalent to "{{locationName}} #{{number}}".</summary>
        /// <param name="locationName">The value to inject for the <c>{{locationName}}</c> token.</param>
        /// <param name="number">The value to inject for the <c>{{number}}</c> token.</param>
        public static string DefaultCategory_Duplicate(object locationName, object number)
        {
            return I18n.GetByKey("default-category.duplicate", new { locationName, number });
        }

        /// <summary>Get a translation equivalent to "Location".</summary>
        public static string Label_Location()
        {
            return I18n.GetByKey("label.location");
        }

        /// <summary>Get a translation equivalent to "tile {{x}}, {{y}}".</summary>
        /// <param name="x">The value to inject for the <c>{{x}}</c> token.</param>
        /// <param name="y">The value to inject for the <c>{{y}}</c> token.</param>
        public static string Label_Location_Tile(object x, object y)
        {
            return I18n.GetByKey("label.location.tile", new { x, y });
        }

        /// <summary>Get a translation equivalent to "Name".</summary>
        public static string Label_Name()
        {
            return I18n.GetByKey("label.name");
        }

        /// <summary>Get a translation equivalent to "Category".</summary>
        public static string Label_Category()
        {
            return I18n.GetByKey("label.category");
        }

        /// <summary>Get a translation equivalent to "Order".</summary>
        public static string Label_Order()
        {
            return I18n.GetByKey("label.order");
        }

        /// <summary>Get a translation equivalent to "Hide this chest".</summary>
        public static string Label_HideChest()
        {
            return I18n.GetByKey("label.hide-chest");
        }

        /// <summary>Get a translation equivalent to "Hide this chest (you'll need to find the chest to undo this!)".</summary>
        public static string Label_HideChestHidden()
        {
            return I18n.GetByKey("label.hide-chest-hidden");
        }

        /// <summary>Get a translation equivalent to "Automate options".</summary>
        public static string Label_AutomateOptions()
        {
            return I18n.GetByKey("label.automate-options");
        }

        /// <summary>Get a translation equivalent to "Put items in this chest".</summary>
        public static string Label_AutomateStore()
        {
            return I18n.GetByKey("label.automate-store");
        }

        /// <summary>Get a translation equivalent to "Put items in this chest first".</summary>
        public static string Label_AutomateStoreFirst()
        {
            return I18n.GetByKey("label.automate-store-first");
        }

        /// <summary>Get a translation equivalent to "Never put items in this chest".</summary>
        public static string Label_AutomateStoreDisabled()
        {
            return I18n.GetByKey("label.automate-store-disabled");
        }

        /// <summary>Get a translation equivalent to "Take items from this chest".</summary>
        public static string Label_AutomateTake()
        {
            return I18n.GetByKey("label.automate-take");
        }

        /// <summary>Get a translation equivalent to "Take items from this chest first".</summary>
        public static string Label_AutomateTakeFirst()
        {
            return I18n.GetByKey("label.automate-take-first");
        }

        /// <summary>Get a translation equivalent to "Never take items from this chest".</summary>
        public static string Label_AutomateTakeDisabled()
        {
            return I18n.GetByKey("label.automate-take-disabled");
        }

        /// <summary>Get a translation equivalent to "edit chest".</summary>
        public static string Button_EditChest()
        {
            return I18n.GetByKey("button.edit-chest");
        }

        /// <summary>Get a translation equivalent to "sort inventory".</summary>
        public static string Button_SortInventory()
        {
            return I18n.GetByKey("button.sort-inventory");
        }

        /// <summary>Get a translation equivalent to "save".</summary>
        public static string Button_Save()
        {
            return I18n.GetByKey("button.save");
        }

        /// <summary>Get a translation equivalent to "reset".</summary>
        public static string Button_Reset()
        {
            return I18n.GetByKey("button.reset");
        }

        /// <summary>Get a translation equivalent to "You can't access chests from here.".</summary>
        public static string Errors_DisabledFromHere()
        {
            return I18n.GetByKey("errors.disabled-from-here");
        }

        /// <summary>Get a translation equivalent to "You don't have any chests yet.".</summary>
        public static string Errors_NoChests()
        {
            return I18n.GetByKey("errors.no-chests");
        }

        /// <summary>Get a translation equivalent to "You don't have any chests in this area.".</summary>
        public static string Errors_NoChestsInLocation()
        {
            return I18n.GetByKey("errors.no-chests-in-location");
        }

        /// <summary>Get a translation equivalent to "You don't have any chests in range.".</summary>
        public static string Errors_NoChestsInRange()
        {
            return I18n.GetByKey("errors.no-chests-in-range");
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

