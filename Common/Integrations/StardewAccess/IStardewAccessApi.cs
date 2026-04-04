using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.Common.Integrations.StardewAccess;

public interface IStardewAccessApi
{
    /// <summary>The configured primary left-click keybind.</summary>
    public KeybindList LeftClickMainKey { get; }

    /// <summary>The configured alternate left-click keybind.</summary>
    public KeybindList LeftClickAlternateKey { get; }

    /// <summary>The configured info/action keybind used by Stardew Access for hover-only controls.</summary>
    public KeybindList PrimaryInfoKey { get; }

    /// <summary>Speaks the text via the loaded screen reader (if any).</summary>
    /// <param name="text">The text to be narrated.</param>
    /// <param name="interrupt">Whether to skip the currently speaking text or not.</param>
    /// <returns>true if the text was spoken otherwise false.</returns>
    public bool Say(string text, bool interrupt);

    /// <summary>The previous menu query used by Stardew Access to suppress duplicate menu narration.</summary>
    public string PrevMenuQueryText { get; set; }

    /// <summary>A one-time prefix applied to the next narrated menu text.</summary>
    public string MenuPrefixNoQueryText { get; set; }

    /// <summary>Speaks the text via the loaded screen reader (if any).
    /// <br/>Skips the text narration if the previously narrated text was the same as the one provided.
    /// <br/><br/>Use this when narrating hovered component in menus to avoid interference.</summary>
    /// <param name="text">The text to be narrated.</param>
    /// <param name="interrupt">Whether to skip the currently speaking text or not.</param>
    /// <param name="customQuery">If set, uses this instead of <paramref name="text"/> as query to check whether to speak the text or not.</param>
    /// <returns>true if the text was spoken otherwise false.</returns>
    public bool SayWithMenuChecker(string text, bool interrupt, string? customQuery = null);

    /// <summary>Get the spoken item details using Stardew Access' existing wording.</summary>
    /// <param name="item">The item to describe.</param>
    /// <param name="giveExtraDetails">Whether to include the more verbose details.</param>
    /// <param name="price">The custom price to include, or <c>-1</c> to use the item's default price.</param>
    /// <param name="extraItemToShowIndex">An optional extra item index to narrate alongside the item.</param>
    /// <param name="extraItemToShowAmount">The amount for the extra item to narrate.</param>
    public string GetDetailsOfItem(Item item, bool giveExtraDetails = false, int price = -1, string? extraItemToShowIndex = null, int extraItemToShowAmount = -1);

    /// <summary>Speak the currently hovered slot from an inventory menu using Stardew Access' built-in menu narration.</summary>
    public bool SpeakHoveredInventorySlot(InventoryMenu? inventoryMenu, bool? giveExtraDetails = null, int hoverPrice = -1, string? extraItemToShowIndex = null, int extraItemToShowAmount = -1, string highlightedItemPrefix = "", string highlightedItemSuffix = "", int? hoverX = null, int? hoverY = null);

    /// <summary>Translate a Stardew Access translation key using its own i18n files.</summary>
    public string Translate(string translationKey, object? tokens = null, string translationCategory = "Default", bool disableWarning = false);
}
