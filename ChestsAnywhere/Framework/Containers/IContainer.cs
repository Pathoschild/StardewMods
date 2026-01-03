using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.Containers;

/// <summary>An in-game container which can store items.</summary>
internal interface IContainer
{
    /*********
    ** Accessors
    *********/
    /// <summary>The underlying inventory.</summary>
    IList<Item?> Inventory { get; }

    /// <summary>The persisted data for this container.</summary>
    ContainerData Data { get; }

    /// <summary>Whether Automate options can be configured for storing items in this chest.</summary>
    public bool CanConfigureAutomateStore { get; }

    /// <summary>Whether Automate options can be configured for taking items from this chest.</summary>
    public bool CanConfigureAutomateTake { get; }


    /*********
    ** Public methods
    *********/
    /// <summary>Get whether the inventory can accept the item type.</summary>
    /// <param name="item">The item.</param>
    bool CanAcceptItem(Item item);

    /// <summary>Get whether the container has the given context tag.</summary>
    /// <param name="tag">The context tag to check.</param>
    bool HasContextTag(string tag);

    /// <summary>Get whether another instance wraps the same underlying container.</summary>
    /// <param name="container">The other container.</param>
    bool IsSameAs(IContainer? container);

    /// <summary>Get whether another instance wraps the same underlying container.</summary>
    /// <param name="inventory">The other container's inventory.</param>
    bool IsSameAs(IList<Item?>? inventory);

    /// <summary>Open a menu to transfer items between the player's inventory and this container.</summary>
    /// <returns>Returns an instance of the opened menu assigned to <see cref="Game1.activeClickableMenu"/>.</returns>
    IClickableMenu OpenMenu();

    /// <summary>Persist the container data.</summary>
    void SaveData();

    /// <summary>Try to get an icon for the chest for the search UI.</summary>
    /// <param name="texture">The texture containing the icon.</param>
    /// <param name="sourceRect">The pixel area within the <paramref name="texture"/> containing the icon.</param>
    /// <param name="scale">The default scale at which to draw the icon.</param>
    /// <returns>Returns <c>true</c> if an icon was returned, else <c>false</c>.</returns>
    bool TryGetIcon([NotNullWhen(true)] out Texture2D? texture, out Rectangle sourceRect, out float scale);
}
