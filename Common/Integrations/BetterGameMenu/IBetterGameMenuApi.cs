#nullable enable

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley.Menus;

namespace Pathoschild.Stardew.Common.Integrations.BetterGameMenu;


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
    /// Get the current page of the provided Better Game Menu instance. If the
    /// provided menu is not a Better Game Menu, or a page is not ready, then
    /// return <c>null</c> instead.
    /// </summary>
    /// <param name="menu">The menu to get the page from.</param>
    IClickableMenu? GetCurrentPage(IClickableMenu menu);

    #endregion

}
