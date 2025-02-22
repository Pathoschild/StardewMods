using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;

namespace Pathoschild.Stardew.DataLayers;

/// <summary>The API which lets other mods interact with Data Layers.</summary>
public interface IDataLayersApi
{
    /// <summary>Register or override layer color schemes.</summary>
    /// <param name="schemeData">The color scheme data, in the form color scheme ID → color name → color values.</param>
    /// <param name="assetName">The name of the asset from which the data was loaded. This is only used to log errors and doesn't affect behavior.</param>
    void RegisterColorSchemes(Dictionary<string, Dictionary<string, string?>> schemeData, string assetName);

    /// <summary>Register a data layer.</summary>
    /// <param name="id">A unique ID for the layer. This only needs to be unique within the layers added by the mod using this API, since the mod ID will be prefixed automatically.</param>
    /// <param name="name">The translated layer name to show in-game.</param>
    /// <param name="getTileGroups">Register the possible tile groups when this layer is loaded.</param>
    /// <param name="updateTiles">Get the tiles to show in the layer when it's drawn. This is called repeatedly while the layer is being drawn to the screen, based on the layer update rate.</param>
    void RegisterLayer(string id, Func<string> name, GetTileGroupsDelegate getTileGroups, UpdateTilesDelegate updateTiles);
}

/// <summary>Register the possible tile groups when this layer is loaded.</summary>
/// <param name="addGroup">Add a tile group to the list.</param>
public delegate void GetTileGroupsDelegate(AddTileGroupDelegate addGroup);

/// <summary>Add a tile group.</summary>
/// <param name="id">A unique ID for the tile type within this layer.</param>
/// <param name="name">The translated tile group name to show in-game.</param>
/// <param name="overlayColor">The overlay color for tiles of this type (unless overridden in the player's color scheme).</param>
/// <param name="borderColor">The color for the tile group's outer borders, or <c>null</c> for no border.</param>
public delegate void AddTileGroupDelegate(string id, Func<string> name, Color overlayColor, Color? borderColor = null);

/// <summary>Get the tiles to show in the layer when it's drawn.</summary>
/// <param name="location">The current location.</param>
/// <param name="visibleArea">The tile area currently visible on the screen.</param>
/// <param name="visibleTiles">The tile positions currently visible on the screen.</param>
/// <param name="cursorTile">The tile position under the cursor.</param>
/// <returns>Returns a lookup of the tile positions by tile group ID.</returns>
public delegate ILookup<string, Vector2> UpdateTilesDelegate(GameLocation location, Rectangle visibleArea, IReadOnlySet<Vector2> visibleTiles, Vector2 cursorTile);
