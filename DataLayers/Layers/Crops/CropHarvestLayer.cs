using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.DataParsers;
using Pathoschild.Stardew.DataLayers.Framework;
using StardewModdingAPI;
using StardewValley;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Pathoschild.Stardew.DataLayers.Layers.Crops
{
    /// <summary>A data layer which shows whether crops are ready to be harvested.</summary>
    internal class CropHarvestLayer : BaseLayer
    {
        /*********
        ** Fields
        *********/
        /// <summary>The color when a crop is ready.</summary>
        private readonly Color ReadyColor = Color.Green;

        /// <summary>The color when a crop is not ready.</summary>
        private readonly Color NotReadyColor = Color.Black;

        /// <summary>The color when a crop will not be ready in time for the season change.</summary>
        public Color NotEnoughTimeColor { get; } = Color.Red;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="translations">Provides translations in stored in the mod folder's i18n folder.</param>
        /// <param name="config">The data layer settings.</param>
        public CropHarvestLayer(ITranslationHelper translations, LayerConfig config)
            : base(translations.Get("crop-harvest.name"), config)
        {
            this.Legend = new[]
            {
                new LegendEntry(translations.Get("crop-harvest.ready"), this.ReadyColor),
                new LegendEntry(translations.Get("crop-harvest.not-ready"), this.NotReadyColor),
                new LegendEntry(translations.Get("crop-harvest.not-enough-time"), this.NotEnoughTimeColor)
            };
        }

        /// <summary>Get the updated data layer tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleArea">The tiles currently visible on the screen.</param>
        /// <param name="cursorTile">The tile position under the cursor.</param>
        public override IEnumerable<TileGroup> Update(GameLocation location, Rectangle visibleArea, Vector2 cursorTile)
        {
            TileData[] tiles = this.GetTiles(location, visibleArea.GetTiles().ToArray()).ToArray();

            yield return new TileGroup(tiles.Where(p => p.Color == this.ReadyColor), outerBorderColor: this.ReadyColor);
            yield return new TileGroup(tiles.Where(p => p.Color == this.NotReadyColor));
            yield return new TileGroup(tiles.Where(p => p.Color == this.NotEnoughTimeColor), outerBorderColor: this.NotEnoughTimeColor);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get all tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleTiles">The tiles currently visible on the screen.</param>
        private IEnumerable<TileData> GetTiles(GameLocation location, Vector2[] visibleTiles)
        {
            foreach (Vector2 tile in visibleTiles)
            {
                // get crop
                Crop crop = this.GetDirt(location, tile)?.crop;
                if (crop == null)
                    continue;
                CropDataParser data = new CropDataParser(crop, isPlanted: true);

                // yield tile
                if (data.CanHarvestNow)
                    yield return new TileData(tile, this.ReadyColor);
                else if (!location.IsGreenhouse && !data.Seasons.Contains(data.GetNextHarvest().Season))
                    yield return new TileData(tile, this.NotEnoughTimeColor);
                else
                    yield return new TileData(tile, this.NotReadyColor);
            }
        }
    }
}
