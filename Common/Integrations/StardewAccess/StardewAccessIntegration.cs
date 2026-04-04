using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.Common.Integrations.StardewAccess;

/// <summary>Handles the logic for integrating with the Stardew Access mod.</summary>
internal class StardewAccessIntegration : BaseIntegration<IStardewAccessApi>
{
    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
    /// <param name="monitor">Encapsulates monitoring and logging.</param>
    public StardewAccessIntegration(IModRegistry modRegistry, IMonitor monitor)
        : base("Stardew Access", "shoaib.stardewaccess", "1.6.2", modRegistry, monitor) { }

    /// <summary>The configured primary left-click keybind.</summary>
    public KeybindList LeftClickMainKey
        => this.SafelyCallApi(api => api.LeftClickMainKey, "reading Stardew Access keybinds", new KeybindList()) ?? new KeybindList();

    /// <summary>The configured alternate left-click keybind.</summary>
    public KeybindList LeftClickAlternateKey
        => this.SafelyCallApi(api => api.LeftClickAlternateKey, "reading Stardew Access keybinds", new KeybindList()) ?? new KeybindList();

    /// <summary>The configured info/action keybind used by Stardew Access for hover-only controls.</summary>
    public KeybindList PrimaryInfoKey
        => this.SafelyCallApi(api => api.PrimaryInfoKey, "reading Stardew Access keybinds", new KeybindList()) ?? new KeybindList();

    /// <summary>The previous menu query used by Stardew Access to suppress duplicate menu narration.</summary>
    public string PrevMenuQueryText
    {
        get => this.SafelyCallApi(api => api.PrevMenuQueryText, "reading Stardew Access previous menu query", "") ?? "";
        set => this.SafelyCallApi(api => api.PrevMenuQueryText = value, "setting Stardew Access previous menu query");
    }

    /// <summary>A one-time prefix applied to the next narrated menu text.</summary>
    public string MenuPrefixNoQueryText
    {
        get => this.SafelyCallApi(api => api.MenuPrefixNoQueryText, "reading Stardew Access menu prefix", "") ?? "";
        set => this.SafelyCallApi(api => api.MenuPrefixNoQueryText = value, "setting Stardew Access menu prefix");
    }

    /// <inheritdoc cref="IStardewAccessApi.Say" />
    public bool Say(string text, bool interrupt)
    {
        return this.SafelyCallApi(
            api => api.Say(text, interrupt),
            "saying text"
        );
    }

    /// <inheritdoc cref="IStardewAccessApi.SayWithMenuChecker" />
    public bool SayWithMenuChecker(string text, bool interrupt, string? customQuery = null)
    {
        return this.SafelyCallApi(
            api => api.SayWithMenuChecker(text, interrupt, customQuery),
            "saying menu text"
        );
    }

    /// <summary>Speaks the content of the given element while using the menu query to prevent speaking multiple times in the menu.</summary>
    /// <param name="element">The element to be spoken.</param>
    /// <param name="interrupt">Whether to skip the currently speaking text or not.</param>
    /// <returns>true if the element was spoken otherwise false.</returns>
    public bool SayMenuElement(IScreenReadable element, bool interrupt = true)
    {
        // note: SayMenuElement is added in Stardew Access 1.7.0 beta, so mimic the implementation here.
        return this.SafelyCallApi(
            api => api.SayWithMenuChecker(element.ScreenReaderText, interrupt),
            "saying menu element"
        );
    }

    /// <inheritdoc cref="IStardewAccessApi.GetDetailsOfItem" />
    public string GetDetailsOfItem(Item item, bool giveExtraDetails = false)
    {
        return this.SafelyCallApi(
            api => api.GetDetailsOfItem(item, giveExtraDetails, -1, null, -1),
            "getting item details",
            item.DisplayName
        ) ?? item.DisplayName;
    }

    /// <inheritdoc cref="IStardewAccessApi.SpeakHoveredInventorySlot" />
    public bool SpeakHoveredInventorySlot(InventoryMenu? inventoryMenu, bool? giveExtraDetails = null, int hoverPrice = -1, string? extraItemToShowIndex = null, int extraItemToShowAmount = -1, string highlightedItemPrefix = "", string highlightedItemSuffix = "", int? hoverX = null, int? hoverY = null)
    {
        return this.SafelyCallApi(
            api => api.SpeakHoveredInventorySlot(inventoryMenu, giveExtraDetails, hoverPrice, extraItemToShowIndex, extraItemToShowAmount, highlightedItemPrefix, highlightedItemSuffix, hoverX, hoverY),
            "speaking hovered inventory slot"
        );
    }

    /// <summary>Translate a Stardew Access translation key using its own i18n files.</summary>
    public string Translate(string translationKey, object? tokens = null, string translationCategory = "Default", bool disableWarning = false)
    {
        return this.SafelyCallApi(
            api => api.Translate(translationKey, tokens, translationCategory, disableWarning),
            "translating Stardew Access text",
            translationKey
        ) ?? translationKey;
    }
}
