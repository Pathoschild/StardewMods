using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common.DataParsers;
using Pathoschild.Stardew.DataLayers.Framework;
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

        /// <summary>The legend entry for crops which won't be ready to harvest before the season change (or are dead).</summary>
        private readonly LegendEntry NotEnoughTimeOrDead;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="config">The data layer settings.</param>
        public CropHarvestLayer(LayerConfig config)
            : base(I18n.CropHarvest_Name(), config)
        {
            this.Legend = new[]
            {
                this.Ready = new LegendEntry(I18n.Keys.CropHarvest_Ready, Color.Green),
                this.NotReady = new LegendEntry(I18n.Keys.CropHarvest_NotReady, Color.Black),
                this.NotEnoughTimeOrDead = new LegendEntry(I18n.Keys.CropHarvest_NotEnoughTimeOrDead, Color.Red)
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
                new TileGroup(tiles.Where(p => p.Type.Id == this.NotEnoughTimeOrDead.Id), outerBorderColor: this.NotEnoughTimeOrDead.Color)
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
                Crop? crop = this.GetDirt(location, tile)?.crop;
                if (crop == null)
                    continue;

                // special case: crop is dead
                if (crop.dead.Value)
                {
                    yield return new TileData(tile, this.NotEnoughTimeOrDead);
                    continue;
                }

                // yield tile
                CropDataParser data = new CropDataParser(crop, isPlanted: true);
                if (data.CanHarvestNow)
                    yield return new TileData(tile, this.Ready);
                else if (!location.SeedsIgnoreSeasonsHere() && !data.Seasons.Contains(data.GetNextHarvest().Season))
                    yield return new TileData(tile, this.NotEnoughTimeOrDead);
                else
                    yield return new TileData(tile, this.NotReady);
            }
        }
    }
}
