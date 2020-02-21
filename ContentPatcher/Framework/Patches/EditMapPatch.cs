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
        /// <param name="logName">A unique name for this patch shown in log messages.</param>
        /// <param name="contentPack">The content pack which requested the patch.</param>
        /// <param name="assetName">The normalized asset name to intercept.</param>
        /// <param name="conditions">The conditions which determine whether this patch should be applied.</param>
        /// <param name="fromAsset">The asset key to load from the content pack instead.</param>
        /// <param name="fromArea">The map area from which to read tiles.</param>
        /// <param name="toArea">The map area to overwrite.</param>
        /// <param name="mapProperties">The map properties to change when editing a map, if any.</param>
        /// <param name="mapTiles">The map tiles to change when editing a map.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="normalizeAssetName">Normalize an asset name.</param>
        public EditMapPatch(string logName, ManagedContentPack contentPack, IManagedTokenString assetName, IEnumerable<Condition> conditions, IManagedTokenString fromAsset, TokenRectangle fromArea, TokenRectangle toArea, IEnumerable<EditMapPatchProperty> mapProperties, IEnumerable<EditMapPatchTile> mapTiles, IMonitor monitor, Func<string, string> normalizeAssetName)
            : base(logName, PatchType.EditMap, contentPack, assetName, conditions, normalizeAssetName, fromAsset: fromAsset)
        {
            this.FromArea = fromArea;
            this.ToArea = toArea;
            this.MapProperties = mapProperties?.ToArray() ?? new EditMapPatchProperty[0];
            this.MapTiles = mapTiles?.ToArray() ?? new EditMapPatchTile[0];
            this.Monitor = monitor;

            this.Contextuals
                .Add(fromArea)
                .Add(toArea);
        }

        /// <summary>Apply the patch to a loaded asset.</summary>
        /// <typeparam name="T">The asset type.</typeparam>
        /// <param name="asset">The asset to edit.</param>
        public override void Edit<T>(IAssetData asset)
        {
            string errorPrefix = $"Can't apply map patch \"{this.LogName}\" to {this.TargetAsset}";

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
            Map target = asset.GetData<Map>();

            // apply map area patch
            if (this.AppliesMapPatch)
            {
                Map source = this.ContentPack.Load<Map>(this.FromAsset);
                if (!this.TryApplyMapPatch(source, target, out string error))
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

        /// <summary>Get a human-readable list of changes applied to the asset for display when troubleshooting.</summary>
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
        /// <param name="target">The target map to overlay.</param>
        /// <param name="error">An error indicating why applying the patch failed, if applicable.</param>
        /// <returns>Returns whether applying the patch succeeded.</returns>
        private bool TryApplyMapPatch(Map source, Map target, out string error)
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
            Point sourceMapSize = new Point(source.Layers.Max(p => p.LayerWidth), source.Layers.Max(p => p.LayerHeight));
            Point targetMapSize = new Point(target.Layers.Max(p => p.LayerWidth), target.Layers.Max(p => p.LayerHeight));

            if (!this.TryValidateArea(sourceArea, sourceMapSize, "source", out error))
                return this.Fail(error, out error);
            if (!this.TryValidateArea(targetArea, targetMapSize, "target", out error))
                return this.Fail(error, out error);
            if (sourceArea.Width != targetArea.Width || sourceArea.Height != targetArea.Height)
                return this.Fail($"{sourceAreaLabel} size (Width:{sourceArea.Width}, Height:{sourceArea.Height}) doesn't match {targetAreaLabel} size (Width:{targetArea.Width}, Height:{targetArea.Height}).", out error);

            // apply source map
            this.ApplyMapOverride(source, sourceArea, target, targetArea);

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
            if ((removeTile || original == null) && (setTilesheet == null || setIndex == null))
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
        /// <param name="maxSize">The maximum map size.</param>
        /// <param name="name">The label for the area (e.g. 'source' or 'target').</param>
        /// <param name="error">An error indicating why parsing failed, if applicable.</param>
        /// <returns>Returns whether parsing the tile succeeded.</returns>
        private bool TryValidateArea(Rectangle area, Point maxSize, string name, out string error)
        {
            string errorPrefix = $"{name} area (X:{area.X}, Y:{area.Y}, Width:{area.Width}, Height:{area.Height})";

            if (area.X < 0 || area.Y < 0 || area.Width < 0 || area.Height < 0)
                return this.Fail($"{errorPrefix} has negative values, which isn't valid.", out error);

            if (area.Right > maxSize.X || area.Bottom > maxSize.Y)
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

        /// <summary>Copy the tiles from one map onto another. This assumes the input arguments have already been validated for correctness.</summary>
        /// <param name="source">The map from which to copy tiles.</param>
        /// <param name="sourceArea">The tile area within the source map to copy.</param>
        /// <param name="targetMap">The map onto which to copy tiles.</param>
        /// <param name="targetArea">The tile area within the target map to overwrite.</param>
        /// <remarks>Derived from <see cref="GameLocation.ApplyMapOverride"/> with a few changes:
        /// - added support for source/target areas;
        /// - added disambiguation if source has a modified version of the same tilesheet, instead of copying tiles into the target tilesheet;
        /// - changed to always overwrite tiles within the target area (to avoid edge cases where some tiles are only partly applied);
        /// - fixed copying tilesheets (avoid "The specified TileSheet was not created for use with this map" error);
        /// - fixed tilesheets not added at the end (via z_ prefix), which can cause crashes in game code which depends on hardcoded tilesheet indexes;
        /// - fixed issue where different tilesheets are linked by ID.
        /// </remarks>
        private void ApplyMapOverride(Map source, Rectangle sourceArea, Map targetMap, Rectangle targetArea)
        {
            // apply tilesheets
            IDictionary<TileSheet, TileSheet> tilesheetMap = new Dictionary<TileSheet, TileSheet>();
            foreach (TileSheet sourceSheet in source.TileSheets)
            {
                // copy tilesheets
                TileSheet targetSheet = targetMap.GetTileSheet(sourceSheet.Id);
                if (targetSheet == null || targetSheet.ImageSource != sourceSheet.ImageSource)
                {
                    // change ID if needed so new tilesheets are added after vanilla ones (to avoid errors in hardcoded game logic)
                    string id = sourceSheet.Id;
                    if (!id.StartsWith("z_", StringComparison.InvariantCultureIgnoreCase))
                        id = $"z_{id}";

                    // change ID if it conflicts with an existing tilesheet
                    if (targetMap.GetTileSheet(id) != null)
                    {
                        int disambiguator = Enumerable.Range(2, int.MaxValue - 1).First(p => targetMap.GetTileSheet($"{id}_{p}") == null);
                        id = $"{id}_{disambiguator}";
                    }

                    // add tilesheet
                    targetSheet = new TileSheet(id, targetMap, sourceSheet.ImageSource, sourceSheet.SheetSize, sourceSheet.TileSize);
                    for (int i = 0, tileCount = sourceSheet.TileCount; i < tileCount; ++i)
                        targetSheet.TileIndexProperties[i].CopyFrom(sourceSheet.TileIndexProperties[i]);
                    targetMap.AddTileSheet(targetSheet);
                }

                tilesheetMap[sourceSheet] = targetSheet;
            }

            // get layer map
            IDictionary<Layer, Layer> layerMap = source.Layers.ToDictionary(p => p, p => targetMap.GetLayer(p.Id));

            // apply tiles
            for (int x = 0; x < sourceArea.Width; x++)
            {
                for (int y = 0; y < sourceArea.Height; y++)
                {
                    // calculate tile positions
                    Point sourcePos = new Point(sourceArea.X + x, sourceArea.Y + y);
                    Point targetPos = new Point(targetArea.X + x, targetArea.Y + y);

                    // copy tiles and add layer if not present in target map
                    foreach (Layer sourceLayer in source.Layers)
                    {
                        Layer targetLayer = layerMap[sourceLayer];
                        if (targetLayer == null)
                        {
                            targetMap.AddLayer(targetLayer = new Layer(sourceLayer.Id, targetMap, targetMap.Layers[0].LayerSize, Layer.m_tileSize));
                            layerMap[sourceLayer] = targetMap.GetLayer(sourceLayer.Id);
                        }

                        foreach (var prop in sourceLayer.Properties)
                            if (!targetLayer.Properties.ContainsKey(prop.Key))
                                targetLayer.Properties.Add(prop);
                            else
                                targetLayer.Properties[prop.Key] = prop.Value;
                                
                        Tile sourceTile = sourceLayer.Tiles[sourcePos.X, sourcePos.Y];
                        Tile targetTile;
                        switch (sourceTile)
                        {
                            case StaticTile _:
                                targetTile = new StaticTile(targetLayer, tilesheetMap[sourceTile.TileSheet], sourceTile.BlendMode, sourceTile.TileIndex);
                                break;

                            case AnimatedTile animatedTile:
                                {
                                    StaticTile[] tileFrames = new StaticTile[animatedTile.TileFrames.Length];
                                    for (int frame = 0; frame < animatedTile.TileFrames.Length; ++frame)
                                    {
                                        StaticTile frameTile = animatedTile.TileFrames[frame];
                                        tileFrames[frame] = new StaticTile(targetLayer, tilesheetMap[frameTile.TileSheet], frameTile.BlendMode, frameTile.TileIndex);
                                    }
                                    targetTile = new AnimatedTile(targetLayer, tileFrames, animatedTile.FrameInterval);
                                }
                                break;

                            default: // null or unhandled type
                                targetTile = null;
                                break;
                        }
                        targetTile?.Properties.CopyFrom(sourceTile.Properties);
                        targetLayer.Tiles[targetPos.X, targetPos.Y] = targetTile;
                    }
                }
            }
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
