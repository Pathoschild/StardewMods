using System;

using StardewModdingAPI;

using StardewValley.Menus;


namespace Pathoschild.Stardew.Common.Integrations.BetterGameMenu;

/// <summary>Handles the logic for integrating with the Better Game Menu mod.</summary>
internal class BetterGameMenuIntegration : BaseIntegration<IBetterGameMenuApi>
{
    public BetterGameMenuIntegration(IModRegistry modRegistry, IMonitor monitor)
        : base("BetterGameMenu", "leclair.bettergamemenu", "0.5.0", modRegistry, monitor) { }

    /// <summary>
    /// Get the currently active page of the provided Better Game Menu instance. If
    /// the provided menu isn't a Better Game Menu, return <c>null</c>.
    /// </summary>
    /// <param name="menu">The game menu to get the page from.</param>
    public IClickableMenu? GetCurrentPage(IClickableMenu menu)
    {
        if (this.IsLoaded && this.ModApi.AsMenu(menu) is IBetterGameMenu bgm)
            return bgm.CurrentPage;
        return null;
    }

    /// <summary>
    /// The currently active Better Game Menu instance of the current screen, if one
    /// exists, otherwise <c>null</c>.
    /// </summary>
    public IBetterGameMenu? ActiveMenu => this.IsLoaded ? this.ModApi.ActiveMenu : null;

}
