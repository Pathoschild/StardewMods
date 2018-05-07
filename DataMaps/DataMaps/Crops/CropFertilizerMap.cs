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
    internal class CropFertilizerMap : IDataMap
    {
        /*********
        ** Properties
        *********/
        /// <summary>The color for fertilizer.</summary>
        private readonly Color FertilizerColor = Color.Green;

        /// <summary>The color for retaining soil.</summary>
        private readonly Color RetainingSoilColor = Color.Blue;

        /// <summary>The color for speed-gro.</summary>
        private readonly Color SpeedGroColor = Color.Magenta;

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
        public CropFertilizerMap(ITranslationHelper translations)
        {
            this.Name = translations.Get("maps.crop-fertilizer.name");
            this.Legend = new[]
            {
                new LegendEntry(translations.Get("maps.crop-fertilizer.fertilizer"), this.FertilizerColor),
                new LegendEntry(translations.Get("maps.crop-fertilizer.retaining-soil"), this.RetainingSoilColor),
                new LegendEntry(translations.Get("maps.crop-fertilizer.speed-gro"), this.SpeedGroColor)
            };
        }

        /// <summary>Get the updated data map tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleArea">The tiles currently visible on the screen.</param>
        /// <param name="cursorTile">The tile position under the cursor.</param>
        public IEnumerable<TileGroup> Update(GameLocation location, Rectangle visibleArea, Vector2 cursorTile)
        {
            Vector2[] visibleTiles = visibleArea.GetTiles().ToArray();

            yield return this.GetGroup(location, visibleTiles, this.FertilizerColor, HoeDirt.fertilizerLowQuality, HoeDirt.fertilizerHighQuality);
            yield return this.GetGroup(location, visibleTiles, this.SpeedGroColor, HoeDirt.speedGro, HoeDirt.superSpeedGro);
            yield return this.GetGroup(location, visibleTiles, this.RetainingSoilColor, HoeDirt.waterRetentionSoil, HoeDirt.waterRetentionSoilQUality);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get a tile group.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleTiles">The tiles currently visible on the screen.</param>
        /// <param name="color">The overlay color.</param>
        /// <param name="states">The fertilizer states to match.</param>
        private TileGroup GetGroup(GameLocation location, Vector2[] visibleTiles, Color color, params int[] states)
        {
            TileData[] crops = this.GetSoilByState(location, visibleTiles, states).Select(pos => new TileData(pos, color)).ToArray();
            return new TileGroup(crops, outerBorderColor: color);
        }

        /// <summary>Get tiles with the given fertilizer states.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleTiles">The tiles currently visible on the screen.</param>
        /// <param name="states">The fertilizer states to match.</param>
        private IEnumerable<Vector2> GetSoilByState(GameLocation location, IEnumerable<Vector2> visibleTiles, int[] states)
        {
            foreach (Vector2 tile in visibleTiles)
            {
                HoeDirt dirt = this.GetDirt(location, tile);
                if (dirt != null && states.Contains(dirt.fertilizer.Value))
                    yield return tile;
            }
        }

        // <summary>Get the dirt instance for a tile, if any.</summary>
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
