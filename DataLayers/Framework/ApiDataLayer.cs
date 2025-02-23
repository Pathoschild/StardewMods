using System;

namespace Pathoschild.Stardew.DataLayers.Framework;

/// <summary>The metadata for a data layer registered through the API.</summary>
/// <param name="UniqueId">The unique ID for this layer, including the mod ID prefix.</param>
/// <param name="LocalId">The unique ID for this layer within the mod, used in color schemes.</param>
/// <param name="Name">The translated layer name to show in-game.</param>
/// <param name="GetTileGroups">Register the possible tile groups when this layer is loaded. This is called once.</param>
/// <param name="UpdateTiles">Get the tiles to show in the layer when it's drawn. This is called repeatedly while the layer is being drawn to the screen, based on the layer update rate.</param>
/// <param name="DefaultUpdatesPerSecond">The default number of updates needed per second, or <c>null</c> for the default update rate. This can be a decimal value (e.g. 0.5 to update every two seconds).</param>
/// <param name="DefaultUpdateWhenViewChanges">Whether to update the layer by default when the player's tile view changes, regardless of the <paramref name="DefaultUpdatesPerSecond"/> value.</param>
internal record ApiDataLayer(string UniqueId, string LocalId, Func<string> Name, GetTileGroupsDelegate GetTileGroups, UpdateTilesDelegate UpdateTiles, decimal? DefaultUpdatesPerSecond, bool DefaultUpdateWhenViewChanges);
