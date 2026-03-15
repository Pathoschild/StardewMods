namespace Pathoschild.Stardew.Common.Integrations.StardewAccess;

public interface IStardewAccessApi
{
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
}
