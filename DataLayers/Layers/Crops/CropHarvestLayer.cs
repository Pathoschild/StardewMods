using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common.DataParsers;
using Pathoschild.Stardew.DataLayers.Framework;
using StardewModdingAPI;
using StardewValley;

namespace Pathoschild.Stardew.DataLayers.Layers.Crops
{
    /// <summary>A data layer which shows whether crops are ready to be harvested.</summary>
    internal class CropHarvestLayer : BaseLayer
    {
        /*********
        ** Fields
        *********/
        /// <summary>The legend entry for crops which are ready.</summary>
        private readonly LegendEntry Ready;

        /// <summary>The legend entry for crops which are not ready.</summary>
        private readonly LegendEntry NotReady;

        /// <summary>The legend entry for crops which won't be ready to harvest before the season change.</summary>
        private readonly LegendEntry NotEnoughTime;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="translations">Provides translations in stored in the mod folder's i18n folder.</param>
        /// <param name="config">The data layer settings.</param>
        /// <param name="input">The API for checking input state.</param>
        /// <param name="monitor">Writes messages to the SMAPI log.</param>
        public CropHarvestLayer(ITranslationHelper translations, LayerConfig config, IInputHelper input, IMonitor monitor)
            : base(translations.Get("crop-harvest.name"), config, input, monitor)
        {
            this.Legend = new[]
            {
                this.Ready = new LegendEntry(translations, "crop-harvest.ready", Color.Green),
                this.NotReady = new LegendEntry(translations, "crop-harvest.not-ready", Color.Black),
                this.NotEnoughTime = new LegendEntry(translations, "crop-harvest.not-enough-time", Color.Red)
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

            return new[]
            {
                new TileGroup(tiles.Where(p => p.Type.Id == this.Ready.Id), outerBorderColor: this.Ready.Color),
                new TileGroup(tiles.Where(p => p.Type.Id == this.NotReady.Id)),
                new TileGroup(tiles.Where(p => p.Type.Id == this.NotEnoughTime.Id), outerBorderColor: this.NotEnoughTime.Color)
            };
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
                    yield return new TileData(tile, this.Ready);
                else if (!location.IsGreenhouse && !data.Seasons.Contains(data.GetNextHarvest().Season))
                    yield return new TileData(tile, this.NotEnoughTime);
                else
                    yield return new TileData(tile, this.NotReady);
            }
        }
    }
}
