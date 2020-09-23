using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.DataLayers.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.DataLayers.Layers.Crops
{
    /// <summary>A data layer which shows whether crops needs to be watered.</summary>
    internal class CropWaterLayer : BaseLayer
    {
        /*********
        ** Fields
        *********/
        /// <summary>The legend entry for dry crops.</summary>
        private readonly LegendEntry Dry;

        /// <summary>The legend entry for watered crops.</summary>
        private readonly LegendEntry Watered;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="config">The data layer settings.</param>
        /// <param name="input">The API for checking input state.</param>
        /// <param name="monitor">Writes messages to the SMAPI log.</param>
        public CropWaterLayer(LayerConfig config, IInputHelper input, IMonitor monitor)
            : base(I18n.CropWater_Name(), config, input, monitor)
        {
            this.Legend = new[]
            {
               this.Watered = new LegendEntry(I18n.Keys.CropWater_Watered, Color.Green),
               this.Dry = new LegendEntry(I18n.Keys.CropWater_Dry, Color.Red)
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
                this.GetGroup(location, visibleTiles, HoeDirt.watered, this.Watered),
                this.GetGroup(location, visibleTiles, HoeDirt.dry, this.Dry)
            };
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get a tile group.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleTiles">The tiles currently visible on the screen.</param>
        /// <param name="state">The watered state to match.</param>
        /// <param name="type">The legend entry for the group.</param>
        private TileGroup GetGroup(GameLocation location, Vector2[] visibleTiles, int state, LegendEntry type)
        {
            var crops = this
                .GetCropsByStatus(location, visibleTiles, state)
                .Select(pos => new TileData(pos, type));
            return new TileGroup(crops, outerBorderColor: type.Color);
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
                if (dirt?.crop != null && !this.IsDeadCrop(dirt) && dirt.state.Value == state)
                    yield return tile;
            }
        }
    }
}
