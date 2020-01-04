using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Tokens;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using xTile;
using xTile.Layers;
using xTile.Tiles;

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

        /// <summary>The map property to change when editing a map.</summary>
        private readonly EditMapPatchProperty[] MapProperties;

        /// <summary>Whether the patch makes changes to the map tiles.</summary>
        private bool PatchesTiles => this.RawFromAsset != null;


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
        /// <param name="mapProperties">The map property to change when editing a map, if any.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="normalizeAssetName">Normalize an asset name.</param>
        public EditMapPatch(string logName, ManagedContentPack contentPack, ITokenString assetName, IEnumerable<Condition> conditions, ITokenString fromAsset, TokenRectangle fromArea, TokenRectangle toArea, IEnumerable<EditMapPatchProperty> mapProperties, IMonitor monitor, Func<string, string> normalizeAssetName)
            : base(logName, PatchType.EditMap, contentPack, assetName, conditions, normalizeAssetName, fromAsset: fromAsset)
        {
            this.FromArea = fromArea;
            this.ToArea = toArea;
            this.MapProperties = mapProperties?.ToArray() ?? new EditMapPatchProperty[0];
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
            if (this.PatchesTiles && !this.FromAssetExists())
            {
                this.Monitor.Log($"{errorPrefix}: the {nameof(PatchConfig.FromFile)} file '{this.FromAsset}' doesn't exist.", LogLevel.Warn);
                return;
            }

            // get map
            Map target = asset.GetData<Map>();

            // patch map tiles
            if (this.PatchesTiles)
            {
                // fetch data
                Map source = this.ContentPack.Load<Map>(this.FromAsset);
                Rectangle mapBounds = this.GetMapArea(source);
                if (!this.TryReadArea(this.FromArea, 0, 0, mapBounds.Width, mapBounds.Height, out Rectangle sourceArea, out string error))
                {
                    this.Monitor.Log($"{errorPrefix}: the source area is invalid: {error}.", LogLevel.Warn);
                    return;
                }
                if (!this.TryReadArea(this.ToArea, 0, 0, sourceArea.Width, sourceArea.Height, out Rectangle targetArea, out error))
                {
                    this.Monitor.Log($"{errorPrefix}: the target area is invalid: {error}.", LogLevel.Warn);
                    return;
                }

                // validate area values
                string sourceAreaLabel = this.FromArea != null ? $"{nameof(this.FromArea)}" : "source map";
                string targetAreaLabel = this.ToArea != null ? $"{nameof(this.ToArea)}" : "target map";
                Point sourceMapSize = new Point(source.Layers.Max(p => p.LayerWidth), source.Layers.Max(p => p.LayerHeight));
                Point targetMapSize = new Point(target.Layers.Max(p => p.LayerWidth), target.Layers.Max(p => p.LayerHeight));

                if (sourceArea.X < 0 || sourceArea.Y < 0 || sourceArea.Width < 0 || sourceArea.Height < 0)
                {
                    this.Monitor.Log($"{errorPrefix}: source area (X:{sourceArea.X}, Y:{sourceArea.Y}, Width:{sourceArea.Width}, Height:{sourceArea.Height}) has negative values, which isn't valid.", LogLevel.Error);
                    return;
                }
                if (targetArea.X < 0 || targetArea.Y < 0 || targetArea.Width < 0 || targetArea.Height < 0)
                {
                    this.Monitor.Log($"{errorPrefix}: target area (X:{targetArea.X}, Y:{targetArea.Y}, Width:{targetArea.Width}, Height:{targetArea.Height}) has negative values, which isn't valid.", LogLevel.Error);
                    return;
                }
                if (targetArea.Right > target.DisplayWidth || targetArea.Bottom > target.DisplayHeight)
                {
                    this.Monitor.Log($"{errorPrefix}: target area (X:{targetArea.X}, Y:{targetArea.Y}, Width:{targetArea.Width}, Height:{targetArea.Height}) extends past the edges of the map, which isn't allowed.", LogLevel.Error);
                    return;
                }
                if (sourceArea.Width != targetArea.Width || sourceArea.Height != targetArea.Height)
                {
                    this.Monitor.Log($"{errorPrefix}: {sourceAreaLabel} size (Width:{sourceArea.Width}, Height:{sourceArea.Height}) doesn't match {targetAreaLabel} size (Width:{targetArea.Width}, Height:{targetArea.Height}).", LogLevel.Error);
                    return;
                }
                if (sourceArea.Right > sourceMapSize.X || sourceArea.Bottom > sourceMapSize.Y)
                {
                    this.Monitor.Log($"{errorPrefix}: {sourceAreaLabel} area (X:{sourceArea.X}, Y:{sourceArea.Y}, Width:{sourceArea.Width}, Height:{sourceArea.Height}) extends past the edge of the source map (Width:{sourceMapSize.X}, Height:{sourceMapSize.Y}).", LogLevel.Error);
                    return;
                }
                if (targetArea.Right > targetMapSize.X || targetArea.Bottom > targetMapSize.Y)
                {
                    this.Monitor.Log($"{errorPrefix}: {targetAreaLabel} area (X:{targetArea.X}, Y:{targetArea.Y}, Width:{targetArea.Width}, Height:{targetArea.Height}) extends past the edge of the source map (Width:{targetMapSize.X}, Height:{targetMapSize.Y}).", LogLevel.Error);
                    return;
                }

                // apply source map
                this.ApplyMapOverride(source, sourceArea, target, targetArea);
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
            if (this.PatchesTiles)
                yield return "patched map tiles";

            if (this.MapProperties.Any())
                yield return "changed map properties";
        }


        /*********
        ** Private methods
        *********/
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

                    // copy tiles
                    foreach (Layer sourceLayer in source.Layers)
                    {
                        Layer targetLayer = layerMap[sourceLayer];
                        if (targetLayer == null)
                            continue;

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
    }
}
