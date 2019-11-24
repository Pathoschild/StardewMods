using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.UI;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using XRectangle = xTile.Dimensions.Rectangle;

namespace Pathoschild.Stardew.DataLayers.Framework
{
    /// <summary>Renders a data layer over the world.</summary>
    internal class DataLayerOverlay : BaseOverlay
    {
        /*********
        ** Fields
        *********/
        /*****
        ** Constants and config
        *****/
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

        /// <summary>Get whether the overlay should be drawn.</summary>
        private readonly Func<bool> DrawOverlay;

        /*****
        ** State
        *****/
        /// <summary>The available data layers.</summary>
        private readonly ILayer[] Layers;

        /// <summary>When two groups of the same color overlap, draw one border around their edges instead of their individual borders.</summary>
        private readonly bool CombineOverlappingBorders;

        /// <summary>An empty set of tile groups.</summary>
        private readonly TileGroup[] EmptyTileGroups = new TileGroup[0];

        /// <summary>The legend entries to show.</summary>
        private LegendEntry[] Legend;

        /// <summary>The tiles to render.</summary>
        private TileGroup[] TileGroups;

        /// <summary>The tick countdown until the next layer update.</summary>
        private int UpdateCountdown;

        /// <summary>The last visible area.</summary>
        private Rectangle LastVisibleArea;


