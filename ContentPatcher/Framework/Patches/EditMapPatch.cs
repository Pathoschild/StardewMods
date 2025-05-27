using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Constants;
using ContentPatcher.Framework.Migrations;
using ContentPatcher.Framework.TextOperations;
using ContentPatcher.Framework.Tokens;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Extensions;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace ContentPatcher.Framework.Patches;

/// <summary>Metadata for a map asset to edit.</summary>
internal class EditMapPatch : Patch
{
    /*********
    ** Fields
    *********/
    /// <summary>Encapsulates monitoring and logging.</summary>
    private readonly IMonitor Monitor;

    /// <summary>The map area from which to read tiles.</summary>
    private readonly TokenRectangle? FromArea;

    /// <summary>The map area to overwrite.</summary>
    private readonly TokenRectangle? ToArea;

    /// <summary>Indicates how the map should be patched.</summary>
    private readonly PatchMapMode PatchMode;

    /// <summary>The map properties to change when editing a map.</summary>
    private readonly EditMapPatchProperty[] MapProperties;

    /// <summary>The map tiles to change when editing a map.</summary>
    private readonly EditMapPatchTile[] MapTiles;

    /// <summary>The NPC-only warps that should be added to the location.</summary>
    public readonly IManagedTokenString[] AddNpcWarps;

    /// <summary>The warps that should be added to the location.</summary>
    public readonly IManagedTokenString[] AddWarps;

    /// <summary>The text operations to apply to existing values.</summary>
    private readonly ITextOperation[] TextOperations;

    /// <summary>Whether the patch applies a map patch.</summary>
    private bool AppliesMapPatch => this.RawFromAsset != null;

    /// <summary>Whether the patch makes changes to individual tiles.</summary>
    private bool AppliesTilePatches => this.MapTiles.Length > 0;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="indexPath">The path of indexes from the root <c>content.json</c> to this patch; see <see cref="IPatch.IndexPath"/>.</param>
    /// <param name="path">The path to the patch from the root content file.</param>
    /// <param name="assetName">The normalized asset name to intercept.</param>
    /// <param name="assetLocale">The locale code in the target asset's name to match. See <see cref="IPatch.TargetAsset"/> for more info.</param>
    /// <param name="priority">The priority for this patch when multiple patches apply.</param>
    /// <param name="conditions">The conditions which determine whether this patch should be applied.</param>
    /// <param name="fromAsset">The asset key to load from the content pack instead.</param>
    /// <param name="fromArea">The map area from which to read tiles.</param>
    /// <param name="toArea">The map area to overwrite.</param>
    /// <param name="patchMode">Indicates how the map should be patched.</param>
    /// <param name="mapProperties">The map properties to change when editing a map, if any.</param>
    /// <param name="mapTiles">The map tiles to change when editing a map.</param>
    /// <param name="addNpcWarps">The NPC-only warps to add to the location.</param>
    /// <param name="addWarps">The warps to add to the location.</param>
    /// <param name="textOperations">The text operations to apply to existing values.</param>
    /// <param name="updateRate">When the patch should be updated.</param>
    /// <param name="inheritedLocalTokens">The local token values inherited from the parent patch if applicable, to use in addition to the pre-existing tokens.</param>
    /// <param name="localTokens">The local token values defined directly on this patch, to use in addition to the pre-existing tokens.</param>
    /// <param name="contentPack">The content pack which requested the patch.</param>
    /// <param name="migrator">The aggregate migration which applies for this patch.</param>
    /// <param name="parentPatch">The parent patch for which this patch was loaded, if any.</param>
    /// <param name="monitor">Encapsulates monitoring and logging.</param>
    /// <param name="parseAssetName">Parse an asset name.</param>
    public EditMapPatch(int[] indexPath, LogPathBuilder path, IManagedTokenString assetName, IManagedTokenString? assetLocale, AssetEditPriority priority, IEnumerable<Condition> conditions, IManagedTokenString? fromAsset, TokenRectangle? fromArea, TokenRectangle? toArea, PatchMapMode patchMode, IEnumerable<EditMapPatchProperty>? mapProperties, IEnumerable<EditMapPatchTile>? mapTiles, IEnumerable<IManagedTokenString>? addNpcWarps, IEnumerable<IManagedTokenString>? addWarps, IEnumerable<ITextOperation>? textOperations, UpdateRate updateRate, InvariantDictionary<IManagedTokenString>? inheritedLocalTokens, InvariantDictionary<IManagedTokenString>? localTokens, IContentPack contentPack, IRuntimeMigration migrator, IPatch? parentPatch, IMonitor monitor, Func<string, IAssetName> parseAssetName)
        : base(
            indexPath: indexPath,
            path: path,
            type: PatchType.EditMap,
            assetName: assetName,
            assetLocale: assetLocale,
            priority: (int)priority,
            updateRate: updateRate,
            inheritedLocalTokens: inheritedLocalTokens,
            localTokens: localTokens,
            conditions: conditions,
            contentPack: contentPack,
            migrator: migrator,
            parentPatch: parentPatch,
            parseAssetName: parseAssetName,
            fromAsset: fromAsset
        )
    {
        this.FromArea = fromArea;
        this.ToArea = toArea;
        this.PatchMode = patchMode;
        this.MapProperties = mapProperties?.ToArray() ?? [];
        this.MapTiles = mapTiles?.ToArray() ?? [];
        this.AddNpcWarps = addNpcWarps?.Reverse().ToArray() ?? []; // reversing the warps allows later ones to 'overwrite' earlier ones, since the game checks them in the listed order
        this.AddWarps = addWarps?.Reverse().ToArray() ?? [];
        this.TextOperations = textOperations?.ToArray() ?? [];
        this.Monitor = monitor;

        this.Contextuals
            .Add(this.FromArea)
            .Add(this.ToArea)
            .Add(this.MapProperties)
            .Add(this.MapTiles)
            .Add(this.AddNpcWarps)
            .Add(this.AddWarps)
            .Add(this.TextOperations);
    }

