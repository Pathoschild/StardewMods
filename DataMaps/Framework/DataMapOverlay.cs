using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;
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
        /// <summary>The pixel padding between the color box and its label.</summary>
        private readonly int LegendColorPadding = 5;

        /// <summary>The size of the margin around the displayed legend.</summary>
        private readonly int Margin = 30;

        /// <summary>The padding between the border and content.</summary>
        private readonly int Padding = 5;

        /// <summary>The pixel size of a color box in the legend.</summary>
        private readonly int LegendColorSize;

        /// <summary>The width of the top-left boxes.</summary>
        private readonly int BoxContentWidth;

        /// <summary>The available data maps.</summary>
        private readonly IDataMap[] Maps;

        /// <summary>The current data map to render.</summary>
        private IDataMap CurrentMap;

        /// <summary>The legend entries to show.</summary>
        private LegendEntry[] Legend;

        /// <summary>The tiles to render.</summary>
        private TileGroup[] TileGroups;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="maps">The data maps to render.</param>
        public DataMapOverlay(IDataMap[] maps)
        {
            if (!maps.Any())
                throw new InvalidOperationException("Can't initialise the data maps overlay with no data maps.");

            this.Maps = maps;
            this.LegendColorSize = (int)Game1.smallFont.MeasureString("X").Y;
            this.BoxContentWidth = this.GetMaxContentWidth(maps, this.LegendColorSize);
            this.SetMap(maps.First());
        }

        /// <summary>Switch to the next data map.</summary>
        public void NextMap()
        {
            int index = Array.IndexOf(this.Maps, this.CurrentMap) + 1;
            if (index >= this.Maps.Length)
                index = 0;
            this.SetMap(this.Maps[index]);
        }

        /// <summary>Switch to the previous data map.</summary>
        public void PrevMap()
        {
            int index = Array.IndexOf(this.Maps, this.CurrentMap) - 1;
            if (index < 0)
                index = this.Maps.Length - 1;
            this.SetMap(this.Maps[index]);
        }

        /// <summary>Update the overlay.</summary>
        public void Update()
        {
            // no tiles to draw
            if (Game1.currentLocation == null || this.CurrentMap == null)
            {
                this.TileGroups = new TileGroup[0];
                return;
            }

            // get updated tiles
            GameLocation location = Game1.currentLocation;
            this.TileGroups = this.CurrentMap.Update(location, this.GetVisibleArea(location, Game1.viewport)).ToArray();
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Draw to the screen.</summary>
        /// <param name="spriteBatch">The sprite batch to which to draw.</param>
        protected override void Draw(SpriteBatch spriteBatch)
        {
            if (!Context.IsPlayerFree)
                return;

            // draw tile overlay
            {
                int tileSize = Game1.tileSize;
                foreach (TileGroup group in this.TileGroups.ToArray())
                {
                    HashSet<Vector2> tileHash = new HashSet<Vector2>(group.Tiles.Select(p => p.TilePosition));
                    foreach (TileData tile in group.Tiles)
                    {
                        Vector2 position = tile.TilePosition * tileSize - new Vector2(Game1.viewport.X, Game1.viewport.Y);

                        // draw tile overlay
                        spriteBatch.Draw(CommonHelper.Pixel, new Rectangle((int)position.X, (int)position.Y, tileSize, tileSize), tile.Color * .3f);

                        // draw borders
                        if (group.OuterBorderColor != null)
                        {
                            const int borderSize = 4;
                            Color borderColor = group.OuterBorderColor.Value * 0.9f;
                            int x = (int)tile.TilePosition.X;
                            int y = (int)tile.TilePosition.Y;

                            // left
                            if (!tileHash.Contains(new Vector2(x - 1, y)))
                                spriteBatch.Draw(CommonHelper.Pixel, new Rectangle((int)position.X, (int)position.Y, borderSize, tileSize), borderColor);

                            // right
                            if (!tileHash.Contains(new Vector2(x + 1, y)))
                                spriteBatch.Draw(CommonHelper.Pixel, new Rectangle((int)(position.X + tileSize - borderSize), (int)position.Y, borderSize, tileSize), borderColor);

                            // top
                            if (!tileHash.Contains(new Vector2(x, y - 1)))
                                spriteBatch.Draw(CommonHelper.Pixel, new Rectangle((int)position.X, (int)position.Y, tileSize, borderSize), borderColor);

                            // bottom
                            if (!tileHash.Contains(new Vector2(x, y + 1)))
                                spriteBatch.Draw(CommonHelper.Pixel, new Rectangle((int)position.X, (int)(position.Y + tileSize - borderSize), tileSize, borderSize), borderColor);
                        }
                    }
                }
            }

            // draw top-left boxes
            {
                // calculate dimensions
                int topOffset = this.Margin;
                int leftOffset = this.Margin;

                // draw overlay label
                {
                    Vector2 labelSize = Game1.smallFont.MeasureString(this.CurrentMap.Name);
                    this.DrawScroll(spriteBatch, leftOffset, topOffset, this.BoxContentWidth, (int)labelSize.Y, out Vector2 contentPos, out Rectangle bounds);

                    contentPos = contentPos + new Vector2((this.BoxContentWidth - labelSize.X) / 2, 0); // center label in box
                    spriteBatch.DrawString(Game1.smallFont, this.CurrentMap.Name, contentPos, Color.Black);

                    topOffset += bounds.Height + this.Padding;
                }

                // draw legend
                if (this.Legend.Any())
                {
                    this.DrawScroll(spriteBatch, leftOffset, topOffset, this.BoxContentWidth, this.Legend.Length * this.LegendColorSize, out Vector2 contentPos, out Rectangle _);
                    for (int i = 0; i < this.Legend.Length; i++)
                    {
                        LegendEntry value = this.Legend[i];
                        int legendX = (int)contentPos.X;
                        int legendY = (int)(contentPos.Y + i * this.LegendColorSize);

                        spriteBatch.DrawLine(legendX, legendY, new Vector2(this.LegendColorSize), value.Color);
                        spriteBatch.DrawString(Game1.smallFont, value.Name, new Vector2(legendX + this.LegendColorSize + this.LegendColorPadding, legendY + 2), Color.Black);
                    }
                }
            }
        }

        /// <summary>Switch to the given data map.</summary>
        /// <param name="map">The data map to select.</param>
        private void SetMap(IDataMap map)
        {
            this.CurrentMap = map;
            this.Legend = this.CurrentMap.Legend.ToArray();
            this.TileGroups = new TileGroup[0];
        }

        /// <summary>Draw a scroll background.</summary>
        /// <param name="spriteBatch">The sprite batch to which to draw.</param>
        /// <param name="x">The top-left X pixel coordinate at which to draw the scroll.</param>
        /// <param name="y">The top-left Y pixel coordinate at which to draw the scroll.</param>
        /// <param name="contentWidth">The scroll content's pixel width.</param>
        /// <param name="contentHeight">The scroll content's pixel height.</param>'
        /// <param name="contentPos">The pixel position at which the content begins.</param>
        /// <param name="bounds">The scroll's outer bounds.</param>
        private void DrawScroll(SpriteBatch spriteBatch, int x, int y, int contentWidth, int contentHeight, out Vector2 contentPos, out Rectangle bounds)
        {
            Rectangle corner = Sprites.Legend.TopLeft;
            int cornerWidth = corner.Width * Game1.pixelZoom;
            int cornerHeight = corner.Height * Game1.pixelZoom;
            int innerWidth = contentWidth + this.Padding * 2;
            int innerHeight = contentHeight + this.Padding * 2;
            int outerWidth = innerWidth + cornerWidth * 2;
            int outerHeight = innerHeight + cornerHeight * 2;

            // draw scroll background
            spriteBatch.Draw(Sprites.Legend.Sheet, new Rectangle(x + cornerWidth, y + cornerHeight, innerWidth, innerHeight), Sprites.Legend.Background, Color.White);

            // draw borders
            spriteBatch.Draw(Sprites.Legend.Sheet, new Rectangle(x + cornerWidth, y, innerWidth, cornerHeight), Sprites.Legend.Top, Color.White);
            spriteBatch.Draw(Sprites.Legend.Sheet, new Rectangle(x + cornerWidth, y + cornerHeight + innerHeight, innerWidth, cornerHeight), Sprites.Legend.Bottom, Color.White);
            spriteBatch.Draw(Sprites.Legend.Sheet, new Rectangle(x, y + cornerHeight, cornerWidth, innerHeight), Sprites.Legend.Left, Color.White);
            spriteBatch.Draw(Sprites.Legend.Sheet, new Rectangle(x + cornerWidth + innerWidth, y + cornerHeight, cornerWidth, innerHeight), Sprites.Legend.Right, Color.White);

            // draw corners
            spriteBatch.Draw(Sprites.Legend.Sheet, new Rectangle(x, y, cornerWidth, cornerHeight), Sprites.Legend.TopLeft, Color.White);
            spriteBatch.Draw(Sprites.Legend.Sheet, new Rectangle(x, y + cornerHeight + innerHeight, cornerWidth, cornerHeight), Sprites.Legend.BottomLeft, Color.White);
            spriteBatch.Draw(Sprites.Legend.Sheet, new Rectangle(x + cornerWidth + innerWidth, y, cornerWidth, cornerHeight), Sprites.Legend.TopRight, Color.White);
            spriteBatch.Draw(Sprites.Legend.Sheet, new Rectangle(x + cornerWidth + innerWidth, y + cornerHeight + innerHeight, cornerWidth, cornerHeight), Sprites.Legend.BottomRight, Color.White);

            // set out params
            contentPos = new Vector2(x + cornerWidth + this.Padding, y + cornerHeight + this.Padding);
            bounds = new Rectangle(x, y, outerWidth, outerHeight);
        }

        /// <summary>Get the tile area currently visible to the player.</summary>
        /// <param name="location">The game location.</param>
        /// <param name="viewport">The game viewport.</param>
        protected Rectangle GetVisibleArea(GameLocation location, XRectangle viewport)
        {
            int tileSize = Game1.tileSize;
            int left = viewport.X / tileSize;
            int top = viewport.Y / tileSize;
            int width = (int)Math.Ceiling(viewport.Width / (decimal)tileSize);
            int height = (int)Math.Ceiling(viewport.Height / (decimal)tileSize);

            return new Rectangle(left - 1, top - 1, width + 2, height + 2); // extend slightly off-screen to avoid tile pop-in at the edges
        }

        /// <summary>Get the maximum content width needed to render the data map labels and legends.</summary>
        /// <param name="maps">The data maps to render.</param>
        /// <param name="legendColorSize">The pixel size of a color box in the legend.</param>
        private int GetMaxContentWidth(IDataMap[] maps, int legendColorSize)
        {
            float labelWidth =
                (
                    from map in maps
                    select Game1.smallFont.MeasureString(map.Name).X
                )
                .Max();
            float legendContentWidth =
                (
                    from map in maps
                    from entry in map.Legend
                    select Game1.smallFont.MeasureString(entry.Name).X
                )
                .Max() + legendColorSize + this.LegendColorPadding;

            return (int)Math.Max(labelWidth, legendContentWidth);
        }
    }
}