        /*********
        ** Accessors
        *********/
        /// <summary>The current layer being rendered.</summary>
        public ILayer CurrentLayer { get; private set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="events">The SMAPI events available for mods.</param>
        /// <param name="inputHelper">An API for checking and changing input state.</param>
        /// <param name="layers">The data layers to render.</param>
        /// <param name="drawOverlay">Get whether the overlay should be drawn.</param>
        /// <param name="combineOverlappingBorders">When two groups of the same color overlap, draw one border around their edges instead of their individual borders.</param>
        public DataLayerOverlay(IModEvents events, IInputHelper inputHelper, ILayer[] layers, Func<bool> drawOverlay, bool combineOverlappingBorders)
            : base(events, inputHelper)
        {
            if (!layers.Any())
                throw new InvalidOperationException("Can't initialize the data layers overlay with no data layers.");

            this.Layers = layers.OrderBy(p => p.Name).ToArray();
            this.DrawOverlay = drawOverlay;
            this.LegendColorSize = (int)Game1.smallFont.MeasureString("X").Y;
            this.BoxContentWidth = this.GetMaxContentWidth(this.Layers, this.LegendColorSize);
            this.CombineOverlappingBorders = combineOverlappingBorders;
            this.SetLayer(this.Layers.First());
        }

        /// <summary>Switch to the next data layer.</summary>
        public void NextLayer()
        {
            int index = Array.IndexOf(this.Layers, this.CurrentLayer) + 1;
            if (index >= this.Layers.Length)
                index = 0;
            this.SetLayer(this.Layers[index]);
        }

        /// <summary>Switch to the previous data layer.</summary>
        public void PrevLayer()
        {
            int index = Array.IndexOf(this.Layers, this.CurrentLayer) - 1;
            if (index < 0)
                index = this.Layers.Length - 1;
            this.SetLayer(this.Layers[index]);
        }

        /// <summary>Update the overlay.</summary>
        public void Update()
        {
            // no tiles to draw
            if (Game1.currentLocation == null || this.CurrentLayer == null)
            {
                this.TileGroups = this.EmptyTileGroups;
                return;
            }

            // get updated tiles
            Rectangle visibleArea = this.GetVisibleTileArea(Game1.viewport);
            if (--this.UpdateCountdown <= 0 || (this.CurrentLayer.UpdateWhenVisibleTilesChange && visibleArea != this.LastVisibleArea))
            {
                GameLocation location = Game1.currentLocation;
                Vector2 cursorTile = TileHelper.GetTileFromCursor();
                this.TileGroups = this.CurrentLayer.Update(location, visibleArea, cursorTile).ToArray();
                this.LastVisibleArea = visibleArea;
                this.UpdateCountdown = this.CurrentLayer.UpdateTickRate;
            }
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Draw to the screen.</summary>
        /// <param name="spriteBatch">The sprite batch to which to draw.</param>
        protected override void Draw(SpriteBatch spriteBatch)
        {
            if (!this.DrawOverlay())
                return;

            // collect tile details
            TileDrawData[] tiles = this.AggregateTileData(this.TileGroups, this.CombineOverlappingBorders).ToArray();

            // draw
            int tileSize = Game1.tileSize;
            const int borderSize = 4;
            foreach (TileDrawData tile in tiles)
            {
                Vector2 pixelPosition = tile.TilePosition * tileSize - new Vector2(Game1.viewport.X, Game1.viewport.Y);

                // overlay
                foreach (Color color in tile.Colors)
                    spriteBatch.Draw(CommonHelper.Pixel, new Rectangle((int)pixelPosition.X, (int)pixelPosition.Y, tileSize, tileSize), color * .3f);

                // borders
                foreach (Color color in tile.BorderColors.Keys)
                {
                    TileEdge edges = tile.BorderColors[color];
                    if (edges.HasFlag(TileEdge.Left))
                        spriteBatch.Draw(CommonHelper.Pixel, new Rectangle((int)pixelPosition.X, (int)pixelPosition.Y, borderSize, tileSize), color);
                    if (edges.HasFlag(TileEdge.Right))
                        spriteBatch.Draw(CommonHelper.Pixel, new Rectangle((int)(pixelPosition.X + tileSize - borderSize), (int)pixelPosition.Y, borderSize, tileSize), color);
                    if (edges.HasFlag(TileEdge.Top))
                        spriteBatch.Draw(CommonHelper.Pixel, new Rectangle((int)pixelPosition.X, (int)pixelPosition.Y, tileSize, borderSize), color);
                    if (edges.HasFlag(TileEdge.Bottom))
                        spriteBatch.Draw(CommonHelper.Pixel, new Rectangle((int)pixelPosition.X, (int)(pixelPosition.Y + tileSize - borderSize), tileSize, borderSize), color);
                }
            }

            // draw top-left boxes
            {
                // calculate dimensions
                int topOffset = this.Margin;
                if (Game1.HostPaused)
                    topOffset += 96; // don't cover the 'paused' message (which has a hardcoded size)

                int leftOffset = this.Margin;

                // draw overlay label
                {
                    Vector2 labelSize = Game1.smallFont.MeasureString(this.CurrentLayer.Name);
                    CommonHelper.DrawScroll(spriteBatch, new Vector2(leftOffset, topOffset), new Vector2(this.BoxContentWidth, labelSize.Y), out Vector2 contentPos, out Rectangle bounds);

                    contentPos = contentPos + new Vector2((this.BoxContentWidth - labelSize.X) / 2, 0); // center label in box
                    spriteBatch.DrawString(Game1.smallFont, this.CurrentLayer.Name, contentPos, Color.Black);

                    topOffset += bounds.Height + this.Padding;
                }

                // draw legend
                if (this.Legend.Any())
                {
                    CommonHelper.DrawScroll(spriteBatch, new Vector2(leftOffset, topOffset), new Vector2(this.BoxContentWidth, this.Legend.Length * this.LegendColorSize), out Vector2 contentPos, out Rectangle _);
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

        /// <summary>Aggregate tile data to draw.</summary>
        /// <param name="groups">The tile groups to draw.</param>
        /// <param name="combineOverlappingBorders">When two groups of the same color overlap, draw one border around their edges instead of their individual borders.</param>
        private IEnumerable<TileDrawData> AggregateTileData(IEnumerable<TileGroup> groups, bool combineOverlappingBorders)
        {
            // collect tile details
            IDictionary<Vector2, TileDrawData> tiles = new Dictionary<Vector2, TileDrawData>();
            foreach (TileGroup group in groups)
            {
                Lazy<HashSet<Vector2>> inGroupLazy = new Lazy<HashSet<Vector2>>(() => new HashSet<Vector2>(group.Tiles.Select(p => p.TilePosition)));
                foreach (TileData groupTile in group.Tiles)
                {
                    // get tile data
                    Vector2 position = groupTile.TilePosition;
                    if (!tiles.TryGetValue(position, out TileDrawData data))
                        data = tiles[position] = new TileDrawData(position);

                    // update data
                    data.Colors.Add(groupTile.Color);
                    if (group.OuterBorderColor.HasValue && !data.BorderColors.ContainsKey(group.OuterBorderColor.Value))
                        data.BorderColors[group.OuterBorderColor.Value] = TileEdge.None; // we'll detect combined borders next

                    // detect borders (if not combined)
                    if (!combineOverlappingBorders && group.OuterBorderColor.HasValue)
                    {
                        Color borderColor = group.OuterBorderColor.Value;
                        int x = (int)groupTile.TilePosition.X;
                        int y = (int)groupTile.TilePosition.Y;
                        HashSet<Vector2> inGroup = inGroupLazy.Value;

                        TileEdge edge = data.BorderColors[borderColor];
                        if (!inGroup.Contains(new Vector2(x - 1, y)))
                            edge |= TileEdge.Left;
                        if (!inGroup.Contains(new Vector2(x + 1, y)))
                            edge |= TileEdge.Right;
                        if (!inGroup.Contains(new Vector2(x, y - 1)))
                            edge |= TileEdge.Top;
                        if (!inGroup.Contains(new Vector2(x, y + 1)))
                            edge |= TileEdge.Bottom;
                        data.BorderColors[borderColor] = edge;
                    }
                }
            }

            // detect color borders
            if (combineOverlappingBorders)
            {
                foreach (Vector2 position in tiles.Keys)
                {
                    // get tile
                    int x = (int)position.X;
                    int y = (int)position.Y;
                    TileDrawData data = tiles[position];
                    if (!data.BorderColors.Any())
                        continue;

                    // get neighbors
                    tiles.TryGetValue(new Vector2(x - 1, y), out TileDrawData left);
                    tiles.TryGetValue(new Vector2(x + 1, y), out TileDrawData right);
                    tiles.TryGetValue(new Vector2(x, y - 1), out TileDrawData top);
                    tiles.TryGetValue(new Vector2(x, y + 1), out TileDrawData bottom);

                    // detect edges
                    foreach (Color color in data.BorderColors.Keys.ToArray())
                    {
                        if (left == null || !left.BorderColors.ContainsKey(color))
                            data.BorderColors[color] |= TileEdge.Left;
                        if (right == null || !right.BorderColors.ContainsKey(color))
                            data.BorderColors[color] |= TileEdge.Right;
                        if (top == null || !top.BorderColors.ContainsKey(color))
                            data.BorderColors[color] |= TileEdge.Top;
                        if (bottom == null || !bottom.BorderColors.ContainsKey(color))
                            data.BorderColors[color] |= TileEdge.Bottom;
                    }
                }
            }

            return tiles.Values;
        }

        /// <summary>Switch to the given data layer.</summary>
        /// <param name="layer">The data layer to select.</param>
        private void SetLayer(ILayer layer)
        {
            this.CurrentLayer = layer;
            this.Legend = this.CurrentLayer.Legend.ToArray();
            this.TileGroups = this.EmptyTileGroups;
            this.UpdateCountdown = 0;
        }

        /// <summary>Get the tile area currently visible to the player.</summary>
        /// <param name="viewport">The game viewport.</param>
        private Rectangle GetVisibleTileArea(XRectangle viewport)
        {
            int tileSize = Game1.tileSize;
            int left = viewport.X / tileSize;
            int top = viewport.Y / tileSize;
            int width = (int)Math.Ceiling(viewport.Width / (decimal)tileSize);
            int height = (int)Math.Ceiling(viewport.Height / (decimal)tileSize);

            return new Rectangle(left - 1, top - 1, width + 2, height + 2); // extend slightly off-screen to avoid tile pop-in at the edges
        }

        /// <summary>Get the maximum content width needed to render the layer labels and legends.</summary>
        /// <param name="layers">The data layers to render.</param>
        /// <param name="legendColorSize">The pixel size of a color box in the legend.</param>
        private int GetMaxContentWidth(ILayer[] layers, int legendColorSize)
        {
            float labelWidth =
                (
                    from layer in layers
                    select Game1.smallFont.MeasureString(layer.Name).X
                )
                .Max();
            float legendContentWidth =
                (
                    from layer in layers
                    from entry in layer.Legend
                    select Game1.smallFont.MeasureString(entry.Name).X
                )
                .Max() + legendColorSize + this.LegendColorPadding;

            return (int)Math.Max(labelWidth, legendContentWidth);
        }
    }
}
