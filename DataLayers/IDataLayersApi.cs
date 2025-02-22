using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace Pathoschild.Stardew.DataLayers;

/// <summary>The API which lets other mods interact with Data Layers.</summary>
public interface IDataLayersApi
{
    /// <summary>Register or override layer color schemes.</summary>
    /// <param name="schemeData">The color scheme data, in the form color scheme ID → color name → color values.</param>
    /// <param name="assetName">The name of the asset from which the data was loaded. This is only used to log errors and doesn't affect behavior.</param>
    void RegisterColorSchemes(Dictionary<string, Dictionary<string, string?>> schemeData, string assetName);

    /// <summary>Register a data layer to show in-game.</summary>
    /// <param name="mod">The manifest for the mod registering the layer.</param>
    /// <param name="id">A unique ID for the layer within those provided by the same mod. Can be left empty if the mod only provides a single layer.</param>
    /// <param name="layer">The layer implementation to register.</param>
    void RegisterLayer(IManifest mod, string id, IDataLayer layer);
}

/// <summary>A data layer registered through <see cref="IDataLayersApi" />.</summary>
public interface IDataLayer
{
    /// <summary>The layer name to show in-game.</summary>
    string Name { get; }

    /// <summary>Configure the legend display for this layer.</summary>
    /// <param name="legendBuilder">The legend builder. All tile type IDs that could be referenced in an <see cref="Update"/> must be registered in the legend.</param>
    void Configure(ILegendBuilder legendBuilder);

    /// <summary>Get the updated data layer tiles.</summary>
    /// <param name="location">The current location.</param>
    /// <param name="builder">The builder instance to which current tile groups and tiles should be added.</param>
    /// <param name="visibleArea">The tile area currently visible on the screen.</param>
    /// <param name="visibleTiles">The tile positions currently visible on the screen.</param>
    /// <param name="cursorTile">The tile position under the cursor.</param>
    void Update(ILayerBuilder builder, GameLocation location, Rectangle visibleArea, IReadOnlySet<Vector2> visibleTiles, Vector2 cursorTile);
}

/// <summary>An API to register the legend entries for a data layer.</summary>
public interface ILegendBuilder
{
    /// <summary>Add an item to the legend.</summary>
    /// <param name="id">A unique ID for the tile type within this layer. This must match the ID provided to <see cref="ILayerBuilder.AddTileGroup"/> and <see cref="ITileGroupBuilder.AddTile"/>.</param>
    /// <param name="name">The descriptive name to show in-game.</param>
    /// <param name="color">The default overlay color for tiles of this type when no color is specified in the <see cref="ColorScheme"/>.</param>
    /// <returns>The builder instance for chaining.</returns>
    ILegendBuilder Add(string id, string name, Color color);
}

/// <summary>An API which tracks the layer tiles to display after an update.</summary>
public interface ILayerBuilder
{
    /// <summary>Add a tile group to display on the next update frame.</summary>
    /// <param name="defaultTileTypeId">The default tile type ID for tiles which don't have an explicit color. This must match the ID provided to <see cref="IDataLayer.Configure"/>.</param>
    /// <param name="buildGroup">The action invoked to build a tile group during an update.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <remarks>A tile group is a set of related tiles that are related in some way (e.g. tiles covered by a sprinker) which have a common overlay color and border.</remarks>
    ILayerBuilder AddTileGroup(string defaultTileTypeId, Action<ITileGroupBuilder> buildGroup);
}

/// <summary>An API to add tiles to a tile group registered via <see cref="ILayerBuilder"/>.</summary>
public interface ITileGroupBuilder
{
    /// <summary>Add one tile to the group.</summary>
    /// <param name="position">The coordinate containing the tile, measured in in-game map tiles.</param>
    /// <param name="typeId">The tile type, matching the ID provided to <see cref="IDataLayer.Configure"/>. This can be omitted to use the <c>defaultTileTypeId</c> specified when creating the group from <see cref="ILayerBuilder.AddTileGroup"/>.</param>
    /// <returns>The builder instance for chaining.</returns>
    ITileGroupBuilder AddTile(Vector2 position, string? typeId = null);

    /// <summary>Add multiple tiles to the group.</summary>
    /// <param name="positions">The coordinate containing the tiles, measured in in-game map tiles.</param>
    /// <param name="typeIdSelector">Provides the tile type (matching the ID provided to <see cref="IDataLayer.Configure"/>) given the tile coordinate. This can be omitted to use the <c>defaultTileTypeId</c> specified when creating the group from <see cref="ILayerBuilder.AddTileGroup"/>.</param>
    /// <returns>The builder instance for chaining.</returns>
    ITileGroupBuilder AddTiles(IEnumerable<Vector2> positions, Func<Vector2, string>? typeIdSelector);

    /// <summary>Configure the border drawn around the outer edges of the tile group 'islands' (i.e. edges that don't border another tile in the same group).</summary>
    /// <param name="color">The border color to draw. If this is <c>null</c>, no border will be drawn.</param>
    /// <returns>The builder instance for chaining.</returns>
    ITileGroupBuilder SetOuterBorderColor(Color? color);
}
