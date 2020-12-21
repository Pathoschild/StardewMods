using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.UI;
using Pathoschild.Stardew.DataLayers.Framework.Components;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
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
        /// <summary>The pixel margin above the displayed legend.</summary>
        private readonly int TopMargin = 30;

        /// <summary>The pixel margin left of the displayed legend.</summary>
        private readonly int LeftMargin = 15;

        /// <summary>The number of pixels between the arrows and label.</summary>
        private readonly int ArrowPadding = 10;

        /// <summary>Get whether the overlay should be drawn.</summary>
        private readonly Func<bool> DrawOverlay;


        /*****
        ** Layer state
        *****/
        /// <summary>The available data layers.</summary>
        private readonly ILayer[] Layers;

        /// <summary>The legend entries to show.</summary>
        private LegendEntry[] LegendEntries;

        /*****
        ** Grid state
        *****/
        /// <summary>When two groups of the same color overlap, draw one border around their edges instead of their individual borders.</summary>
        private readonly bool CombineOverlappingBorders;

        /// <summary>An empty set of tiles.</summary>
        private readonly Vector2[] EmptyTiles = new Vector2[0];

        /// <summary>An empty set of tile groups.</summary>
        private readonly TileGroup[] EmptyTileGroups = new TileGroup[0];

        /// <summary>The visible tiles.</summary>
        private Vector2[] VisibleTiles;

        /// <summary>The tile layer data to render.</summary>
        private TileGroup[] TileGroups;

        /// <summary>The tick countdown until the next layer update.</summary>
        private int UpdateCountdown;

        /// <summary>Whether to show a tile grid by default.</summary>
        private readonly bool ShowGrid;

        /// <summary>The width of grid lines between tiles, if enabled.</summary>
        private readonly int GridBorderSize = 1;

        /// <summary>The color of grid lines between tiles, if enabled.</summary>
        private readonly Color GridColor = Color.Black;

        /*****
        ** UI state
        *****/
        /// <summary>The last visible area.</summary>
        private Rectangle LastVisibleArea;

        /// <summary>Whether the game was paused last time the menu was updated.</summary>
        private bool WasPaused;

        /*****
        ** Components
        *****/
        /// <summary>The UI component for the layer title and legend.</summary>
        private LegendComponent Legend;

        /// <summary>The clickable 'previous layer' icon.</summary>
        private ClickableTextureComponent PrevButton;

        /// <summary>The clickable 'next layer' icon.</summary>
        private ClickableTextureComponent NextButton;


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
        /// <param name="reflection">Simplifies access to private code.</param>
        /// <param name="layers">The data layers to render.</param>
        /// <param name="drawOverlay">Get whether the overlay should be drawn.</param>
        /// <param name="combineOverlappingBorders">When two groups of the same color overlap, draw one border around their edges instead of their individual borders.</param>
        /// <param name="showGrid">Whether to show a tile grid when a layer is open.</param>
        public DataLayerOverlay(IModEvents events, IInputHelper inputHelper, IReflectionHelper reflection, ILayer[] layers, Func<bool> drawOverlay, bool combineOverlappingBorders, bool showGrid)
            : base(events, inputHelper, reflection)
        {
            if (!layers.Any())
                throw new InvalidOperationException("Can't initialize the data layers overlay with no data layers.");

            this.Layers = layers.OrderBy(p => p.Name).ToArray();
            this.DrawOverlay = drawOverlay;
            this.CombineOverlappingBorders = combineOverlappingBorders;
            this.ShowGrid = showGrid;

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

        /// <summary>Switch to the given data layer.</summary>
        /// <param name="layer">The data layer to select.</param>
        public void SetLayer(ILayer layer)
        {
            this.CurrentLayer = layer;
            this.LegendEntries = this.CurrentLayer.Legend.ToArray();
            this.TileGroups = this.EmptyTileGroups;
            this.UpdateCountdown = 0;

            this.ReinitializeComponents();
        }

        /// <summary>Update the overlay.</summary>
        public void Update()
        {
            // move UI if it overlaps pause message
            if (this.WasPaused != Game1.HostPaused)
            {
                this.WasPaused = Game1.HostPaused;
                this.ReinitializeComponents();
            }

            // get updated tiles
            if (Game1.currentLocation == null || this.CurrentLayer == null)
            {
                this.VisibleTiles = this.EmptyTiles;
                this.TileGroups = this.EmptyTileGroups;
            }
            else
            {
                Rectangle visibleArea = TileHelper.GetVisibleArea(expand: 1);
                if (--this.UpdateCountdown <= 0 || (this.CurrentLayer.UpdateWhenVisibleTilesChange && visibleArea != this.LastVisibleArea))
                {
                    GameLocation location = Game1.currentLocation;
                    Vector2 cursorTile = TileHelper.GetTileFromCursor();
                    this.VisibleTiles = visibleArea.GetTiles().ToArray();
                    this.TileGroups = this.CurrentLayer.Update(location, visibleArea, this.VisibleTiles, cursorTile).ToArray();
                    this.LastVisibleArea = visibleArea;
                    this.UpdateCountdown = this.CurrentLayer.UpdateTickRate;
                }
            }
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>The method invoked when the cursor is hovered.</summary>
        /// <param name="x">The cursor's X position.</param>
        /// <param name="y">The cursor's Y position.</param>
        /// <returns>Whether the event has been handled and shouldn't be propagated further.</returns>
        protected override bool ReceiveCursorHover(int x, int y)
        {
            this.PrevButton.tryHover(x, y);
            this.NextButton.tryHover(x, y);

            return this.PrevButton.containsPoint(x, y) || this.NextButton.containsPoint(x, y);
        }

        /// <summary>The method invoked when the player left-clicks.</summary>
        /// <param name="x">The X-position of the cursor.</param>
        /// <param name="y">The Y-position of the cursor.</param>
        /// <returns>Whether the event has been handled and shouldn't be propagated further.</returns>
        protected override bool ReceiveLeftClick(int x, int y)
        {
            if (this.PrevButton.containsPoint(x, y))
            {
                this.PrevLayer();
                return true;
            }

            if (this.NextButton.containsPoint(x, y))
            {
                this.NextLayer();
                return true;
            }

            return base.ReceiveLeftClick(x, y);
        }

        /// <summary>Draw the overlay to the screen under the UI.</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        protected override void DrawWorld(SpriteBatch spriteBatch)
        {
            if (!this.DrawOverlay())
                return;

            int tileSize = Game1.tileSize;
            const int borderSize = 4;
            IDictionary<Vector2, TileDrawData> tiles = this.AggregateTileData(this.TileGroups, this.CombineOverlappingBorders);

            foreach (Vector2 tilePos in this.VisibleTiles)
            {
                Vector2 pixelPosition = tilePos * tileSize - new Vector2(Game1.viewport.X, Game1.viewport.Y);

                // draw tile data
                bool hasLeftBorder = false, hasRightBorder = false, hasTopBorder = false, hasBottomBorder = false;
                int gridSize = this.ShowGrid || this.CurrentLayer.AlwaysShowGrid ? this.GridBorderSize : 0;
                if (tiles.TryGetValue(tilePos, out TileDrawData tile))
                {
                    // draw overlay
                    foreach (Color color in tile.Colors)
                        spriteBatch.Draw(CommonHelper.Pixel, new Rectangle((int)pixelPosition.X, (int)pixelPosition.Y, tileSize, tileSize), color * .3f);

                    // draw group borders
                    foreach (Color color in tile.BorderColors.Keys)
                    {
                        TileEdge edges = tile.BorderColors[color];

                        int leftBorderSize = edges.HasFlag(TileEdge.Left) ? borderSize : gridSize;
                        int rightBorderSize = edges.HasFlag(TileEdge.Right) ? borderSize : gridSize;
                        int topBorderSize = edges.HasFlag(TileEdge.Top) ? borderSize : gridSize;
                        int bottomBorderSize = edges.HasFlag(TileEdge.Bottom) ? borderSize : gridSize;

                        hasLeftBorder = this.DrawBorder(spriteBatch, pixelPosition, TileEdge.Left, color, leftBorderSize);
                        hasRightBorder = this.DrawBorder(spriteBatch, pixelPosition, TileEdge.Right, color, rightBorderSize);
                        hasTopBorder = this.DrawBorder(spriteBatch, pixelPosition, TileEdge.Top, color, topBorderSize);
                        hasBottomBorder = this.DrawBorder(spriteBatch, pixelPosition, TileEdge.Bottom, color, bottomBorderSize);
                    }
                }

                // draw grid
                if (gridSize > 0)
                {
                    Color color = (tile?.Colors.First() ?? this.GridColor) * 0.5f;
                    int width = this.GridBorderSize;

                    if (!hasLeftBorder)
                        this.DrawBorder(spriteBatch, pixelPosition, TileEdge.Left, color, width);
                    if (!hasRightBorder)
                        this.DrawBorder(spriteBatch, pixelPosition, TileEdge.Right, color, width);
                    if (!hasTopBorder)
                        this.DrawBorder(spriteBatch, pixelPosition, TileEdge.Top, color, width);
                    if (!hasBottomBorder)
                        this.DrawBorder(spriteBatch, pixelPosition, TileEdge.Bottom, color, width);
                }
            }

            // draw top-left UI
            this.Legend.Draw(spriteBatch);
            this.PrevButton.draw(spriteBatch);
            this.NextButton.draw(spriteBatch);
        }

        /// <summary>Reinitialize the UI components.</summary>
        private void ReinitializeComponents()
        {
            // move UI to avoid covering 'paused' message (which has a hardcoded size)
            int topMargin = this.TopMargin;
            if (Game1.HostPaused)
                topMargin += 96;

            // init UI
            var leftArrow = CommonSprites.Icons.LeftArrow;
            var rightArrow = CommonSprites.Icons.RightArrow;

            this.PrevButton = new ClickableTextureComponent(new Rectangle(this.LeftMargin, this.TopMargin + 10, leftArrow.Width, leftArrow.Height), CommonSprites.Icons.Sheet, leftArrow, 1);
            this.Legend = new LegendComponent(this.PrevButton.bounds.Right + this.ArrowPadding, topMargin, this.Layers, this.CurrentLayer.Name, this.LegendEntries);
            this.NextButton = new ClickableTextureComponent(new Rectangle(this.Legend.bounds.Right + this.ArrowPadding, this.TopMargin + 10, rightArrow.Width, rightArrow.Height), CommonSprites.Icons.Sheet, rightArrow, 1);
        }

        /// <summary>Draw a tile border.</summary>
        /// <param name="spriteBatch">The sprite batch to which to draw.</param>
        /// <param name="origin">The top-left pixel position of the tile relative to the screen.</param>
        /// <param name="edge">The tile edge to draw.</param>
        /// <param name="color">The border color.</param>
        /// <param name="width">The border width.</param>
        /// <returns>Returns whether a border was drawn. This may return false if the width is zero, or the edge is invalid.</returns>
        private bool DrawBorder(SpriteBatch spriteBatch, Vector2 origin, TileEdge edge, Color color, int width)
        {
            if (width <= 0)
                return false;

            switch (edge)
            {
                case TileEdge.Left:
                    spriteBatch.Draw(CommonHelper.Pixel, new Rectangle((int)origin.X, (int)origin.Y, width, Game1.tileSize), color);
                    return true;

                case TileEdge.Right:
                    spriteBatch.Draw(CommonHelper.Pixel, new Rectangle((int)(origin.X + Game1.tileSize - width), (int)origin.Y, width, Game1.tileSize), color);
                    return true;

                case TileEdge.Top:
                    spriteBatch.Draw(CommonHelper.Pixel, new Rectangle((int)origin.X, (int)origin.Y, Game1.tileSize, width), color);
                    return true;

                case TileEdge.Bottom:
                    spriteBatch.Draw(CommonHelper.Pixel, new Rectangle((int)origin.X, (int)(origin.Y + Game1.tileSize - width), Game1.tileSize, width), color);
                    return true;
            }

            return false;
        }

        /// <summary>Aggregate tile data to draw.</summary>
        /// <param name="groups">The tile groups to draw.</param>
        /// <param name="combineOverlappingBorders">When two groups of the same color overlap, draw one border around their edges instead of their individual borders.</param>
        private IDictionary<Vector2, TileDrawData> AggregateTileData(IEnumerable<TileGroup> groups, bool combineOverlappingBorders)
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

            return tiles;
        }
    }
}
