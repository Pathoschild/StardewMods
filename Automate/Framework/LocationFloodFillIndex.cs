using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using Object = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>A temporary indexed view of a location's potentially automateable entities.</summary>
    internal class LocationFloodFillIndex
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The log keys for entities which throw an exception when scanned, to avoid logging them repeatedly.</summary>
        private static readonly HashSet<string> LoggedErrorKeys = new();

        /// <summary>The indexed entities.</summary>
        private readonly IDictionary<Vector2, object[]> Entities;

        /// <summary>The monitor with which to log errors.</summary>
        private readonly IMonitor Monitor;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="location">The location to index.</param>
        /// <param name="monitor">The monitor with which to log errors.</param>
        public LocationFloodFillIndex(GameLocation location, IMonitor monitor)
        {
            this.Monitor = monitor;

            this.Entities = this.Scan(location)
                .GroupBy(group => group.Key)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(p => p.Value).ToArray()
                );
        }

        /// <summary>Get all indexed entity covering the given tile.</summary>
        /// <param name="tile">The tile to check.</param>
        public IEnumerable<object> GetEntities(Vector2 tile)
        {
            return this.Entities.TryGetValue(tile, out object[]? entities)
                ? entities
                : Array.Empty<object>();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Find all indexable entities in a location.</summary>
        /// <param name="location">The location to scan.</param>
        private IEnumerable<KeyValuePair<Vector2, object>> Scan(GameLocation location)
        {
            // objects
            foreach ((Vector2 tile, Object obj) in location.netObjects.Pairs)
                yield return new(tile, obj);
            foreach ((Vector2 tile, Object obj) in location.overlayObjects)
                yield return new(tile, obj);

            // furniture
            foreach (Furniture furniture in location.furniture)
            {
                if (!this.TryGetData("furniture", location, null, () => furniture.TileLocation, out Vector2? tile))
                    continue;

                yield return new(tile.Value, furniture);
            }

            // terrain features
            foreach ((Vector2 originTile, TerrainFeature feature) in location.terrainFeatures.Pairs)
            {
                if (!this.TryGetData("terrain feature", location, originTile, () => this.GetTilesIn(this.AbsoluteToTileArea(feature.getBoundingBox())), out Vector2[]? tiles))
                    continue;

                foreach (Vector2 tile in tiles)
                    yield return new(tile, feature);
            }

            // large terrain features
            foreach (LargeTerrainFeature feature in location.largeTerrainFeatures)
            {
                if (!this.TryGetData("large terrain feature", location, null, () => this.GetTilesIn(this.AbsoluteToTileArea(feature.getBoundingBox())), out Vector2[]? tiles))
                    continue;

                foreach (Vector2 tile in tiles)
                    yield return new(tile, feature);
            }

            // buildings
            foreach (Building building in location.buildings)
            {
                Rectangle tileArea = new(building.tileX.Value, building.tileY.Value, building.tilesWide.Value, building.tilesHigh.Value);
                foreach (Vector2 tile in this.GetTilesIn(tileArea))
                    yield return new(tile, building);
            }
        }

        /// <summary>Get the tiles included in a tile area.</summary>
        /// <param name="area">The tile area.</param>
        private Vector2[] GetTilesIn(Rectangle area)
        {
            Vector2[] tiles = new Vector2[area.Width * area.Height];

            if (tiles.Length == 1)
                tiles[0] = new Vector2(area.X, area.Y);
            else
            {
                int i = 0;
                for (int y = area.Y, bottom = area.Bottom; y < bottom; y++)
                {
                    for (int x = area.X, right = area.Right; x < right; x++)
                        tiles[i++] = new Vector2(x, y);
                }
            }

            return tiles;
        }

        /// <summary>Get a tile area for a given absolute pixel area.</summary>
        /// <param name="area">The absolute pixel area.</param>
        private Rectangle AbsoluteToTileArea(Rectangle area)
        {
            return new Rectangle(
                area.X / Game1.tileSize,
                area.Y / Game1.tileSize,
                area.Width / Game1.tileSize,
                area.Height / Game1.tileSize
            );
        }

        /// <summary>Get information from an entity, or log an error if it throws an exception.</summary>
        /// <typeparam name="TData">The type of the data to retrieve.</typeparam>
        /// <param name="label">A human-readable label for the entity type (like 'furniture' or 'terrain feature').</param>
        /// <param name="location">The location containing the entity.</param>
        /// <param name="tile">The entity's origin tile, if known.</param>
        /// <param name="get">Read the entity data.</param>
        /// <param name="data">The read data, if successfully read.</param>
        /// <returns>Returns whether the data was successfully read.</returns>
        private bool TryGetData<TData>(string label, GameLocation location, Vector2? tile, Func<TData> get, [NotNullWhen(true)] out TData? data)
        {
            try
            {
                data = get();
                return data != null;
            }
            catch (Exception ex)
            {
                if (LocationFloodFillIndex.LoggedErrorKeys.Add($"{label}|{location.NameOrUniqueName}|{tile}"))
                {
                    this.Monitor.Log($"Failed accessing broken {label} in {location.NameOrUniqueName}{(tile.HasValue ? $" at {tile.Value}" : "")}. See the SMAPI log for details.", LogLevel.Warn);
                    this.Monitor.Log(ex.ToString());
                }

                data = default;
                return false;
            }
        }
    }
}
