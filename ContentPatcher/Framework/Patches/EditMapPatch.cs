using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Tokens;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using xTile;
using xTile.Layers;
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

        /// <summary>The map properties to change when editing a map.</summary>
        private readonly EditMapPatchProperty[] MapProperties;

        /// <summary>The map tiles to change when editing a map.</summary>
        private readonly EditMapPatchTile[] MapTiles;

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
        /// <param name="toArea">The map area to overwrite.</param>
        /// <param name="mapProperties">The map properties to change when editing a map, if any.</param>
        /// <param name="mapTiles">The map tiles to change when editing a map.</param>
        /// <param name="updateRate">When the patch should be updated.</param>
        /// <param name="contentPack">The content pack which requested the patch.</param>
        /// <param name="parentPatch">The parent patch for which this patch was loaded, if any.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="reflection">Simplifies access to private code.</param>
        /// <param name="normalizeAssetName">Normalize an asset name.</param>
        public EditMapPatch(LogPathBuilder path, IManagedTokenString assetName, IEnumerable<Condition> conditions, IManagedTokenString fromAsset, TokenRectangle fromArea, TokenRectangle toArea, IEnumerable<EditMapPatchProperty> mapProperties, IEnumerable<EditMapPatchTile> mapTiles, UpdateRate updateRate, ManagedContentPack contentPack, IPatch parentPatch, IMonitor monitor, IReflectionHelper reflection, Func<string, string> normalizeAssetName)
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
            this.MapProperties = mapProperties?.ToArray() ?? new EditMapPatchProperty[0];
            this.MapTiles = mapTiles?.ToArray() ?? new EditMapPatchTile[0];
            this.Monitor = monitor;
            this.Reflection = reflection;

            this.Contextuals
                .Add(this.FromArea)
                .Add(this.ToArea)
                .Add(this.MapProperties)
                .Add(this.MapTiles);
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
                Map source = this.ContentPack.Load<Map>(this.FromAsset);
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
                        this.Monitor.Log($"{errorPrefix}: {nameof(PatchConfig.MapTiles)} > entry {i + 1} couldn't be applied: {error}", LogLevel.Warn);
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
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetChangeLabels()
        {
            if (this.AppliesMapPatch || this.AppliesTilePatches)
                yield return "patched map tiles";

            if (this.MapProperties.Any())
                yield return "changed map properties";
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
            targetAsset.PatchMap(source: source, sourceArea: sourceArea, targetArea: targetArea);

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
                    return this.Fail($"{nameof(PatchMapTileConfig.SetIndex)} specifies '{tile.SetIndex.Value}', which isn't a valid number.", out error);
                setIndex = parsed;
            }

            // position
            if (!tile.Position.TryGetLocation(out position, out error))
                return this.Fail($"{nameof(PatchMapTileConfig.Position)} specifies '{tile.Position.X.Value}, {tile.Position.Y.Value}', which isn't a valid position.", out error);

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
                    return this.Fail($"{nameof(PatchMapTileConfig.Remove)} specifies '{tile.Remove.Value}', which isn't a valid boolean value (must be 'true' or 'false').", out error);
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

        /// <summary>A utility method for returning false with an out error.</summary>
        /// <param name="inError">The error message.</param>
        /// <param name="outError">The input error.</param>
        /// <returns>Return false.</returns>
        private bool Fail(string inError, out string outError)
        {
            outError = inError;
            return false;
        }
    }
}
