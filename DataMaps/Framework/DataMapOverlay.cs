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
        /// <summary>The number of pixels between the color box and its label.</summary>
        private readonly int LegendColorBoxPadding = 5;

        /// <summary>The size of the margin around the displayed legend.</summary>
        private readonly int Margin = 30;

        /// <summary>The padding between the border and content.</summary>
        private readonly int Padding = 5;

        /// <summary>The data map to render.</summary>
        private readonly IDataMap Map;

        /// <summary>The UI bounds to draw.</summary>
        private Rectangle Bounds;

        /// <summary>The pixel size of a color box.</summary>
        private int LegendColorBoxSize;

        /// <summary>The maximum label width.</summary>
        private int LegendContentWidth;

        /// <summary>The combined height of the labels.</summary>
        private int LegendContentHeight;

        /// <summary>The legend entries to show.</summary>
        private LegendEntry[] Legend;

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
            this.Legend = map.GetLegendEntries().ToArray();

            this.RecalculateDimensions();
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
            if (this.Legend.Any())
            {
                // calculate dimensions
                var bounds = this.Bounds;
                int cornerWidth = Sprites.Legend.TopLeft.Width * Game1.pixelZoom;
                int cornerHeight = Sprites.Legend.TopLeft.Height * Game1.pixelZoom;
                int innerWidth = bounds.Width - cornerWidth * 2;
                int innerHeight = bounds.Height - cornerHeight * 2;

                // draw scroll background
                spriteBatch.Draw(Sprites.Legend.Sheet, new Rectangle(bounds.Left + cornerWidth, bounds.Top + cornerHeight, innerWidth, innerHeight), Sprites.Legend.Background, Color.White);

                // draw borders
                spriteBatch.Draw(Sprites.Legend.Sheet, new Rectangle(bounds.Left + cornerWidth, bounds.Top, innerWidth, cornerHeight), Sprites.Legend.Top, Color.White);
                spriteBatch.Draw(Sprites.Legend.Sheet, new Rectangle(bounds.Left + cornerWidth, bounds.Bottom - cornerHeight, innerWidth, cornerHeight), Sprites.Legend.Bottom, Color.White);
                spriteBatch.Draw(Sprites.Legend.Sheet, new Rectangle(bounds.Left, bounds.Top + cornerHeight, cornerWidth, innerHeight), Sprites.Legend.Left, Color.White);
                spriteBatch.Draw(Sprites.Legend.Sheet, new Rectangle(bounds.Right - cornerWidth, bounds.Top + cornerHeight, cornerWidth, innerHeight), Sprites.Legend.Right, Color.White);

                // draw corners
                spriteBatch.Draw(Sprites.Legend.Sheet, new Rectangle(bounds.Left, bounds.Top, cornerWidth, cornerHeight), Sprites.Legend.TopLeft, Color.White);
                spriteBatch.Draw(Sprites.Legend.Sheet, new Rectangle(bounds.Left, bounds.Bottom - cornerHeight, cornerWidth, cornerHeight), Sprites.Legend.BottomLeft, Color.White);
                spriteBatch.Draw(Sprites.Legend.Sheet, new Rectangle(bounds.Right - cornerWidth, bounds.Top, cornerWidth, cornerHeight), Sprites.Legend.TopRight, Color.White);
                spriteBatch.Draw(Sprites.Legend.Sheet, new Rectangle(bounds.Right - cornerWidth, bounds.Bottom - cornerHeight, cornerWidth, cornerHeight), Sprites.Legend.BottomRight, Color.White);

                // draw text
                for (int i = 0; i < this.Legend.Length; i++)
                {
                    LegendEntry value = this.Legend[i];
                    int leftOffset = bounds.X + cornerWidth + this.Padding;
                    int topOffset = bounds.Y + cornerHeight + this.Padding + i * this.LegendColorBoxSize;

                    spriteBatch.DrawLine(leftOffset, topOffset, new Vector2(this.LegendColorBoxSize), value.Color);
                    spriteBatch.DrawString(Game1.smallFont, value.Name, new Vector2(leftOffset + this.LegendColorBoxSize + this.LegendColorBoxPadding, topOffset + 2), Color.Black);
                }
            }
        }

        /// <summary>The method invoked when the player resizes the game windoww.</summary>
        /// <param name="oldBounds">The previous game window bounds.</param>
        /// <param name="newBounds">The new game window bounds.</param>
        protected override void ReceiveGameWindowResized(XRectangle oldBounds, XRectangle newBounds)
        {
            this.RecalculateDimensions();
        }

        /// <summary>Recalculate the component positions and dimensions.</summary>
        private void RecalculateDimensions()
        {
            // get corner dimensions
            var corner = Sprites.Legend.TopLeft;
            int cornerWidth = corner.Width * Game1.pixelZoom;
            int cornerHeight = corner.Height * Game1.pixelZoom;

            // calculate legend dimensions
            if (this.Legend.Any())
            {
                this.LegendColorBoxSize = (int)Game1.smallFont.MeasureString("X").Y;
                this.LegendContentWidth = this.LegendColorBoxSize + this.LegendColorBoxPadding + (int)this.Legend.Select(p => Game1.smallFont.MeasureString(p.Name).X).Max();
                this.LegendContentHeight = this.Legend.Length * this.LegendColorBoxSize;
                this.Bounds = new Rectangle(
                    x: this.Margin,
                    y: this.Margin,
                    width: this.LegendContentWidth + (cornerWidth + this.Padding) * 2,
                    height: this.LegendContentHeight + (cornerHeight + this.Padding) * 2
                );
            }
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
