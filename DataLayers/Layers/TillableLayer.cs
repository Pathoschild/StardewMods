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
        /// <summary>The color for a tillable tile.</summary>
        private readonly Color TillableColor = Color.Green;

        /// <summary>The color for a tillable but occupied tile.</summary>
        private readonly Color OccupiedColor = Color.Orange;

        /// <summary>The color for a non-tillable tile.</summary>
        private readonly Color NonTillableColor = Color.Red;


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
                new LegendEntry(translations.Get("tillable.tillable"), this.TillableColor),
                new LegendEntry(translations.Get("tillable.occupied"), this.OccupiedColor),
                new LegendEntry(translations.Get("tillable.not-tillable"), this.NonTillableColor)
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
            TileData[] tillableTiles = tiles.Where(p => p.Color == this.TillableColor).ToArray();
            yield return new TileGroup(tillableTiles, outerBorderColor: this.TillableColor);

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
                Color color;
                if (this.IsTillable(location, tile))
                    color = this.IsOccupied(location, tile) ? this.OccupiedColor : this.TillableColor;
                else
                    color = this.NonTillableColor;

                // yield
                yield return new TileData(tile, color);
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