    /// <inheritdoc />
    public override void Edit<T>(IAssetData asset)
    {
        // validate
        if (typeof(T) != typeof(Map))
        {
            this.WarnForPatch($"this file isn't a map file (found {typeof(T)}).");
            return;
        }
        if (this.AppliesMapPatch && !this.FromAssetExists())
        {
            this.WarnForPatch($"the {nameof(PatchConfig.FromFile)} file '{this.FromAsset}' doesn't exist.");
            return;
        }

        // get map
        IAssetDataForMap targetAsset = asset.AsMap();
        Map target = targetAsset.Data;

        // apply map area patch
        if (this.AppliesMapPatch)
        {
            Map source = this.ContentPack.ModContent.Load<Map>(this.FromAsset!);
            if (!this.TryApplyMapPatch(source, targetAsset, out string? error))
                this.WarnForPatch($"map patch couldn't be applied: {error}");
        }

        // patch map tiles
        if (this.AppliesTilePatches)
        {
            int i = 0;
            foreach (EditMapPatchTile tilePatch in this.MapTiles)
            {
                i++;
                if (!this.TryApplyTile(target, tilePatch, out string? error))
                    this.WarnForPatch($"{nameof(PatchConfig.MapTiles)} > entry {i} couldn't be applied: {error}");
            }
        }

        // patch map properties
        foreach (EditMapPatchProperty property in this.MapProperties)
        {
            string key = property.Key.Value!;
            string? value = property.Value?.Value;

            if (value == null)
                target.Properties.Remove(key);
            else
                target.Properties[key] = value;
        }

        // apply map NPC-only warps
        if (this.AddNpcWarps.Length > 0)
        {
            this.ApplyWarps(target, this.AddNpcWarps, out IDictionary<string, string> errors, "NPCWarp");
            foreach ((string warp, string error) in errors)
                this.WarnForPatch($"{nameof(PatchConfig.AddNpcWarps)} > warp '{warp}' couldn't be applied: {error}");
        }

        // apply map warps
        if (this.AddWarps.Length > 0)
        {
            this.ApplyWarps(target, this.AddWarps, out IDictionary<string, string> errors, "Warp");
            foreach ((string warp, string error) in errors)
                this.WarnForPatch($"{nameof(PatchConfig.AddWarps)} > warp '{warp}' couldn't be applied: {error}");
        }

        // apply text operations
        for (int i = 0; i < this.TextOperations.Length; i++)
        {
            if (!this.TryApplyTextOperation(target, this.TextOperations[i], out string? error))
                this.WarnForPatch($"{nameof(PatchConfig.TextOperations)} > entry {i} couldn't be applied: {error}");
        }
    }

