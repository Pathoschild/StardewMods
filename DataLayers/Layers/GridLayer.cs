using Microsoft.Xna.Framework;
using Pathoschild.Stardew.DataLayers.Framework;
using StardewModdingAPI;
using StardewValley;

namespace Pathoschild.Stardew.DataLayers.Layers
{
    /// <summary>A data layer which just shows the tile grid.</summary>
    internal class GridLayer : BaseLayer
    {
        /*********
        ** Fields
        *********/
        /// <summary>A cached empty tile group list.</summary>
        private readonly TileGroup[] NoGroups = new TileGroup[0];


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="config">The data layer settings.</param>
        /// <param name="input">The API for checking input state.</param>
        /// <param name="monitor">Writes messages to the SMAPI log.</param>
        public GridLayer(LayerConfig config, IInputHelper input, IMonitor monitor)
            : base(I18n.Grid_Name(), config, input, monitor)
        {
            this.Legend = new LegendEntry[0];
            this.AlwaysShowGrid = true;
        }

        /// <summary>Get the updated data layer tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleArea">The tile area currently visible on the screen.</param>
        /// <param name="visibleTiles">The tile positions currently visible on the screen.</param>
        /// <param name="cursorTile">The tile position under the cursor.</param>
        public override TileGroup[] Update(GameLocation location, in Rectangle visibleArea, in Vector2[] visibleTiles, in Vector2 cursorTile)
        {
            return this.NoGroups;
        }
    }
}
