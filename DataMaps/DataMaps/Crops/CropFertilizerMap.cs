using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.DataMaps.Framework;
using StardewModdingAPI;
using StardewValley;
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
        /// <summary>The color for basic fertilizer.</summary>
        private readonly Color BasicFertilizerColor = Color.DarkOrange;

        /// <summary>The color for Quality Fertilizer.</summary>
        private readonly Color QualityFertilizerColor = Color.Brown;

        /// <summary>The color for Basic Retaining Soil.</summary>
        private readonly Color BasicRetainingSoilColor = Color.DarkBlue;

        /// <summary>The color for Quality Retaining Soil.</summary>
        private readonly Color QualityRetainingSoilColor = Color.Blue;

        /// <summary>The color for SpeedGro.</summary>
        private readonly Color SpeedGroColor = Color.MediumPurple;

        /// <summary>The color for DeluxeSpeedGro.</summary>
        private readonly Color DeluxeSpeedGroColor = Color.DeepPink;

        /// <summary>The color for DeluxeSpeedGro.</summary>
        private readonly Color ModedFertilizerColor = Color.Green;

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
                new LegendEntry(translations.Get("maps.crop-fertilizer.basic-fertilizer"), this.BasicFertilizerColor),
                new LegendEntry(translations.Get("maps.crop-fertilizer.quality-fertilizer"), this.QualityFertilizerColor),
                new LegendEntry(translations.Get("maps.crop-fertilizer.basic-retaining-soil"), this.BasicRetainingSoilColor),
                new LegendEntry(translations.Get("maps.crop-fertilizer.quality-retaining-soil"), this.QualityRetainingSoilColor),
                new LegendEntry(translations.Get("maps.crop-fertilizer.speed-gro"), this.SpeedGroColor),
                new LegendEntry(translations.Get("maps.crop-fertilizer.delux-speed-gro"), this.DeluxeSpeedGroColor)
            };
        }

        /// <summary>Get the updated data map tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleArea">The tiles currently visible on the screen.</param>
        /// <param name="cursorTile">The tile position under the cursor.</param>
        public IEnumerable<TileGroup> Update(GameLocation location, Rectangle visibleArea, Vector2 cursorTile)
        {
            TileData[] tiles = this.GetFertilizedSoil(location, visibleArea.GetTiles()).ToArray();

            //fertilized crops
            yield return new TileGroup(tiles.ToArray());
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get titles with fertilizers.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleTiles">The tiles currently visible on the screen.</param>
        private IEnumerable<TileData> GetFertilizedSoil(GameLocation location, IEnumerable<Vector2> visibleTiles)
        {
            foreach (Vector2 tile in visibleTiles)
            {
                if (location.terrainFeatures.TryGetValue(tile, out TerrainFeature terrain))
                {
                    if (terrain is HoeDirt dirt)
                    {
                        Color color;
                        switch (dirt.fertilizer)
                        {
                            // basic fertilizer
                            case 368:
                                color = this.BasicFertilizerColor;
                                break;

                            // quality fertilizer
                            case 369:
                                color = this.QualityFertilizerColor;
                                break;

                            // basic retaining soil
                            case 370:
                                color = this.BasicRetainingSoilColor;
                                break;

                            // quality retaining soil
                            case 371:
                                color = this.QualityRetainingSoilColor;
                                break;

                            // speed-gro
                            case 465:
                                color = this.SpeedGroColor;
                                break;

                            // deluxe speed-gro
                            case 466:
                                color = this.DeluxeSpeedGroColor;
                                break;

                            //no fertilizer, skip the loop
                            case 0:
                                continue;

                            // modded fertilizer?
                            default:
                                color = this.ModedFertilizerColor;
                                break;
                        }
                        // yield
                        yield return new TileData(tile, color);
                    }
                }
            }
        }
    }
}
