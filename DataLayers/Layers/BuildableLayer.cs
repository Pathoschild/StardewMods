using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.DataLayers.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Pathoschild.Stardew.DataLayers.Layers
{
    /// <summary>A data layer which shows whether tiles can be built on.</summary>
    internal class BuildableLayer : BaseLayer
    {
        /*********
        ** Fields
        *********/
        /// <summary>The color for a buildable tile.</summary>
        private readonly Color BuildableColor = Color.Green;

        /// <summary>The color for a buildable but occupied tile.</summary>
        private readonly Color OccupiedColor = Color.Orange;

        /// <summary>The color for a non-buildable tile.</summary>
        private readonly Color NonBuildableColor = Color.Red;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="translations">Provides translations in stored in the mod folder's i18n folder.</param>
        /// <param name="config">The data layer settings.</param>
        public BuildableLayer(ITranslationHelper translations, LayerConfig config)
            : base(translations.Get("buildable.name"), config)
        {
            this.Legend = new[]
            {
                new LegendEntry(translations.Get("buildable.buildable"), this.BuildableColor),
                new LegendEntry(translations.Get("buildable.occupied"), this.OccupiedColor),
                new LegendEntry(translations.Get("buildable.not-buildable"), this.NonBuildableColor)
            };
        }

        /// <summary>Get the updated data layer tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleArea">The tiles currently visible on the screen.</param>
        /// <param name="cursorTile">The tile position under the cursor.</param>
        public override IEnumerable<TileGroup> Update(GameLocation location, Rectangle visibleArea, Vector2 cursorTile)
        {
            TileData[] tiles = this.GetTiles(location, visibleArea.GetTiles()).ToArray();

            // buildable tiles
            TileData[] buildableTiles = tiles.Where(p => p.Color == this.BuildableColor).ToArray();
            yield return new TileGroup(buildableTiles, outerBorderColor: this.BuildableColor);

            // other tiles
            yield return new TileGroup(tiles.Except(buildableTiles).ToArray());
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
                    Color color;
                    if (this.IsBuildable(buildableLocation, tile))
                        color = this.IsOccupied(buildableLocation, tile) ? this.OccupiedColor : this.BuildableColor;
                    else
                        color = this.NonBuildableColor;

                    // yield
                    yield return new TileData(tile, color);
                }
            }

            // non-buildable location
            else
            {
                foreach (Vector2 tile in visibleTiles)
                    yield return new TileData(tile, this.NonBuildableColor);
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
