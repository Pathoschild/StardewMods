using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.DataLayers;
using StardewValley;

namespace Pathoschild.Stardew.TestDataLayersMod.Framework;

/// <summary>A data layer which shows a checkerboard tile pattern.</summary>
internal static class CheckerboardLayer
{
    /*********
    ** Public methods
    *********/
    /// <inheritdoc cref="GetTileGroupsDelegate" />
    public static void GetTileGroups(AddTileGroupDelegate addGroup)
    {
        addGroup(id: "even", name: I18n.Layer_Even, overlayColor: "yes");
        addGroup(id: "odd", name: I18n.Layer_Odd, overlayColor: "no");
    }

    /// <inheritdoc cref="UpdateTilesDelegate" />
    public static ILookup<string, Vector2> UpdateTiles(GameLocation location, Rectangle visibleArea, IReadOnlySet<Vector2> visibleTiles, Vector2 cursorTile)
    {
        return visibleTiles.ToLookup(CheckerboardLayer.GetLayerId);
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get the tile group ID for a tile position.</summary>
    /// <param name="tile">The tile position.</param>
    private static string GetLayerId(Vector2 tile)
    {
        return (tile.X + tile.Y) % 2 == 0
            ? "even"
            : "odd";
    }
}
