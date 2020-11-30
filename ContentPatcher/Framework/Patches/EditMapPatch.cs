using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Constants;
using ContentPatcher.Framework.Tokens;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using xTile;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;
using Location = xTile.Dimensions.Location;
using Size = xTile.Dimensions.Size;

namespace ContentPatcher.Framework.Patches
{
    /// <summary>Metadata for a map asset to edit.</summary>
    internal class EditMapPatch : Patch
    {
        /*********
        ** Fields
        *********/
        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>Simplifies access to private code.</summary>
        private readonly IReflectionHelper Reflection;

        /// <summary>The map area from which to read tiles.</summary>
        private readonly TokenRectangle FromArea;

        /// <summary>The map area to overwrite.</summary>
        private readonly TokenRectangle ToArea;

        /// <summary>Indicates how the map should be patched.</summary>
        private readonly PatchMapMode PatchMode;

        /// <summary>The map properties to change when editing a map.</summary>
        private readonly EditMapPatchProperty[] MapProperties;

        /// <summary>The map tiles to change when editing a map.</summary>
        private readonly EditMapPatchTile[] MapTiles;

        /// <summary>The text operations to apply to existing values.</summary>
        private readonly TextOperation[] TextOperations;

        /// <summary>Whether the patch applies a map patch.</summary>
        private bool AppliesMapPatch => this.RawFromAsset != null;

        /// <summary>Whether the patch makes changes to individual tiles.</summary>
        private bool AppliesTilePatches => this.MapTiles.Any();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="path">The path to the patch from the root content file.</param>
        /// <param name="assetName">The normalized asset name to intercept.</param>
        /// <param name="conditions">The conditions which determine whether this patch should be applied.</param>
        /// <param name="fromAsset">The asset key to load from the content pack instead.</param>
        /// <param name="fromArea">The map area from which to read tiles.</param>
        /// <param name="patchMode">Indicates how the map should be patched.</param>
        /// <param name="toArea">The map area to overwrite.</param>
        /// <param name="mapProperties">The map properties to change when editing a map, if any.</param>
        /// <param name="textOperations">The text operations to apply to existing values.</param>
        /// <param name="mapTiles">The map tiles to change when editing a map.</param>
        /// <param name="updateRate">When the patch should be updated.</param>
        /// <param name="contentPack">The content pack which requested the patch.</param>
        /// <param name="parentPatch">The parent patch for which this patch was loaded, if any.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="reflection">Simplifies access to private code.</param>
        /// <param name="normalizeAssetName">Normalize an asset name.</param>
        public EditMapPatch(LogPathBuilder path, IManagedTokenString assetName, IEnumerable<Condition> conditions, IManagedTokenString fromAsset, TokenRectangle fromArea, TokenRectangle toArea, PatchMapMode patchMode, IEnumerable<EditMapPatchProperty> mapProperties, IEnumerable<EditMapPatchTile> mapTiles, IEnumerable<TextOperation> textOperations, UpdateRate updateRate, IContentPack contentPack, IPatch parentPatch, IMonitor monitor, IReflectionHelper reflection, Func<string, string> normalizeAssetName)
            : base(
                path: path,
                type: PatchType.EditMap,
                assetName: assetName,
                fromAsset: fromAsset,
                conditions: conditions,
                updateRate: updateRate,
                contentPack: contentPack,
                parentPatch: parentPatch,
                normalizeAssetName: normalizeAssetName
            )
        {
            this.FromArea = fromArea;
            this.ToArea = toArea;
            this.PatchMode = patchMode;
            this.MapProperties = mapProperties?.ToArray() ?? new EditMapPatchProperty[0];
            this.MapTiles = mapTiles?.ToArray() ?? new EditMapPatchTile[0];
            this.TextOperations = textOperations?.ToArray() ?? new TextOperation[0];
            this.Monitor = monitor;
            this.Reflection = reflection;

            this.Contextuals
                .Add(this.FromArea)
                .Add(this.ToArea)
                .Add(this.MapProperties)
                .Add(this.MapTiles)
                .Add(this.TextOperations);
        }

