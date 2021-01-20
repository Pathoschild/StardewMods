using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.DataLayers.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;

namespace Pathoschild.Stardew.DataLayers.Layers
{
    /// <summary>A data layer which shows whether tiles can be built on.</summary>
    internal class BuildableLayer : BaseLayer
    {
        /*********
        ** Fields
        *********/
        /// <summary>The legend entry for buildable tiles.</summary>
        private readonly LegendEntry Buildable;

        /// <summary>The legend entry for buildable but occupied tiles.</summary>
        private readonly LegendEntry Occupied;

        /// <summary>The legend entry for non-buildable tiles.</summary>
        private readonly LegendEntry NonBuildable;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="config">The data layer settings.</param>
        public BuildableLayer(LayerConfig config)
            : base(I18n.Buildable_Name(), config)
        {
            this.Legend = new[]
            {
                this.Buildable = new LegendEntry(I18n.Keys.Buildable_Buildable, Color.Green),
                this.Occupied = new LegendEntry(I18n.Keys.Buildable_Occupied, Color.Orange),
                this.NonBuildable = new LegendEntry(I18n.Keys.Buildable_NotBuildable, Color.Red)
            };
        }

        /// <summary>Get the updated data layer tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleArea">The tile area currently visible on the screen.</param>
        /// <param name="visibleTiles">The tile positions currently visible on the screen.</param>
        /// <param name="cursorTile">The tile position under the cursor.</param>
        public override TileGroup[] Update(GameLocation location, in Rectangle visibleArea, in Vector2[] visibleTiles, in Vector2 cursorTile)
        {
            TileData[] tiles = this.GetTiles(location, visibleTiles).ToArray();
            TileData[] buildableTiles = tiles.Where(p => p.Type.Id == this.Buildable.Id).ToArray();

            return new[]
            {
                new TileGroup(buildableTiles, outerBorderColor: this.Buildable.Color),
                new TileGroup(tiles.Except(buildableTiles))
            };
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the updated data layer tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleTiles">The tiles currently visible on the screen.</param>
        private IEnumerable<TileData> GetTiles(GameLocation location, IEnumerable<Vector2> visibleTiles)
        {
            // buildable location
            if (location is BuildableGameLocation buildableLocation)
            {
                foreach (Vector2 tile in visibleTiles)
                {
                    // get color
                    LegendEntry type;
                    if (this.IsBuildable(buildableLocation, tile))
                        type = this.IsOccupied(buildableLocation, tile) ? this.Occupied : this.Buildable;
                    else
                        type = this.NonBuildable;

                    // yield
                    yield return new TileData(tile, type);
                }
            }

            // non-buildable location
            else
            {
                foreach (Vector2 tile in visibleTiles)
                    yield return new TileData(tile, this.NonBuildable);
            }
        }


        /// <summary>Get whether a tile is buildable.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="tile">The tile to check.</param>
        /// <remarks>Derived from <see cref="BuildableGameLocation.buildStructure(Building, Vector2, Farmer, bool)"/>.</remarks>
        private bool IsBuildable(BuildableGameLocation location, Vector2 tile)
        {
            return location.isBuildable(tile);
        }

        /// <summary>Get whether a tile is blocked due to something it contains.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="tile">The tile to check.</param>
        /// <remarks>Derived from <see cref="BuildableGameLocation.buildStructure(Building, Vector2, Farmer, bool)"/>.</remarks>
        private bool IsOccupied(BuildableGameLocation location, Vector2 tile)
        {
            // buildings
            foreach (Building building in location.buildings)
            {
                if (building.occupiesTile(tile))
                    return true;
            }

            // players
            foreach (Farmer farmer in location.farmers)
            {
                if (farmer.GetBoundingBox().Intersects(new Rectangle((int)tile.X * Game1.tileSize, (int)tile.Y * Game1.tileSize, Game1.tileSize, Game1.tileSize)))
                    return false;
            }

            return false;
        }
    }
}
