using StardewModdingAPI.Utilities;
using StardewValley;

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
    public string GetDetailsOfItem(Item item, bool giveExtraDetails = false);
}