        /// <inheritdoc />
        public override void Edit<T>(IAssetData asset)
        {
            string errorPrefix = $"Can't apply map patch \"{this.Path}\" to {this.TargetAsset}";

            // validate
            if (typeof(T) != typeof(Map))
            {
                this.Monitor.Log($"{errorPrefix}: this file isn't a map file (found {typeof(T)}).", LogLevel.Warn);
                return;
            }
            if (this.AppliesMapPatch && !this.FromAssetExists())
            {
                this.Monitor.Log($"{errorPrefix}: the {nameof(PatchConfig.FromFile)} file '{this.FromAsset}' doesn't exist.", LogLevel.Warn);
                return;
            }

            // get map
            IAssetDataForMap targetAsset = asset.AsMap();
            Map target = targetAsset.Data;

            // apply map area patch
            if (this.AppliesMapPatch)
            {
                Map source = this.ContentPack.LoadAsset<Map>(this.FromAsset);
                if (!this.TryApplyMapPatch(source, targetAsset, out string error))
                    this.Monitor.Log($"{errorPrefix}: map patch couldn't be applied: {error}", LogLevel.Warn);
            }

            // patch map tiles
            if (this.AppliesTilePatches)
            {
                int i = 0;
                foreach (EditMapPatchTile tilePatch in this.MapTiles)
                {
                    i++;
                    if (!this.TryApplyTile(target, tilePatch, out string error))
                        this.Monitor.Log($"{errorPrefix}: {nameof(PatchConfig.MapTiles)} > entry {i} couldn't be applied: {error}", LogLevel.Warn);
                }
            }

            // patch map properties
            foreach (EditMapPatchProperty property in this.MapProperties)
            {
                string key = property.Key.Value;
                string value = property.Value.Value;

                if (value == null)
                    target.Properties.Remove(key);
                else
                    target.Properties[key] = value;
            }

            // apply text operations
            for (int i = 0; i < this.TextOperations.Length; i++)
            {
                if (!this.TryApplyTextOperation(target, this.TextOperations[i], out string error))
                    this.Monitor.Log($"{errorPrefix}: {nameof(PatchConfig.TextOperations)} > entry {i} couldn't be applied: {error}", LogLevel.Warn);
            }
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetChangeLabels()
        {
            if (this.AppliesMapPatch || this.AppliesTilePatches)
                yield return "patched map tiles";

            if (this.MapProperties.Any())
                yield return "changed map properties";

            if (this.TextOperations.Any())
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
        private bool TryApplyMapPatch(Map source, IAssetDataForMap targetAsset, out string error)
        {
            Map target = targetAsset.Data;

            // read data
            Rectangle mapBounds = this.GetMapArea(source);
            if (!this.TryReadArea(this.FromArea, 0, 0, mapBounds.Width, mapBounds.Height, out Rectangle sourceArea, out error))
                return this.Fail($"the source area is invalid: {error}.", out error);
            if (!this.TryReadArea(this.ToArea, 0, 0, sourceArea.Width, sourceArea.Height, out Rectangle targetArea, out error))
                return this.Fail($"the target area is invalid: {error}.", out error);

            // validate area values
            string sourceAreaLabel = this.FromArea != null ? $"{nameof(this.FromArea)}" : "source map";
            string targetAreaLabel = this.ToArea != null ? $"{nameof(this.ToArea)}" : "target map";
            Point sourceMapSize = new Point(source.Layers.Max(p => p.LayerWidth), source.Layers.Max(p => p.LayerHeight));

            if (!this.TryValidateArea(sourceArea, sourceMapSize, "source", out error))
                return this.Fail(error, out error);
            if (!this.TryValidateArea(targetArea, null, "target", out error))
                return this.Fail(error, out error);
            if (sourceArea.Width != targetArea.Width || sourceArea.Height != targetArea.Height)
                return this.Fail($"{sourceAreaLabel} size (Width:{sourceArea.Width}, Height:{sourceArea.Height}) doesn't match {targetAreaLabel} size (Width:{targetArea.Width}, Height:{targetArea.Height}).", out error);

            // apply source map
            this.ExtendMap(target, minWidth: targetArea.Right, minHeight: targetArea.Bottom);
            this.PatchMap(targetAsset, source: source, patchMode: this.PatchMode, sourceArea: sourceArea, targetArea: targetArea);

            error = null;
            return true;
        }

        /// <summary>Try to apply a map tile patch.</summary>
        /// <param name="map">The target map to patch.</param>
        /// <param name="tilePatch">The tile patch info.</param>
        /// <param name="error">An error indicating why applying the patch failed, if applicable.</param>
        /// <returns>Returns whether applying the patch succeeded.</returns>
        private bool TryApplyTile(Map map, EditMapPatchTile tilePatch, out string error)
        {
            // parse tile data
            if (!this.TryReadTile(tilePatch, out string layerName, out Location position, out int? setIndex, out string setTilesheetId, out IDictionary<string, string> setProperties, out bool removeTile, out error))
                return this.Fail(error, out error);
            bool hasEdits = setIndex != null || setTilesheetId != null || setProperties.Any();

            // get layer
            var layer = map.GetLayer(layerName);
            if (layer == null)
                return this.Fail($"{nameof(PatchMapTileConfig.Layer)} specifies a '{layerName}' layer which doesn't exist.", out error);

            // get tilesheet
            TileSheet setTilesheet = null;
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
            if (setTilesheet != null || setIndex != null || setProperties.Any())
            {
                var tile = new StaticTile(layer, setTilesheet ?? original.TileSheet, original?.BlendMode ?? BlendMode.Alpha, setIndex ?? original.TileIndex);
                foreach (var pair in setProperties)
                    tile.Properties[pair.Key] = pair.Value;
                layer.Tiles[position] = tile;
            }

            error = null;
            return true;
        }

        /// <summary>Try to apply a text operation.</summary>
        /// <param name="target">The target map to change.</param>
        /// <param name="operation">The text operation to apply.</param>
        /// <param name="error">An error indicating why applying the operation failed, if applicable.</param>
        /// <returns>Returns whether applying the operation succeeded.</returns>
        private bool TryApplyTextOperation(Map target, TextOperation operation, out string error)
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
                        string key = operation.Target[1].Value;
                        string value = target.Properties.TryGetValue(key, out PropertyValue property)
                            ? property.ToString()
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
        private bool TryReadTile(EditMapPatchTile tile, out string layerName, out Location position, out int? setIndex, out string setTilesheetId, out IDictionary<string, string> properties, out bool remove, out string error)
        {
            // init
            layerName = null;
            setIndex = null;
            setTilesheetId = null;
            position = Location.Origin;
            properties = new Dictionary<string, string>();
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
            if (tile.SetProperties != null)
            {
                foreach (var pair in tile.SetProperties)
                    properties[pair.Key.Value] = pair.Value.Value;
            }

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
        private bool TryValidateArea(Rectangle area, Point? maxSize, string name, out string error)
        {
            string errorPrefix = $"{name} area (X:{area.X}, Y:{area.Y}, Width:{area.Width}, Height:{area.Height})";

            if (area.X < 0 || area.Y < 0 || area.Width < 0 || area.Height < 0)
                return this.Fail($"{errorPrefix} has negative values, which isn't valid.", out error);

            if (maxSize.HasValue && (area.Right > maxSize.Value.X || area.Bottom > maxSize.Value.Y))
                return this.Fail($"{errorPrefix} extends past the edges of the {name} map, which isn't allowed.", out error);

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

        /// <summary>Extend the map if needed to fit the given size. Note that this is an expensive operation.</summary>
        /// <param name="map">The map to resize.</param>
        /// <param name="minWidth">The minimum map width in tiles.</param>
        /// <param name="minHeight">The minimum map height in tiles.</param>
        /// <returns>Whether the map was resized.</returns>
        private bool ExtendMap(Map map, int minWidth, int minHeight)
        {
            bool resized = false;

            // resize layers
            foreach (Layer layer in map.Layers)
            {
                // check if resize needed
                if (layer.LayerWidth >= minWidth && layer.LayerHeight >= minHeight)
                    continue;
                resized = true;

                // build new tile matrix
                int width = Math.Max(minWidth, layer.LayerWidth);
                int height = Math.Max(minHeight, layer.LayerHeight);
                Tile[,] tiles = new Tile[width, height];
                for (int x = 0; x < layer.LayerWidth; x++)
                {
                    for (int y = 0; y < layer.LayerHeight; y++)
                        tiles[x, y] = layer.Tiles[x, y];
                }

                // update fields
                this.Reflection.GetField<Tile[,]>(layer, "m_tiles").SetValue(tiles);
                this.Reflection.GetField<TileArray>(layer, "m_tileArray").SetValue(new TileArray(layer, tiles));
                this.Reflection.GetField<Size>(layer, "m_layerSize").SetValue(new Size(width, height));
            }

            // resize map
            if (resized)
                this.Reflection.GetMethod(map, "UpdateDisplaySize").Invoke();

            return resized;
        }

        /// <summary>Copy layers, tiles, and tilesheets from another map onto the asset.</summary>
        /// <param name="asset">The asset being edited.</param>
        /// <param name="source">The map from which to copy.</param>
        /// <param name="patchMode">Indicates how the map should be patched.</param>
        /// <param name="sourceArea">The tile area within the source map to copy, or <c>null</c> for the entire source map size. This must be within the bounds of the <paramref name="source"/> map.</param>
        /// <param name="targetArea">The tile area within the target map to overwrite, or <c>null</c> to patch the whole map. The original content within this area will be erased. This must be within the bounds of the existing map.</param>
        /// <remarks>
        /// This is temporarily duplicated from SMAPI's <see cref="IAssetDataForMap"/>, to add map overlay support before the feature is added to SMAPI.
        /// </remarks>
        public void PatchMap(IAssetDataForMap asset, Map source, PatchMapMode patchMode, Rectangle? sourceArea = null, Rectangle? targetArea = null)
        {
            Map target = asset.Data;

            // get areas
            {
                Rectangle sourceBounds = this.GetMapArea(source);
                Rectangle targetBounds = this.GetMapArea(target);
                sourceArea ??= new Rectangle(0, 0, sourceBounds.Width, sourceBounds.Height);
                targetArea ??= new Rectangle(0, 0, Math.Min(sourceArea.Value.Width, targetBounds.Width), Math.Min(sourceArea.Value.Height, targetBounds.Height));

                // validate
                if (sourceArea.Value.X < 0 || sourceArea.Value.Y < 0 || sourceArea.Value.Right > sourceBounds.Width || sourceArea.Value.Bottom > sourceBounds.Height)
                    throw new ArgumentOutOfRangeException(nameof(sourceArea), $"The source area ({sourceArea}) is outside the bounds of the source map ({sourceBounds}).");
                if (targetArea.Value.X < 0 || targetArea.Value.Y < 0 || targetArea.Value.Right > targetBounds.Width || targetArea.Value.Bottom > targetBounds.Height)
                    throw new ArgumentOutOfRangeException(nameof(targetArea), $"The target area ({targetArea}) is outside the bounds of the target map ({targetBounds}).");
                if (sourceArea.Value.Width != targetArea.Value.Width || sourceArea.Value.Height != targetArea.Value.Height)
                    throw new InvalidOperationException($"The source area ({sourceArea}) and target area ({targetArea}) must be the same size.");
            }

            // apply tilesheets
            IDictionary<TileSheet, TileSheet> tilesheetMap = new Dictionary<TileSheet, TileSheet>();
            foreach (TileSheet sourceSheet in source.TileSheets)
            {
                // copy tilesheets
                TileSheet targetSheet = target.GetTileSheet(sourceSheet.Id);
                if (targetSheet == null || this.NormalizeTilesheetPathForComparison(targetSheet.ImageSource) != this.NormalizeTilesheetPathForComparison(sourceSheet.ImageSource))
                {
                    // change ID if needed so new tilesheets are added after vanilla ones (to avoid errors in hardcoded game logic)
                    string id = sourceSheet.Id;
                    if (!id.StartsWith("z_", StringComparison.OrdinalIgnoreCase))
                        id = $"z_{id}";

                    // change ID if it conflicts with an existing tilesheet
                    if (target.GetTileSheet(id) != null)
                    {
                        int disambiguator = Enumerable.Range(2, int.MaxValue - 1).First(p => target.GetTileSheet($"{id}_{p}") == null);
                        id = $"{id}_{disambiguator}";
                    }

                    // add tilesheet
                    targetSheet = new TileSheet(id, target, sourceSheet.ImageSource, sourceSheet.SheetSize, sourceSheet.TileSize);
                    for (int i = 0, tileCount = sourceSheet.TileCount; i < tileCount; ++i)
                        targetSheet.TileIndexProperties[i].CopyFrom(sourceSheet.TileIndexProperties[i]);
                    target.AddTileSheet(targetSheet);
                }

                tilesheetMap[sourceSheet] = targetSheet;
            }

            // get target layers
            IDictionary<Layer, Layer> sourceToTargetLayers = source.Layers.ToDictionary(p => p, p => target.GetLayer(p.Id));
            HashSet<Layer> orphanedTargetLayers = new HashSet<Layer>(target.Layers.Except(sourceToTargetLayers.Values));

            // apply tiles
            bool replaceAll = patchMode == PatchMapMode.Replace;
            bool replaceByLayer = patchMode == PatchMapMode.ReplaceByLayer;
            for (int x = 0; x < sourceArea.Value.Width; x++)
            {
                for (int y = 0; y < sourceArea.Value.Height; y++)
                {
                    // calculate tile positions
                    Point sourcePos = new Point(sourceArea.Value.X + x, sourceArea.Value.Y + y);
                    Point targetPos = new Point(targetArea.Value.X + x, targetArea.Value.Y + y);

                    // replace tiles on target-only layers
                    if (replaceAll)
                    {
                        foreach (Layer targetLayer in orphanedTargetLayers)
                            targetLayer.Tiles[targetPos.X, targetPos.Y] = null;
                    }

                    // merge layers
                    foreach (Layer sourceLayer in source.Layers)
                    {
                        // get layer
                        Layer targetLayer = sourceToTargetLayers[sourceLayer];
                        if (targetLayer == null)
                        {
                            target.AddLayer(targetLayer = new Layer(sourceLayer.Id, target, target.Layers[0].LayerSize, Layer.m_tileSize));
                            sourceToTargetLayers[sourceLayer] = target.GetLayer(sourceLayer.Id);
                        }

                        // copy layer properties
                        targetLayer.Properties.CopyFrom(sourceLayer.Properties);

                        // create new tile
                        Tile sourceTile = sourceLayer.Tiles[sourcePos.X, sourcePos.Y];
                        Tile newTile = sourceTile != null
                            ? this.CreateTile(sourceTile, targetLayer, tilesheetMap[sourceTile.TileSheet])
                            : null;
                        newTile?.Properties.CopyFrom(sourceTile.Properties);

                        // replace tile
                        if (newTile != null || replaceByLayer || replaceAll)
                            targetLayer.Tiles[targetPos.X, targetPos.Y] = newTile;
                    }
                }
            }
        }

        /// <summary>Create a new tile for the target map.</summary>
        /// <param name="sourceTile">The source tile to copy.</param>
        /// <param name="targetLayer">The target layer.</param>
        /// <param name="targetSheet">The target tilesheet.</param>
        private Tile CreateTile(Tile sourceTile, Layer targetLayer, TileSheet targetSheet)
        {
            switch (sourceTile)
            {
                case StaticTile _:
                    return new StaticTile(targetLayer, targetSheet, sourceTile.BlendMode, sourceTile.TileIndex);

                case AnimatedTile animatedTile:
                    {
                        StaticTile[] tileFrames = new StaticTile[animatedTile.TileFrames.Length];
                        for (int frame = 0; frame < animatedTile.TileFrames.Length; ++frame)
                        {
                            StaticTile frameTile = animatedTile.TileFrames[frame];
                            tileFrames[frame] = new StaticTile(targetLayer, targetSheet, frameTile.BlendMode, frameTile.TileIndex);
                        }

                        return new AnimatedTile(targetLayer, tileFrames, animatedTile.FrameInterval);
                    }

                default: // null or unhandled type
                    return null;
            }
        }

        /// <summary>Normalize a map tilesheet path for comparison. This value should *not* be used as the actual tilesheet path.</summary>
        /// <param name="path">The path to normalize.</param>
        private string NormalizeTilesheetPathForComparison(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            path = PathUtilities.NormalizePath(path);
            if (path.StartsWith($"Maps{System.IO.Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
                path = path.Substring($"Maps{System.IO.Path.DirectorySeparatorChar}".Length);
            if (path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                path = path.Substring(0, path.Length - 4);

            return path;
        }
    }
}
