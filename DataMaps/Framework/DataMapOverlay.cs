using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using StardewValley;
using XRectangle = xTile.Dimensions.Rectangle;

namespace Pathoschild.Stardew.DataMaps.Framework
{
    /// <summary>Renders a data map as an overlay over the world.</summary>
    internal class DataMapOverlay : BaseOverlay
    {
        /*********
        ** Properties
        *********/
        /// <summary>The legend to display (if any).</summary>
        private readonly LegendComponent Legend;

        /// <summary>The data map to render.</summary>
        private readonly IDataMap Map;

        /// <summary>The tiles to render.</summary>
        private TileData[] Tiles;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="map">The data map to render.</param>
        public DataMapOverlay(IDataMap map)
        {
            this.Map = map;
            this.Legend = new LegendComponent(map.GetLegendEntries().ToArray());
        }

        /// <summary>Update the overlay.</summary>
        public void Update()
        {
            // no tiles to draw
            if (Game1.currentLocation == null)
            {
                this.Tiles = new TileData[0];
                return;
            }

            // get updated tiles
            GameLocation location = Game1.currentLocation;
            this.Tiles = this.Map.Update(location, this.GetVisibleTiles(location, Game1.viewport)).ToArray();
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Draw to the screen.</summary>
        /// <param name="spriteBatch">The sprite batch to which to draw.</param>
        protected override void Draw(SpriteBatch spriteBatch)
        {
            if (this.Tiles == null || this.Tiles.Length == 0)
                return;

            // draw tile overlay
            int tileSize = Game1.tileSize;
            foreach (TileData tile in this.Tiles.ToArray())
            {
                Vector2 position = tile.TilePosition * tileSize - new Vector2(Game1.viewport.X, Game1.viewport.Y);
                spriteBatch.Draw(CommonHelper.Pixel, new Rectangle((int)position.X, (int)position.Y, tileSize, tileSize), tile.Color * .3f);
            }

            // draw legend
            this.Legend?.Draw(spriteBatch);
        }

        /// <summary>The method invoked when the player resizes the game windoww.</summary>
        /// <param name="oldBounds">The previous game window bounds.</param>
        /// <param name="newBounds">The new game window bounds.</param>
        protected override void ReceiveGameWindowResized(XRectangle oldBounds, XRectangle newBounds)
        {
            this.Legend?.ReceiveWindowSizeChanged(oldBounds, newBounds);
        }

        /// <summary>Get all tiles currently visible to the player.</summary>
        /// <param name="location">The game location.</param>
        /// <param name="viewport">The game viewport.</param>
        protected IEnumerable<Vector2> GetVisibleTiles(GameLocation location, XRectangle viewport)
        {
            int tileSize = Game1.tileSize;
            int left = viewport.X / tileSize;
            int top = viewport.Y / tileSize;
            int right = (int)Math.Ceiling((viewport.X + viewport.Width) / (decimal)tileSize);
            int bottom = (int)Math.Ceiling((viewport.Y + viewport.Height) / (decimal)tileSize);

            for (int x = left; x < right; x++)
            {
                for (int y = top; y < bottom; y++)
                {
                    if (location.isTileOnMap(x, y))
                        yield return new Vector2(x, y);
                }
            }
        }
    }
}
