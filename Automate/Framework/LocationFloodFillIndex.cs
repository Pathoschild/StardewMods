using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
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
        /// <summary>The indexed entities.</summary>
        private readonly IDictionary<Vector2, object[]> Entities;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="location">The location to index.</param>
        public LocationFloodFillIndex(GameLocation location)
        {
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
                yield return new(furniture.TileLocation, furniture);

            // terrain features
            foreach ((Vector2 originTile, TerrainFeature feature) in location.terrainFeatures.Pairs)
            {
                Rectangle box = this.AbsoluteToTileArea(feature.getBoundingBox(originTile));
                foreach (Vector2 tile in this.GetTilesIn(box))
                    yield return new(tile, feature);
            }

            // large terrain features
            foreach (LargeTerrainFeature feature in location.largeTerrainFeatures)
            {
                Rectangle box = this.AbsoluteToTileArea(feature.getBoundingBox());
                foreach (Vector2 tile in this.GetTilesIn(box))
                    yield return new(tile, feature);
            }

            // buildings
            if (location is BuildableGameLocation buildableLocation)
            {
                foreach (Building building in buildableLocation.buildings)
                {
                    Rectangle tileArea = new(building.tileX.Value, building.tileY.Value, building.tilesWide.Value, building.tilesHigh.Value);
                    foreach (Vector2 tile in this.GetTilesIn(tileArea))
                        yield return new(tile, building);
                }
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
    }
}
