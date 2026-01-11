using StardewModdingAPI;
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
}
