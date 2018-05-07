using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.DataMaps.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Pathoschild.Stardew.DataMaps.DataMaps.Crops
{
    /// <summary>A data map which shows whether crops needs to be watered.</summary>
    internal class CropWaterMap : IDataMap
    {
        /*********
        ** Properties
        *********/
        /// <summary>The color for a dry crop.</summary>
        private readonly Color DryColor = Color.Red;

        /// <summary>The color for a watered crop.</summary>
        private readonly Color WateredColor = Color.Green;

        /// <summary>The legend entries to display.</summary>
        public LegendEntry[] Legend { get; }


        /*********
        ** Accessors
        *********/
        /// <summary>The map's display name.</summary>
        public string Name { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="translations">Provides translations in stored in the mod folder's i18n folder.</param>
        public CropWaterMap(ITranslationHelper translations)
        {
            this.Name = translations.Get("maps.crop-water.name");
            this.Legend = new[]
            {
                new LegendEntry(translations.Get("maps.crop-water.watered"), this.WateredColor),
                new LegendEntry(translations.Get("maps.crop-water.dry"), this.DryColor)
            };
        }

        /// <summary>Get the updated data map tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleArea">The tiles currently visible on the screen.</param>
        /// <param name="cursorTile">The tile position under the cursor.</param>
        public IEnumerable<TileGroup> Update(GameLocation location, Rectangle visibleArea, Vector2 cursorTile)
        {
            Vector2[] visibleTiles = visibleArea.GetTiles().ToArray();

            yield return this.GetGroup(location, visibleTiles, HoeDirt.watered, this.WateredColor);
            yield return this.GetGroup(location, visibleTiles, HoeDirt.dry, this.DryColor);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get a tile group.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleTiles">The tiles currently visible on the screen.</param>
        /// <param name="state">The watered state to match.</param>
        /// <param name="color">The overlay color.</param>
        private TileGroup GetGroup(GameLocation location, Vector2[] visibleTiles, int state, Color color)
        {
            TileData[] crops = this.GetCropsByStatus(location, visibleTiles, state).Select(pos => new TileData(pos, color)).ToArray();
            return new TileGroup(crops, outerBorderColor: color);
        }

        /// <summary>Get tiles containing crops not covered by a sprinkler.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleTiles">The tiles currently visible on the screen.</param>
        /// <param name="state">The watered state to match.</param>
        private IEnumerable<Vector2> GetCropsByStatus(GameLocation location, Vector2[] visibleTiles, int state)
        {
            foreach (Vector2 tile in visibleTiles)
            {
                HoeDirt dirt = this.GetDirt(location, tile);
                if (dirt?.crop != null && dirt.state.Value == state)
                    yield return tile;
            }
        }

        /// <summary>Get the dirt instance for a tile, if any.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="tile">The tile to check.</param>
        private HoeDirt GetDirt(GameLocation location, Vector2 tile)
        {
            if (location.terrainFeatures.TryGetValue(tile, out TerrainFeature terrain) && terrain is HoeDirt dirt)
                return dirt;
            if (location.objects.TryGetValue(tile, out Object obj) && obj is IndoorPot pot)
                return pot.hoeDirt.Value;

            return null;
        }
    }
}