    /// <inheritdoc />
    public override IEnumerable<string> GetChangeLabels()
    {
        if (this.AppliesMapPatch || this.AppliesTilePatches)
            yield return "patched map tiles";

        if (this.MapProperties.Length > 0 || this.AddNpcWarps.Length > 0 || this.AddWarps.Length > 0)
            yield return "changed map properties";

        if (this.TextOperations.Length > 0)
            yield return "applied text operations";
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Try to apply a map overlay patch.</summary>
    /// <param name="source">The source map to overlay.</param>
    /// <param name="targetAsset">The target map to overlay.</param>
    /// <param name="error">An error indicating why applying the patch failed, if applicable.</param>
    /// <returns>Returns whether applying the patch succeeded.</returns>
    private bool TryApplyMapPatch(Map source, IAssetDataForMap targetAsset, [NotNullWhen(false)] out string? error)
    {
        // read data
        Rectangle mapBounds = this.GetMapArea(source);
        if (!this.TryReadArea(this.FromArea, 0, 0, mapBounds.Width, mapBounds.Height, out Rectangle sourceArea, out error))
            return this.Fail($"the source area is invalid: {error}.", out error);
        if (!this.TryReadArea(this.ToArea, 0, 0, sourceArea.Width, sourceArea.Height, out Rectangle targetArea, out error))
            return this.Fail($"the target area is invalid: {error}.", out error);

        // validate area values
        string sourceAreaLabel = this.FromArea != null ? $"{nameof(this.FromArea)}" : "source map";
        string targetAreaLabel = this.ToArea != null ? $"{nameof(this.ToArea)}" : "target map";
        Point sourceMapSize = new(source.Layers.Max(p => p.LayerWidth), source.Layers.Max(p => p.LayerHeight));

        if (!this.TryValidateArea(sourceArea, sourceMapSize, "source", out error))
            return this.Fail(error, out error);
        if (!this.TryValidateArea(targetArea, null, "target", out error))
            return this.Fail(error, out error);
        if (sourceArea.Width != targetArea.Width || sourceArea.Height != targetArea.Height)
            return this.Fail($"{sourceAreaLabel} size (Width:{sourceArea.Width}, Height:{sourceArea.Height}) doesn't match {targetAreaLabel} size (Width:{targetArea.Width}, Height:{targetArea.Height}).", out error);

        // apply source map
        targetAsset.ExtendMap(minWidth: targetArea.Right, minHeight: targetArea.Bottom);
        targetAsset.PatchMap(source: source, sourceArea: sourceArea, targetArea: targetArea, patchMode: this.PatchMode);

        error = null;
        return true;
    }

    /// <summary>Try to apply a map tile patch.</summary>
    /// <param name="map">The target map to patch.</param>
    /// <param name="tilePatch">The tile patch info.</param>
    /// <param name="error">An error indicating why applying the patch failed, if applicable.</param>
    /// <returns>Returns whether applying the patch succeeded.</returns>
    private bool TryApplyTile(Map map, EditMapPatchTile tilePatch, [NotNullWhen(false)] out string? error)
    {
        // parse tile data
        if (!this.TryReadTile(tilePatch, out string? layerName, out Location position, out int? setIndex, out string? setTilesheetId, out Dictionary<string, string?> setProperties, out bool removeTile, out error))
            return this.Fail(error, out error);
        bool hasEdits = setIndex != null || setTilesheetId != null || setProperties.Count > 0;

        // get layer
        Layer? layer = map.GetLayer(layerName);
        if (layer == null)
            return this.Fail($"{nameof(PatchMapTileConfig.Layer)} specifies a '{layerName}' layer which doesn't exist.", out error);

        // get tilesheet
        TileSheet? setTilesheet = null;
        if (setTilesheetId != null)
        {
            setTilesheet = map.GetTileSheet(setTilesheetId);
            if (setTilesheet == null)
                return this.Fail($"{nameof(PatchMapTileConfig.SetTilesheet)} specifies a '{setTilesheetId}' tilesheet which doesn't exist.", out error);
        }

        // get original tile
        if (!layer.IsValidTileLocation(position))
            return this.Fail($"{nameof(PatchMapTileConfig.Position)} specifies a tile position '{position.X}, {position.Y}' which is outside the map area.", out error);
        Tile original = layer.Tiles[position];

        // if adding a new tile, the min tile info is required
        if (hasEdits && (removeTile || original == null) && (setTilesheet == null || setIndex == null))
            return this.Fail($"the map has no tile at {layerName} ({position.X}, {position.Y}). To add a tile, the {nameof(PatchMapTileConfig.SetTilesheet)} and {nameof(PatchMapTileConfig.SetIndex)} fields must be set.", out error);

        // apply new tile
        if (removeTile)
            layer.Tiles[position] = null;
        if (setTilesheet != null || setIndex != null || setProperties.Count > 0)
        {
            var tile = new StaticTile(layer, setTilesheet ?? original!.TileSheet, original?.BlendMode ?? BlendMode.Alpha, setIndex ?? original!.TileIndex);

            if (original?.Properties.Count > 0)
            {
                foreach ((string key, PropertyValue value) in original.Properties)
                    tile.Properties[key] = value;
            }

            foreach ((string key, string? value) in setProperties)
            {
                if (value == null)
                    tile.Properties.Remove(key);
                else
                    tile.Properties[key] = value;
            }

            layer.Tiles[position] = tile;
        }

        error = null;
        return true;
    }

    /// <summary>Add warps to the map.</summary>
    /// <param name="target">The target map to change.</param>
    /// <param name="addWarps">The warps to add.</param>
    /// <param name="errors">The errors indexed by warp string/</param>
    /// <param name="propertyName">The map property to edit.</param>
    private void ApplyWarps(Map target, IManagedTokenString[] addWarps, out IDictionary<string, string> errors, string propertyName)
    {
        errors = new InvariantDictionary<string>();

        // build new warp string
        List<string> validWarps = new(addWarps.Length);
        foreach (string? warp in addWarps.Select(p => p.Value))
        {
            if (!this.ValidateWarp(warp, out string? error))
            {
                errors[warp ?? "<null>"] = error;
                continue;
            }

            validWarps.Add(warp);
        }

        // prepend to map property
        if (validWarps.Count > 0)
        {
            string prevWarps = target.Properties.TryGetValue(propertyName, out string? rawWarps)
                ? rawWarps
                : "";
            string newWarps = string.Join(" ", validWarps);

            target.Properties[propertyName] = $"{newWarps} {prevWarps}".Trim(); // prepend so warps added later 'overwrite' in case of conflict
        }
    }

    /// <summary>Validate that a warp string is in the value format.</summary>
    /// <param name="warp">The raw warp string.</param>
    /// <param name="error">The error indicating why it's invalid, if applicable.</param>
    private bool ValidateWarp([NotNullWhen(true)] string? warp, [NotNullWhen(false)] out string? error)
    {
        // handle null
        if (warp == null)
            return this.Fail("warp cannot be null", out error);

        // check field count
        string[] parts = warp.Split(' ');
        if (parts.Length != 5)
            return this.Fail("must have exactly five fields in the form `fromX fromY toMap toX toY`.", out error);

        // check tile coordinates
        foreach (string part in new[] { parts[0], parts[1], parts[3], parts[4] })
        {
            if (!int.TryParse(part, out _) || part.Any(ch => !char.IsDigit(ch) && ch != '-')) // int.TryParse will allow strings like '\t14' which will crash the game
                return this.Fail($"can't parse '{part}' as a tile coordinate number.", out error);
        }

        // check map name
        if (parts[2].Trim().Length == 0)
            return this.Fail("the map value can't be blank.", out error);

        error = null;
        return true;
    }

    /// <summary>Try to apply a text operation.</summary>
    /// <param name="target">The target map to change.</param>
    /// <param name="operation">The text operation to apply.</param>
    /// <param name="error">An error indicating why applying the operation failed, if applicable.</param>
    /// <returns>Returns whether applying the operation succeeded.</returns>
    private bool TryApplyTextOperation(Map target, ITextOperation operation, [NotNullWhen(false)] out string? error)
    {
        var targetRoot = operation.GetTargetRoot();
        switch (targetRoot)
        {
            case TextOperationTargetRoot.MapProperties:
                {
                    // validate
                    if (operation.Target.Length > 2)
                        return this.Fail($"a '{TextOperationTargetRoot.MapProperties}' path must only have one other segment for the property name.", out error);

                    // get key/value
                    string key = operation.Target[1].Value!;
                    string? value = target.Properties.TryGetValue(key, out string? property)
                        ? property
                        : null;

                    // apply
                    target.Properties[key] = operation.Apply(value);
                }
                break;

            default:
                return this.Fail(
                    targetRoot == null
                        ? $"unknown path root '{operation.Target[0]}'."
                        : $"path root '{targetRoot}' isn't valid for an {nameof(PatchType.EditMap)} patch",
                    out error
                );
        }

        error = null;
        return true;
    }

    /// <summary>Try to read a map tile patch to apply.</summary>
    /// <param name="tile">The map tile patch.</param>
    /// <param name="layerName">The parsed layer name.</param>
    /// <param name="position">The parsed tile position.</param>
    /// <param name="setIndex">The parsed tile index.</param>
    /// <param name="setTilesheetId">The parsed tilesheet ID.</param>
    /// <param name="properties">The parsed tile properties.</param>
    /// <param name="remove">The parsed remove flag.</param>
    /// <param name="error">An error indicating why parsing failed, if applicable.</param>
    /// <returns>Returns whether parsing the tile succeeded.</returns>
    private bool TryReadTile(EditMapPatchTile tile, out string? layerName, out Location position, out int? setIndex, out string? setTilesheetId, out Dictionary<string, string?> properties, out bool remove, [NotNullWhen(false)] out string? error)
    {
        // init
        setIndex = null;
        position = Location.Origin;
        properties = [];
        remove = false;

        // layer & tilesheet
        layerName = tile.Layer?.Value;
        setTilesheetId = tile.SetTilesheet?.Value;

        // set index
        if (tile.SetIndex.IsMeaningful())
        {
            if (!int.TryParse(tile.SetIndex.Value, out int parsed))
                return this.Fail($"{nameof(PatchMapTileConfig.SetIndex)} specifies '{tile.SetIndex}', which isn't a valid number.", out error);
            setIndex = parsed;
        }

        // position
        if (!tile.Position.TryGetLocation(out position, out error))
            return this.Fail($"{nameof(PatchMapTileConfig.Position)} specifies '{tile.Position.X}, {tile.Position.Y}', which isn't a valid position.", out error);

        // tile properties
        foreach ((ITokenString key, ITokenString? value) in tile.SetProperties)
            properties[key.Value!] = value?.Value;

        // remove
        if (tile.Remove.IsMeaningful())
        {
            if (!bool.TryParse(tile.Remove.Value, out remove))
                return this.Fail($"{nameof(PatchMapTileConfig.Remove)} specifies '{tile.Remove}', which isn't a valid boolean value (must be 'true' or 'false').", out error);
        }

        error = null;
        return true;
    }

    /// <summary>Validate an area's values.</summary>
    /// <param name="area">The area to validate.</param>
    /// <param name="maxSize">The maximum map size, if any.</param>
    /// <param name="name">The label for the area (e.g. 'source' or 'target').</param>
    /// <param name="error">An error indicating why parsing failed, if applicable.</param>
    /// <returns>Returns whether parsing the tile succeeded.</returns>
    private bool TryValidateArea(Rectangle area, Point? maxSize, string name, [NotNullWhen(false)] out string? error)
    {
        if (area.X < 0 || area.Y < 0 || area.Width < 0 || area.Height < 0)
            return this.FailArea(area, name, "has negative values, which isn't valid.", out error);

        if (maxSize.HasValue && (area.Right > maxSize.Value.X || area.Bottom > maxSize.Value.Y))
            return this.FailArea(area, name, $"extends past the edges of the {name} map, which isn't allowed.", out error);

        error = null;
        return true;
    }

    /// <summary>Get a rectangle which encompasses all layers for a map.</summary>
    /// <param name="map">The map to check.</param>
    private Rectangle GetMapArea(Map map)
    {
        // get max map size
        int maxWidth = 0;
        int maxHeight = 0;
        foreach (Layer layer in map.Layers)
        {
            if (layer.LayerWidth > maxWidth)
                maxWidth = layer.LayerWidth;
            if (layer.LayerHeight > maxHeight)
                maxHeight = layer.LayerHeight;
        }

        return new Rectangle(0, 0, maxWidth, maxHeight);
    }

    /// <summary>A utility method for returning false with an out error specific to an area error.</summary>
    /// <param name="area">The area to validate.</param>
    /// <param name="name">The label for the area (e.g. 'source' or 'target').</param>
    /// <param name="inError">The error message.</param>
    /// <param name="outError">The input error.</param>
    /// <returns>Return false.</returns>
    private bool FailArea(Rectangle area, string name, string inError, out string outError)
    {
        return this.Fail($"{name} area (X:{area.X}, Y:{area.Y}, Width:{area.Width}, Height:{area.Height}) {inError}", out outError);
    }

    /// <summary>Log a warning for an issue when applying the patch.</summary>
    /// <param name="message">The message to log.</param>
    private void WarnForPatch(string message)
    {
        this.Monitor.Log($"Can't apply map patch \"{this.Path}\" to {this.TargetAsset}: {message}", LogLevel.Warn);
    }
}
