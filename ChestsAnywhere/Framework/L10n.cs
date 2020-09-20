using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework
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

        /// <summary>Get a translation equivalent to "Auto-grabber #{{number}}".</summary>
        /// <param name="number">The value to inject for the <c>{{number}}</c> token.</param>
        public static string DefaultName_AutoGrabber(object number)
        {
            return L10n.Translations.Get("default-name.auto-grabber", new { number });
        }

        /// <summary>Get a translation equivalent to "Chest #{{number}}".</summary>
        /// <param name="number">The value to inject for the <c>{{number}}</c> token.</param>
        public static string DefaultName_Chest(object number)
        {
            return L10n.Translations.Get("default-name.chest", new { number });
        }

        /// <summary>Get a translation equivalent to "Fridge".</summary>
        public static string DefaultName_Fridge()
        {
            return L10n.Translations.Get("default-name.fridge");
        }

        /// <summary>Get a translation equivalent to "Junimo hut #{{number}}".</summary>
        /// <param name="number">The value to inject for the <c>{{number}}</c> token.</param>
        public static string DefaultName_JunimoHut(object number)
        {
            return L10n.Translations.Get("default-name.junimo-hut", new { number });
        }

        /// <summary>Get a translation equivalent to "Shipping bin".</summary>
        public static string DefaultName_ShippingBin()
        {
            return L10n.Translations.Get("default-name.shipping-bin");
        }

        /// <summary>Get a translation equivalent to "Shipping bin (ship items)".</summary>
        public static string DefaultName_ShippingBin_Store()
        {
            return L10n.Translations.Get("default-name.shipping-bin.store");
        }

        /// <summary>Get a translation equivalent to "Shipping bin (retrieve items)".</summary>
        public static string DefaultName_ShippingBin_Take()
        {
            return L10n.Translations.Get("default-name.shipping-bin.take");
        }

        /// <summary>Get a translation equivalent to "Cabin ({{owner}})".</summary>
        /// <param name="owner">The value to inject for the <c>{{owner}}</c> token.</param>
        public static string DefaultCategory_OwnedCabin(object owner)
        {
            return L10n.Translations.Get("default-category.owned-cabin", new { owner });
        }

        /// <summary>Get a translation equivalent to "Cabin (empty)".</summary>
        public static string DefaultCategory_UnownedCabin()
        {
            return L10n.Translations.Get("default-category.unowned-cabin");
        }

        /// <summary>Get a translation equivalent to "{{locationName}} #{{number}}".</summary>
        /// <param name="locationName">The value to inject for the <c>{{locationName}}</c> token.</param>
        /// <param name="number">The value to inject for the <c>{{number}}</c> token.</param>
        public static string DefaultCategory_Duplicate(object locationName, object number)
        {
            return L10n.Translations.Get("default-category.duplicate", new { locationName, number });
        }

        /// <summary>Get a translation equivalent to "Location".</summary>
        public static string Label_Location()
        {
            return L10n.Translations.Get("label.location");
        }

        /// <summary>Get a translation equivalent to "tile {{x}}, {{y}}".</summary>
        /// <param name="x">The value to inject for the <c>{{x}}</c> token.</param>
        /// <param name="y">The value to inject for the <c>{{y}}</c> token.</param>
        public static string Label_Location_Tile(object x, object y)
        {
            return L10n.Translations.Get("label.location.tile", new { x, y });
        }

        /// <summary>Get a translation equivalent to "Name".</summary>
        public static string Label_Name()
        {
            return L10n.Translations.Get("label.name");
        }

        /// <summary>Get a translation equivalent to "Category".</summary>
        public static string Label_Category()
        {
            return L10n.Translations.Get("label.category");
        }

        /// <summary>Get a translation equivalent to "Order".</summary>
        public static string Label_Order()
        {
            return L10n.Translations.Get("label.order");
        }

        /// <summary>Get a translation equivalent to "Hide this chest".</summary>
        public static string Label_HideChest()
        {
            return L10n.Translations.Get("label.hide-chest");
        }

        /// <summary>Get a translation equivalent to "Hide this chest (you'll need to find the chest to undo this!)".</summary>
        public static string Label_HideChestHidden()
        {
            return L10n.Translations.Get("label.hide-chest-hidden");
        }

        /// <summary>Get a translation equivalent to "(Automate) Put items in this chest".</summary>
        public static string Label_AutomateStore()
        {
            return L10n.Translations.Get("label.automate-store");
        }

        /// <summary>Get a translation equivalent to "(Automate) Put items in this chest first".</summary>
        public static string Label_AutomateStoreFirst()
        {
            return L10n.Translations.Get("label.automate-store-first");
        }

        /// <summary>Get a translation equivalent to "(Automate) Take items from this chest".</summary>
        public static string Label_AutomateTake()
        {
            return L10n.Translations.Get("label.automate-take");
        }

        /// <summary>Get a translation equivalent to "(Automate) Take items from this chest first".</summary>
        public static string Label_AutomateTakeFirst()
        {
            return L10n.Translations.Get("label.automate-take-first");
        }

        /// <summary>Get a translation equivalent to "edit chest".</summary>
        public static string Button_EditChest()
        {
            return L10n.Translations.Get("button.edit-chest");
        }

        /// <summary>Get a translation equivalent to "sort inventory".</summary>
        public static string Button_SortInventory()
        {
            return L10n.Translations.Get("button.sort-inventory");
        }

        /// <summary>Get a translation equivalent to "save".</summary>
        public static string Button_Save()
        {
            return L10n.Translations.Get("button.save");
        }

        /// <summary>Get a translation equivalent to "reset".</summary>
        public static string Button_Reset()
        {
            return L10n.Translations.Get("button.reset");
        }

        /// <summary>Get a translation equivalent to "You can't access chests from here.".</summary>
        public static string Errors_DisabledFromHere()
        {
            return L10n.Translations.Get("errors.disabled-from-here");
        }

        /// <summary>Get a translation equivalent to "You don't have any chests yet.".</summary>
        public static string Errors_NoChests()
        {
            return L10n.Translations.Get("errors.no-chests");
        }

        /// <summary>Get a translation equivalent to "You don't have any chests in this area.".</summary>
        public static string Errors_NoChestsInLocation()
        {
            return L10n.Translations.Get("errors.no-chests-in-location");
        }

        /// <summary>Get a translation equivalent to "You don't have any chests in range.".</summary>
        public static string Errors_NoChestsInRange()
        {
            return L10n.Translations.Get("errors.no-chests-in-range");
        }
    }
}
