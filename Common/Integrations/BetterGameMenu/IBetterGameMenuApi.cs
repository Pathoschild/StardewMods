#nullable enable

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.Common.Integrations.BetterGameMenu;


/// <summary>
/// This interface represents a Better Game Menu. 
/// </summary>
public interface IBetterGameMenu
{
    /// <summary>
    /// The <see cref="IClickableMenu"/> instance for this game menu. This is
    /// the same object, but with a different type. This property is included
    /// for convenience due to how API proxying works.
    /// </summary>
    IClickableMenu Menu { get; }

    /// <summary>
    /// Whether or not the menu is currently drawing itself. This is typically
    /// always <c>false</c> except when viewing the <c>Map</c> tab.
    /// </summary>
    bool Invisible { get; set; }

    /// <summary>
    /// A list of ids of the currently visible tabs.
    /// </summary>
    IReadOnlyList<string> VisibleTabs { get; }

    /// <summary>
    /// The id of the currently active tab.
    /// </summary>
    string CurrentTab { get; }

    /// <summary>
    /// The <see cref="IClickableMenu"/> instance for the currently active tab.
    /// This may be <c>null</c> if the page instance for the currently active
    /// tab is still being initialized.
    /// </summary>
    IClickableMenu? CurrentPage { get; }

    /// <summary>
    /// Whether or not the currently displayed page is an error page. Error
    /// pages are used when a tab implementation's GetPageInstance method
    /// throws an exception.
    /// </summary>
    bool CurrentTabHasErrored { get; }

    /// <summary>
    /// Try to get the source for the specific tab.
    /// </summary>
    /// <param name="target">The id of the tab to get the source of.</param>
    /// <param name="source">The unique ID of the mod that registered the
    /// implementation being used, or <c>stardew</c> if the base game's
    /// implementation is being used.</param>
    /// <returns>Whether or not the tab is registered with the system.</returns>
    bool TryGetSource(string target, [NotNullWhen(true)] out string? source);

    /// <summary>
    /// Try to get the <see cref="IClickableMenu"/> instance for a specific tab.
    /// </summary>
    /// <param name="target">The id of the tab to get the page for.</param>
    /// <param name="page">The page instance, if one exists.</param>
    /// <param name="forceCreation">If set to true, an instance will attempt to
    /// be created if one has not already been created.</param>
    /// <returns>Whether or not a page instance for that tab exists.</returns>
    bool TryGetPage(string target, [NotNullWhen(true)] out IClickableMenu? page, bool forceCreation = false);

    /// <summary>
    /// Attempt to change the currently active tab to the target tab.
    /// </summary>
    /// <param name="target">The id of the tab to change to.</param>
    /// <param name="playSound">Whether or not to play a sound.</param>
    /// <returns>Whether or not the tab was changed successfully.</returns>
    bool TryChangeTab(string target, bool playSound = true);

    /// <summary>
    /// Force the menu to recalculate the visible tabs. This will not recreate
    /// <see cref="IClickableMenu"/> instances, but can be used to cause an
    /// inactive tab to be removed, or a previously hidden tab to be added.
    /// This can also be used to update tab decorations if necessary.
    /// </summary>
    /// <param name="target">Optionally, a specific tab to update rather than
    /// updating all tabs.</param>
    void UpdateTabs(string? target = null);

}


/// <summary>
/// This enum is included for reference and has the order value for
/// all the default tabs from the base game. These values are intentionally
/// spaced out to allow for modded tabs to be inserted at specific points.
/// </summary>
public enum VanillaTabOrders
{
    Inventory = 0,
    Skills = 20,
    Social = 40,
    Map = 60,
    Crafting = 80,
    Animals = 100,
    Powers = 120,
    Collections = 140,
    Options = 160,
    Exit = 200
}


public interface IBetterGameMenuApi
{

    /// <summary>
    /// A delegate for drawing something onto the screen.
    /// </summary>
    /// <param name="batch">The <see cref="SpriteBatch"/> to draw with.</param>
    /// <param name="bounds">The region where the thing should be drawn.</param>
    public delegate void DrawDelegate(SpriteBatch batch, Rectangle bounds);

    #region Menu Class Access

    /// <summary>
    /// The active screen's current Better Game Menu, if one is open,
    /// else <c>null</c>.
    /// </summary>
    IBetterGameMenu? ActiveMenu { get; }

    /// <summary>
    /// Attempt to cast the provided menu into an <see cref="IBetterGameMenu"/>.
    /// This can be useful if you're working with a menu that isn't currently
    /// assigned to <see cref="Game1.activeClickableMenu"/>.
    /// </summary>
    /// <param name="menu">The menu to attempt to cast</param>
    IBetterGameMenu? AsMenu(IClickableMenu menu);

    #endregion

}
