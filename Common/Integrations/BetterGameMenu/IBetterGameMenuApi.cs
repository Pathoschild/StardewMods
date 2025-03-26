using StardewValley.Menus;

namespace Pathoschild.Stardew.Common.Integrations.BetterGameMenu;

public interface IBetterGameMenuApi
{
    /// <summary>Get the current page of the provided Better Game Menu instance. If the provided menu is not a Better Game Menu, or a page is not ready, then return <c>null</c> instead.</summary>
    /// <param name="menu">The menu to get the page from.</param>
    IClickableMenu? GetCurrentPage(IClickableMenu menu);
}
