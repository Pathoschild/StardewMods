using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>A temporary indexed view of a location's potentially automateable entities.</summary>
    internal class LocationFloodFillIndex
    {
        /*********
        ** Accessors
        *********/
        /// <summary>A cached empty object list.</summary>
        private readonly object[] EmptyObjects = new object[0];

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
            return this.Entities.TryGetValue(tile, out object[] entities)
                ? entities
                : this.EmptyObjects;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Find all indexable entities in a location.</summary>
        /// <param name="location">The location to scan.</param>
        private IEnumerable<KeyValuePair<Vector2, object>> Scan(GameLocation location)
        {
            // objects
            foreach (var pair in location.netObjects.Pairs)
                yield return new KeyValuePair<Vector2, object>(pair.Key, pair.Value);
            foreach (var pair in location.overlayObjects)
                yield return new KeyValuePair<Vector2, object>(pair.Key, pair.Value);

            // terrain features
            foreach (var pair in location.terrainFeatures.Pairs)
            {
                Rectangle box = this.AbsoluteToTileArea(pair.Value.getBoundingBox(pair.Key));
                foreach (Vector2 tile in this.GetTilesIn(box))
                    yield return new KeyValuePair<Vector2, object>(tile, pair.Value);
            }

            // large terrain features
            foreach (LargeTerrainFeature feature in location.largeTerrainFeatures)
            {
                Rectangle box = this.AbsoluteToTileArea(feature.getBoundingBox());
                foreach (Vector2 tile in this.GetTilesIn(box))
                    yield return new KeyValuePair<Vector2, object>(tile, feature);
            }

            // buildings
            if (location is BuildableGameLocation buildableLocation)
            {
                foreach (Building building in buildableLocation.buildings)
                {
                    Rectangle tileArea = new Rectangle(building.tileX.Value, building.tileY.Value, building.tilesWide.Value, building.tilesHigh.Value);
                    foreach (Vector2 tile in this.GetTilesIn(tileArea))
                        yield return new KeyValuePair<Vector2, object>(tile, building);
                }
            }
        }

        /// <summary>Get the tiles included in a tile area.</summary>
        /// <param name="area">The tile area.</param>
        private IEnumerable<Vector2> GetTilesIn(Rectangle area)
        {
            for (int x = area.X; x < area.Right; x++)
            {
                for (int y = area.Y; y < area.Bottom; y++)
                    yield return new Vector2(x, y);
            }
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
