using StardewModdingAPI;
using StardewValley.Menus;

namespace Pathoschild.Stardew.Common.Integrations.BetterGameMenu;

/// <summary>Handles the logic for integrating with the Better Game Menu mod.</summary>
internal class BetterGameMenuIntegration : BaseIntegration<IBetterGameMenuApi>
{
    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
    /// <param name="monitor">Encapsulates monitoring and logging.</param>
    public BetterGameMenuIntegration(IModRegistry modRegistry, IMonitor monitor)
        : base("BetterGameMenu", "leclair.bettergamemenu", "0.5.2", modRegistry, monitor) { }

    /// <summary>Get the currently active page of the provided Better Game Menu instance. If the provided menu isn't a Better Game Menu, return <c>null</c>.</summary>
    /// <param name="menu">The game menu to get the page from.</param>
    public IClickableMenu? GetCurrentPage(IClickableMenu? menu)
    {
        if (menu is null)
            return null;

        return this.SafelyCallApi(
            api => api.GetCurrentPage(menu),
            "Failed getting game menu page from {0}."
        );
    }
}
