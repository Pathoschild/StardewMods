using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.DataLayers.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.DataLayers.Layers.Crops
{
    /// <summary>A data layer which shows whether crops needs to be watered.</summary>
    internal class CropFertilizerLayer : BaseLayer
    {
        /*********
        ** Fields
        *********/
        /// <summary>The legend entry for fertilizer.</summary>
        private readonly LegendEntry Fertilizer;

        /// <summary>The legend entry for retaining soil.</summary>
        private readonly LegendEntry RetainingSoil;

        /// <summary>The legend entry for speed-gro.</summary>
        private readonly LegendEntry SpeedGro;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="config">The data layer settings.</param>
        public CropFertilizerLayer(LayerConfig config)
            : base(I18n.CropFertilizer_Name(), config)
        {
            this.Legend = new[]
            {
                this.Fertilizer = new LegendEntry(I18n.Keys.CropFertilizer_Fertilizer, Color.Green),
                this.RetainingSoil = new LegendEntry(I18n.Keys.CropFertilizer_RetainingSoil, Color.Blue),
                this.SpeedGro = new LegendEntry(I18n.Keys.CropFertilizer_SpeedGro, Color.Magenta)
            };
        }

        /// <summary>Get the updated data layer tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleArea">The tile area currently visible on the screen.</param>
        /// <param name="visibleTiles">The tile positions currently visible on the screen.</param>
        /// <param name="cursorTile">The tile position under the cursor.</param>
        public override TileGroup[] Update(GameLocation location, in Rectangle visibleArea, in Vector2[] visibleTiles, in Vector2 cursorTile)
        {
            return new[]
            {
                this.GetGroup(location, visibleTiles, this.Fertilizer, HoeDirt.fertilizerLowQuality, HoeDirt.fertilizerHighQuality, HoeDirt.fertilizerDeluxeQuality),
                this.GetGroup(location, visibleTiles, this.SpeedGro, HoeDirt.speedGro, HoeDirt.superSpeedGro, HoeDirt.hyperSpeedGro),
                this.GetGroup(location, visibleTiles, this.RetainingSoil, HoeDirt.waterRetentionSoil, HoeDirt.waterRetentionSoilQuality, HoeDirt.waterRetentionSoilDeluxe)
            };
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get a tile group.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleTiles">The tiles currently visible on the screen.</param>
        /// <param name="type">The legend entry for the group.</param>
        /// <param name="states">The fertilizer states to match.</param>
        private TileGroup GetGroup(GameLocation location, Vector2[] visibleTiles, LegendEntry type, params int[] states)
        {
            var crops = this
                .GetSoilByState(location, visibleTiles, states)
                .Select(pos => new TileData(pos, type));

            return new TileGroup(crops, outerBorderColor: type.Color);
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
                if (dirt != null && !this.IsDeadCrop(dirt) && states.Contains(dirt.fertilizer.Value))
                    yield return tile;
            }
        }
    }
}
