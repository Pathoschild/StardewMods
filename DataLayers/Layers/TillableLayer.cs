using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.DataLayers.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using xTile.Dimensions;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Pathoschild.Stardew.DataLayers.Layers
{
    /// <summary>A data layer which shows whether tiles are tillable.</summary>
    internal class TillableLayer : BaseLayer
    {
        /*********
        ** Fields
        *********/
        /// <summary>The legend entry for tillable tiles.</summary>
        private readonly LegendEntry Tillable;

        /// <summary>The legend entry for tillable but occupied tiles.</summary>
        private readonly LegendEntry Occupied;

        /// <summary>The legend entry for non-tillable tiles.</summary>
        private readonly LegendEntry NonTillable;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="translations">Provides translations in stored in the mod folder's i18n folder.</param>
        /// <param name="config">The data layer settings.</param>
        public TillableLayer(ITranslationHelper translations, LayerConfig config)
            : base(translations.Get("tillable.name"), config)
        {
            this.Legend = new[]
            {
                this.Tillable = new LegendEntry(translations, "tillable.tillable", Color.Green),
                this.Occupied = new LegendEntry(translations, "tillable.occupied", Color.Orange),
                this.NonTillable = new LegendEntry(translations, "tillable.not-tillable", Color.Red)
            };
        }

        /// <summary>Get the updated data layer tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleArea">The tiles currently visible on the screen.</param>
        /// <param name="cursorTile">The tile position under the cursor.</param>
        public override IEnumerable<TileGroup> Update(GameLocation location, Rectangle visibleArea, Vector2 cursorTile)
        {
            TileData[] tiles = this.GetTiles(location, visibleArea.GetTiles()).ToArray();

            // tillable tiles
            TileData[] tillableTiles = tiles.Where(p => p.Type.Id == this.Tillable.Id).ToArray();
            yield return new TileGroup(tillableTiles, outerBorderColor: this.Tillable.Color);

            // other tiles
            yield return new TileGroup(tiles.Except(tillableTiles).ToArray());
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the updated data layer tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleTiles">The tiles currently visible on the screen.</param>
        private IEnumerable<TileData> GetTiles(GameLocation location, IEnumerable<Vector2> visibleTiles)
        {
            foreach (Vector2 tile in visibleTiles)
            {
                // get color
                LegendEntry type;
                if (this.IsTillable(location, tile))
                    type = this.IsOccupied(location, tile) ? this.Occupied : this.Tillable;
                else
                    type = this.NonTillable;

                // yield
                yield return new TileData(tile, type);
            }
        }


        /// <summary>Get whether a tile is tillable.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="tile">The tile to check.</param>
        /// <remarks>Derived from <see cref="StardewValley.Tools.Hoe.DoFunction"/>.</remarks>
        private bool IsTillable(GameLocation location, Vector2 tile)
        {
            return location.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Diggable", "Back") != null;
        }

        /// <summary>Get whether a tile is blocked due to something it contains.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="tile">The tile to check.</param>
        /// <remarks>Derived from <see cref="StardewValley.Tools.Hoe.DoFunction"/>.</remarks>
        private bool IsOccupied(GameLocation location, Vector2 tile)
        {
            // check for objects, characters, or terrain features
            if (location.isTileOccupied(tile) || !location.isTilePassable(new Location((int)tile.X, (int)tile.Y), Game1.viewport))
                return true;

            // buildings
            if (location is BuildableGameLocation buildableLocation)
            {
                foreach (Building building in buildableLocation.buildings)
                {
                    if (building.occupiesTile(tile))
                        return true;
                }
            }

            return false;
        }
    }
}
